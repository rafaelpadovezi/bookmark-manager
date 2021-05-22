using System;
using System.Diagnostics;
using System.Text.Json;

namespace BookmarkManager.Domain.Models
{
    public enum OutboxMessageStatus
    {
        ReadyToSend,
        Sent
    }

    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string QueueName { get; private set; }
        public string Type { get; private set; }
        public string Payload { get; private set; }
        public string ActivityId { get; set; }
        public DateTime CreationDate { get; private set; }
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.ReadyToSend;

        // Used by EF
        private OutboxMessage()
        {
        }

        public OutboxMessage(string queueName, object message)
        {
            QueueName = queueName;
            Type = message.GetType().FullName + ", " + message.GetType().Assembly.GetName().Name;
            Payload = JsonSerializer.Serialize(message);
            ActivityId = Activity.Current?.Id;
            CreationDate = DateTime.Now;
        }
    }
}
