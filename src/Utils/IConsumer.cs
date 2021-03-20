using System;
using System.Threading.Tasks;

namespace BookmarkManager.Utils
{
    public interface IConsumer<TMessage>
    {
        Task ExecuteAsync(TMessage message, Action ack);
    }
}
