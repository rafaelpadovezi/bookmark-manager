using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Constants = BookmarkManager.Utils.Constants;

namespace BookmarkManager.Infrastructure
{
    public class QueueClient : IQueueConsumer, IQueueProducer
    {
        private readonly string _queueName;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<QueueClient> _logger;
        private readonly IModel _channel;
        private bool _disposedValue;

        public QueueClient(
            IServiceProvider serviceProvider,
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<QueueClient> logger)
        {
            _serviceProvider = serviceProvider;
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;

            var connection = _rabbitMQConnectionFactory.Connection.Value;
            _channel = connection.CreateModel();
        }

        public void Publish<T>(string queueName, T message)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            IBasicProperties props = _channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2;
            props.Headers = new Dictionary<string, object>
            {
                { "traceparent", Activity.Current.Id }
            };

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: props,
                                 body: body);

            _logger.LogInformation("Sent {message}", message);
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

        public void Subscribe<T>(string queueName, Func<T, ConsumerDelegate> handler)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                Activity activity = StartActivity(ea);

                var payload = new Payload(ea.Body.ToArray(), _channel, ea);

                _logger.LogInformation("Received message from {queue}", queueName);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<T>();
                    await handler(service)(payload);
                }
                catch (Exception ex)
                {
                    payload.NackAndDiscard();
                    _logger.LogError(ex, "Error processing message");
                }
                finally
                {
                    activity.Stop();
                }
            };

            _channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }
    }
}
