using BookmarkManager.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class BookmarkInsertedConsumer : BackgroundService
    {
        private const string _queue = "bookmark_inserted";
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<RabbitMQConnectionFactory> _logger;

        public BookmarkInsertedConsumer(
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<RabbitMQConnectionFactory> logger)
        {
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = _rabbitMQConnectionFactory.Connection.Value;
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: _queue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Receveid {message}", message);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: _queue,
                                 autoAck: false,
                                 consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
