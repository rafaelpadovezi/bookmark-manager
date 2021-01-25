using BookmarkManager.Models;

namespace BookmarkManager.Infrastructure
{
    public interface IBookmarkInsertedQueue
    {
        void Publish(Bookmark bookmark);
    }
}