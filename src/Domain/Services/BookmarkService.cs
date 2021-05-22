using BookmarkManager.Domain.Dtos;
using BookmarkManager.Domain.Models;
using BookmarkManager.Infrastructure.DbContexts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookmarkManager.Domain.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly BookmarkManagerContext _context;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(
            BookmarkManagerContext context,
            ILogger<BookmarkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Bookmark> AddBookmarkAsync(AddBookmarkRequest request)
        {
            var bookmark = new Bookmark(request.Url);

            _context.Bookmarks.Add(bookmark);

            var outboxMessage = new OutboxMessage(
                "bookmark.inserted",
                new BookmarkInserted
                {
                    Url = bookmark.Url,
                    Id = bookmark.Id
                });

            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bookmark {url} added", request.Url);

            return bookmark;
        }
    }
}
