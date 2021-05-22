using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BookmarkManager.Infrastructure.RabbitMQ
{
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

    public static class PayloadExtensions
    {
        public static T Parse<T>(this Payload payload) =>
            JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload.Body));
    }
}