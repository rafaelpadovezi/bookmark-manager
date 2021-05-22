using BookmarkManager.Domain.Dtos;
using BookmarkManager.Domain.Services;
using BookmarkManager.Infrastructure.DbContexts;
using BookmarkManager.Infrastructure.RabbitMQ;
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

        public async Task UpdateBookmarkDetailsAsync(Payload payload)
        {
            var message = payload.Parse<BookmarkInserted>();
            var (title, description, imageUrl) = await _webpageService.GetPageInformationAsync(message.Url);

            var bookmark = await _context.Bookmarks.FindAsync(message.Id);
            if (bookmark is null)
            {
                _logger.LogWarning("Could not find bookmark with id {id}", message.Id);
                payload.Ack();
                return;
            }

            bookmark.Update(title, description, imageUrl);
            _context.Bookmarks.Update(bookmark);

            await _context.SaveChangesAsync();

            payload.Ack();
        }
    }
}
