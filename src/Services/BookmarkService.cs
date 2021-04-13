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
        private readonly IQueueProducer _bookmarkInsertedQueue;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(
            BookmarkManagerContext context,
            IQueueProducer bookmarkInsertedQueue,
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

            _logger.LogInformation("Bookmark {url} added", request.Url);

            _bookmarkInsertedQueue.Publish("bookmark.inserted", new BookmarkInserted
            {
                Url = bookmark.Url,
                Id = bookmark.Id
            });

            _logger.LogInformation("Bookmark {url} sent to que", request.Url);

            return bookmark;
        }
    }
}
