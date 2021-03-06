using System;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public interface IConsumer<TMessage>
    {
        Task ExecuteAsync(TMessage message, Action ack);
    }
}
