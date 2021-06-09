using BookmarkManager.Domain.Dtos;
using BookmarkManager.Domain.Models;
using BookmarkManager.Domain.Services;
using BookmarkManager.Infrastructure.DbContexts;
using BookmarkManager.Infrastructure.RabbitMQ;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
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

            bool processed = await TrackMessageAsync(message);
            if (processed)
            {
                payload.Ack();
                _logger.LogDebug($"Message {message.Id} was processed and is being discarded.");
                return;
            }

            try
            {
                await UpdateBookmarkDetailsAsync(message);
            }
            catch (DbUpdateException ex) when (IsMessageExistsError(ex))
            {
                _logger.LogDebug($"Message {message.Id} was processed and is being discarded.");
            }

            payload.Ack();
        }

        private static bool IsMessageExistsError(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                var entry = ex.Entries.FirstOrDefault(
                    x => x.Entity.GetType() == typeof(ProcessedMessage));
                if (sqlEx.Number == 2627 && entry is not null)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task UpdateBookmarkDetailsAsync(BookmarkInserted message)
        {
            var bookmark = await _context.Bookmarks.FindAsync(message.Id);
            if (bookmark is null)
            {
                _logger.LogWarning("Could not find bookmark with id {id}", message.Id);
                return;
            }

            var (title, description, imageUrl) = await
                _webpageService.GetPageInformationAsync(message.Url);
            bookmark.Update(title, description, imageUrl);
            _context.Bookmarks.Update(bookmark);

            await _context.SaveChangesAsync();
        }

        private async Task<bool> TrackMessageAsync(BookmarkInserted message)
        {
            var processedMessage = new ProcessedMessage(
                message.Id,
                typeof(BookmarkInsertedConsumer));
            var processed = await _context.ProcessedMessages.AnyAsync(processedMessage.IsEqual());
            if (!processed)
                _context.ProcessedMessages.Add(processedMessage);

            return processed;
        }
    }
}
