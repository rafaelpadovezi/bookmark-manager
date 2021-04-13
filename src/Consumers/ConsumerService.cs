using BookmarkManager.Infrastructure;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class ConsumerService : IHostedService
    {
        private readonly IQueueConsumer _queueConsumer;

        public ConsumerService(IQueueConsumer queueConsumer)
        {
            _queueConsumer = queueConsumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _queueConsumer.Subscribe<BookmarkInsertedConsumer>(
                "bookmark.inserted",
                consumer => consumer.SaveBookmarDetailsAsync);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
