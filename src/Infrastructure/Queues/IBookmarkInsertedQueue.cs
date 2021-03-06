using BookmarkManager.Models;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure
{
    public interface IQueue<TMessage> : IDisposable
    {
        void Publish(TMessage bookmark);
        Task RunInTransaction(Func<Task> task);
        void Subscribe(Func<Bookmark, Action, Task> func);
    }
}