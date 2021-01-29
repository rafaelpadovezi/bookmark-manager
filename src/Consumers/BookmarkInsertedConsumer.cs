using BookmarkManager.Infrastructure;
using BookmarkManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class BookmarkInsertedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBookmarkInsertedQueue _bookmarkInsertedQueue;
        private readonly ILogger<RabbitMQConnectionFactory> _logger;

        public BookmarkInsertedConsumer(
            IServiceProvider services,
            IBookmarkInsertedQueue bookmarkInsertedQueue,
            ILogger<RabbitMQConnectionFactory> logger)
        {
            _serviceProvider = services;
            _bookmarkInsertedQueue = bookmarkInsertedQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _bookmarkInsertedQueue.Subscribe(async (channel, eventArgs, bookmark) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var (context, webpageService) = GetRequiredServices(scope);

                var (title, description, imageUrl) = await webpageService.GetPageInformation(bookmark.Url);

                bookmark.Update(title, description, imageUrl);

                context.Update(bookmark);

                using var transaction = await context.Database.BeginTransactionAsync();
                await context.SaveChangesAsync();

                channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                await transaction.CommitAsync();
            });

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
