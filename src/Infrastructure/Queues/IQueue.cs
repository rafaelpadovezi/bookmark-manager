using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure
{
    public delegate Task ConsumerDelegate(Payload payload);

    public interface IQueueConsumer : IDisposable
    {
        void Subscribe<T>(string queueName, Func<T, ConsumerDelegate> handler);
    }

    public interface IQueueProducer : IDisposable
    {
        void Publish<T>(string queueName, T message);
    }

    public static class PayloadExtensions
    {
        public static T Parse<T>(this Payload payload) =>
            JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload.Body));
    }

    public class Payload
    {
        public byte[] Body { get; }
        private readonly IModel _channel;
        private readonly BasicDeliverEventArgs _ea;

        public Payload(byte[] body, IModel channel, BasicDeliverEventArgs ea)
        {
            Body = body;
            _channel = channel;
            _ea = ea;
        }

        public void Ack()
        {
            _channel.BasicAck(_ea.DeliveryTag, false);
        }

        public void NackAndReQueue()
        {
            _channel.BasicNack(_ea.DeliveryTag, false, true);
        }

        public void NackAndDiscard()
        {
            _channel.BasicNack(_ea.DeliveryTag, false, false);
        }
    }
}