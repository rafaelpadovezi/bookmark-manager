using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using BookmarkManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class BookmarkInsertedConsumer : BackgroundService
    {
        private const string _queue = "bookmark_inserted";
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<RabbitMQConnectionFactory> _logger;

        public BookmarkInsertedConsumer(
            IServiceProvider services,
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<RabbitMQConnectionFactory> logger)
        {
            _serviceProvider = services;
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

            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var bookmark = JsonSerializer.Deserialize<Bookmark>(
                    Encoding.UTF8.GetString(body));

                _logger.LogInformation("Receveid {@message}", bookmark);

                using var scope = _serviceProvider.CreateScope();
                var webpageService = scope.ServiceProvider.GetRequiredService<IWebpageService>();
                var context = scope.ServiceProvider.GetRequiredService<BookmarkManagerContext>();

                var (title, description, imageUrl) = await webpageService.GetPageInformation(bookmark.Url);

                bookmark.Update(title, description, imageUrl);

                context.Update(bookmark);

                using var transaction = await context.Database.BeginTransactionAsync();
                await context.SaveChangesAsync();

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                await transaction.CommitAsync();
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
