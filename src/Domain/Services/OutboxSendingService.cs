using BookmarkManager.Domain.Models;
using BookmarkManager.Infrastructure.DbContexts;
using BookmarkManager.Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Domain.Services
{
    public sealed class OutboxSendingService : IHostedService, IDisposable
    {
        private readonly ILogger<OutboxSendingService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public OutboxSendingService(
            ILogger<OutboxSendingService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Outbox Sending Service is starting.");

            _timer = new Timer(Process, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        public void Process(object state)
        {
            _ = SendMessagesAsync();
        }

        public async Task SendMessagesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BookmarkManagerContext>();
            var producer = scope.ServiceProvider.GetRequiredService<IQueueProducer>();

            var messagesToSend = await context.OutboxMessages
                .Where(x => x.Status == OutboxMessageStatus.ReadyToSend)
                .OrderBy(x => x.CreationDate)
                .Take(50) // fetch a limited number of messages for each run
                .ToListAsync();

            if (!messagesToSend.Any())
                return;

            foreach (var message in messagesToSend)
            {
                producer.Publish(message);
                message.Status = OutboxMessageStatus.Sent;
                await context.SaveChangesAsync();
            }

            _logger.LogInformation("{count} messages sent", messagesToSend.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Outbox Sending Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
