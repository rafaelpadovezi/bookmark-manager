using BookmarkManager.Infrastructure.RabbitMQ;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure.Consumer
{
    public class ConsumerService : IHostedService
    {
        private readonly IQueueConsumer _queueConsumer;
        private readonly Action<IQueueConsumer> _startConsumer;

        public ConsumerService(IQueueConsumer queueConsumer, Action<IQueueConsumer> startConsumer)
        {
            _queueConsumer = queueConsumer;
            _startConsumer = startConsumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _startConsumer(_queueConsumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
