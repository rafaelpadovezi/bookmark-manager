using BookmarkManager.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure
{
    public interface IBookmarkInsertedQueue : IDisposable
    {
        void Publish(Bookmark bookmark);
        Task RunInTransaction(Func<Task> task);
        void Subscribe(Func<IModel, BasicDeliverEventArgs, Bookmark, Task> func);
    }
}