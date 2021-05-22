using BookmarkManager.Domain.Models;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure.RabbitMQ
{
    public delegate Task ConsumerDelegate(Payload payload);

    public interface IQueueConsumer : IDisposable
    {
        void Subscribe<T>(string queueName, Func<T, ConsumerDelegate> handler);
    }

    public interface IQueueProducer : IDisposable
    {
        void Publish<T>(string queueName, T message);
        void Publish(OutboxMessage outboxMessage);
    }
}