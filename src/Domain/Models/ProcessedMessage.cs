using System;
using System.Linq.Expressions;

namespace BookmarkManager.Domain.Models
{
    public class ProcessedMessage
    {
        public Guid Id { get; set; }
        public string ConsumerName { get; set; }

        // Used by EF
        private ProcessedMessage()
        {
        }

        public ProcessedMessage(Guid id, Type consumerType)
        {
            Id = id;
            ConsumerName = consumerType.FullName;
        }

        public Expression<Func<ProcessedMessage, bool>> IsEqual()
        {
            return x => x.Id == Id && x.ConsumerName == ConsumerName;
        }
    }
}
