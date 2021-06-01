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
    public class RabbitMqClient : IQueueConsumer, IQueueProducer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqConnection _rabbitMqConnection;
        private readonly ILogger<RabbitMqClient> _logger;

        public RabbitMqClient(
            IServiceProvider serviceProvider,
            RabbitMqConnection rabbitMqConnection,
            ILogger<RabbitMqClient> logger)
        {
            _serviceProvider = serviceProvider;
            _rabbitMqConnection = rabbitMqConnection;
            _logger = logger;
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
            var channel = _rabbitMqConnection.Channel;
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            IBasicProperties props = channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2; // persistent
            if (!string.IsNullOrEmpty(activityId))
            {
                props.Headers = new Dictionary<string, object>
                {
                    { "traceparent", activityId }
                };
            }

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: props,
                                 body: body);
        }

        private static Activity StartActivity(string queueName, BasicDeliverEventArgs ea)
        {
            var activity = new Activity($"{queueName}.Consumer");

            object traceparent = null;
            var hasTraceparent =
                ea.BasicProperties.Headers != null &&
                ea.BasicProperties.Headers.TryGetValue(
                    Constants.TraceParentHeaderName, out traceparent);
            if (hasTraceparent && traceparent is byte[] bytes)
            {
                var traceparentString = Encoding.UTF8.GetString(bytes);
                activity.SetParentId(traceparentString);
            }

            activity.Start();
            return activity;
        }

        public void Subscribe<T>(string queueName, Func<T, ConsumerDelegate> handler)
        {
            var channel = _rabbitMqConnection.Channel;
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (sender, ea) =>
            {
                Activity activity = StartActivity(queueName, ea);

                var payload = new Payload(ea.Body.ToArray(), channel, ea);

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

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }
    }
}
