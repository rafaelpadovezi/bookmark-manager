using BookmarkManager.Domain.Models;
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

namespace BookmarkManager.Infrastructure.RabbitMQ
{
    public class RabbitMQClient : IQueueConsumer, IQueueProducer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<RabbitMQClient> _logger;
        private readonly IModel _channel;
        private bool _disposedValue;

        public RabbitMQClient(
            IServiceProvider serviceProvider,
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<RabbitMQClient> logger)
        {
            _serviceProvider = serviceProvider;
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;

            var connection = _rabbitMQConnectionFactory.Connection;
            // Closing and opening new channels per operation is usually
            // unnecessary but can be appropriate.
            // https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan
            _channel = connection.CreateModel();
        }

        public void Publish(OutboxMessage outboxMessage)
        {
            var body = Encoding.UTF8.GetBytes(outboxMessage.Payload);

            Publish(outboxMessage.QueueName, body, outboxMessage.ActivityId);

            _logger.LogInformation("Sent {message}", outboxMessage.Type);
            _logger.LogDebug("Message content: {content}", outboxMessage.Payload);
        }

        public void Publish<T>(string queueName, T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            Publish(queueName, body, Activity.Current?.Id);

            _logger.LogInformation("Sent {message}", message);
        }

        private void Publish(string queueName, byte[] body, string activityId)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            IBasicProperties props = _channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2; // persistent
            if (!string.IsNullOrEmpty(activityId))
            {
                props.Headers = new Dictionary<string, object>
                {
                    { "traceparent", activityId }
                };
            }

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: props,
                                 body: body);
        }

        private static Activity StartActivity(string queueName, BasicDeliverEventArgs ea)
        {
            var activity = new Activity($"{queueName}.Consumer");
            if (ea.BasicProperties.Headers.TryGetValue(
                Constants.TraceParentHeaderName, out var traceparent))
            {
                var traceparentString = Encoding.UTF8.GetString(traceparent as byte[]);
                activity.SetParentId(traceparentString);
            }
            activity.Start();
            return activity;
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
                Activity activity = StartActivity(queueName, ea);

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
