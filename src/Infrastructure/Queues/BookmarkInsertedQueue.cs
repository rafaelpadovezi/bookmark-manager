using BookmarkManager.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure
{

    public sealed class BookmarkInsertedQueue : IBookmarkInsertedQueue
    {
        private const string _queueName = "bookmark_inserted";
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<BookmarkInsertedQueue> _logger;
        private readonly IModel _channel;

        public BookmarkInsertedQueue(
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<BookmarkInsertedQueue> logger)
        {
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;

            var connection = _rabbitMQConnectionFactory.Connection.Value;
            _channel = connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public void Publish(Bookmark bookmark)
        {
            var message = new { bookmark.Id, bookmark.Url };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
            _logger.LogInformation("Sent {message}", message);
        }

        public void Subscribe(Func<IModel, BasicDeliverEventArgs, Bookmark, Task> func)
        {
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var bookmark = JsonSerializer.Deserialize<Bookmark>(
                    Encoding.UTF8.GetString(body));

                _logger.LogInformation("Receveid {@message}", bookmark);

                await func(_channel, ea, bookmark);
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
