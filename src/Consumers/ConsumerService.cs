using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class ConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(
            IServiceProvider services,
            ILogger<ConsumerService> logger)
        {
            _serviceProvider = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bookmarkInsertedQueue = _serviceProvider.GetRequiredService<IQueue<BookmarkInserted>>();

            bookmarkInsertedQueue.Subscribe(async (message, ack) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<BookmarkInserted>>();
                await consumer.ExecuteAsync(message, ack);
            });

            _logger.LogInformation("Subscribed to queue");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
