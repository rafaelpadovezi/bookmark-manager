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

            _context.Add(bookmark);
            await _context.SaveChangesAsync();

            _bookmarkInsertedQueue.Publish(bookmark);

            return bookmark;
        }

        public async Task<Bookmark> AddBookmarkWithTransactionAsync(AddBookmarkRequest request)
        {
            var bookmark = new Bookmark(request.Url);

            using var transation = await _context.Database.BeginTransactionAsync();

            _context.Add(bookmark);
            await _context.SaveChangesAsync();

            _bookmarkInsertedQueue.Publish(bookmark);

            await transation.CommitAsync();

            return bookmark;
        }
    }
}
