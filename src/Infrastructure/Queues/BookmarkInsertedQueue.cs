using BookmarkManager.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BookmarkManager.Infrastructure
{

    public class BookmarkInsertedQueue : IBookmarkInsertedQueue
    {
        private const string QueueName = "bookmark_inserted";
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<BookmarkInsertedQueue> _logger;

        public BookmarkInsertedQueue(
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<BookmarkInsertedQueue> logger)
        {
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;
        }

        public void Publish(Bookmark bookmark)
        {
            var connection = _rabbitMQConnectionFactory.Connection.Value;
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = new { bookmark.Id, bookmark.Url };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(exchange: "",
                                 routingKey: QueueName,
                                 basicProperties: null,
                                 body: body);
            _logger.LogInformation("Sent {message}", message);
        }
    }
}
