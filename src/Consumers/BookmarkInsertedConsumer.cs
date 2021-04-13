using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public class BookmarkInsertedConsumer
    {
        private readonly BookmarkManagerContext _context;
        private readonly IWebpageService _webpageService;
        private readonly ILogger<BookmarkInsertedConsumer> _logger;

        public BookmarkInsertedConsumer(
            BookmarkManagerContext context,
            IWebpageService webpageService,
            ILogger<BookmarkInsertedConsumer> logger)
        {
            _context = context;
            _webpageService = webpageService;
            _logger = logger;
        }

        public async Task SaveBookmarDetailsAsync(Payload payload)
        {
            var message = payload.Parse<BookmarkInserted>();
            var (title, description, imageUrl) = await _webpageService.GetPageInformation(message.Url);

            var bookmark = await _context.Bookmarks.FindAsync(message.Id);
            if (bookmark is null)
            {
                _logger.LogWarning("Could not find bookmark with id {id}", message.Id);
                payload.Ack();
                return;
            }

            bookmark.Update(title, description, imageUrl);
            _context.Update(bookmark);

            using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();

            payload.Ack();
            await transaction.CommitAsync();
        }
    }
}
