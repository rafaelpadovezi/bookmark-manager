using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using BookmarkManager.Services;
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
            var queues = _serviceProvider.GetServices<IQueue<>>();

            foreach (var queue in queues)
            {
                queue.Subscribe(async (message, ack) =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<Bookmark>>();
                    await consumer.ExecuteAsync(message, ack);
                });
            }

            _logger.LogInformation("Subscribed to queue");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }

        private static (BookmarkManagerContext, IWebpageService) GetRequiredServices(IServiceScope scope)
        {
            return (
                scope.ServiceProvider.GetRequiredService<BookmarkManagerContext>(),
                scope.ServiceProvider.GetRequiredService<IWebpageService>());
        }
    }
}
