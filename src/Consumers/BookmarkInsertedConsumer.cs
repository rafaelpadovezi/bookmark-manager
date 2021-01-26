using BookmarkManager.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class BookmarkInsertedConsumer : BackgroundService
    {
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<RabbitMQConnectionFactory> _logger;

        public BookmarkInsertedConsumer(
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<RabbitMQConnectionFactory> logger)
        {
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;
        }

        public void Subscribe()
        {
            var connection = _rabbitMQConnectionFactory.Connection.Value;
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "bookmark_inserted",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Receveid {message}", message);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
