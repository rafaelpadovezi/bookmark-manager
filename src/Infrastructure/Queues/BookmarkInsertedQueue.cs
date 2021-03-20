using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Constants = BookmarkManager.Utils.Constants;

namespace BookmarkManager.Infrastructure
{
    public class Queue<T> : IQueue<T>
    {
        private readonly string _queueName;
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<Queue<T>> _logger;
        private readonly IModel _channel;
        private bool _disposedValue;

        public Queue(
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<Queue<T>> logger,
            string queueName)
        {
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;
            _queueName = queueName;

            var connection = _rabbitMQConnectionFactory.Connection.Value;
            _channel = connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public async Task RunInTransaction(Func<Task> task)
        {
            _channel.TxSelect();
            try
            {
                await task();
                _channel.TxCommit();
                _logger.LogDebug("Broker tx commited");
            }
            catch
            {
                _logger.LogWarning("Broker tx is being rollbacked after exception");
                _channel.TxRollback();
                throw;
            }
        }

        public void Publish(T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            IBasicProperties props = _channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2;
            props.Headers = new Dictionary<string, object>
            {
                { "traceparent", Activity.Current.Id }
            };

            _channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: props,
                                 body: body);

            _logger.LogInformation("Sent {message}", message);
        }

        public void Subscribe(Func<T, Action, Task> func)
        {
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                Activity activity = StartActivity(ea);

                var message = JsonSerializer.Deserialize<T>(
                    Encoding.UTF8.GetString(ea.Body.ToArray()));

                _logger.LogInformation("Received message from {queue}", _queueName);

                void ackAction() => _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                try
                {
                    await func(message, ackAction);
                }
                catch (Exception ex)
                {
                    _channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
                    _logger.LogError(ex, "Error processing message");
                    throw;
                }
                finally
                {
                    activity.Stop();
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        private Activity StartActivity(BasicDeliverEventArgs ea)
        {
            var activity = new Activity($"{_queueName}.Consumer");
            if (ea.BasicProperties.Headers.TryGetValue(
                Constants.TraceParentHeaderName, out var traceparent))
            {
                var traceparentString = Encoding.UTF8.GetString(traceparent as byte[]);
                activity.SetParentId(traceparentString);
            }
            activity.Start();
            return activity;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channel?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
