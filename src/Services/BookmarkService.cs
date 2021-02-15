using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookmarkManager.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly BookmarkManagerContext _context;
        private readonly IBookmarkInsertedQueue _bookmarkInsertedQueue;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(
            BookmarkManagerContext context,
            IBookmarkInsertedQueue bookmarkInsertedQueue,
            ILogger<BookmarkService> logger)
        {
            _context = context;
            _bookmarkInsertedQueue = bookmarkInsertedQueue;
            _logger = logger;
        }

        public async Task<Bookmark> AddBookmarkAsync(AddBookmarkRequest request)
        {
            var bookmark = new Bookmark(request.Url);

            await _bookmarkInsertedQueue.RunInTransaction(async () =>
            {
                _bookmarkInsertedQueue.Publish(bookmark);

                _context.Add(bookmark);
                await _context.SaveChangesAsync();
            });

            return bookmark;
        }
    }
}
