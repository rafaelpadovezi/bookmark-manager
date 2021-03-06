using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public interface IConsumer<TMessage>
    {
        Task ExecuteAsync(TMessage message, Action ack);
    }

    public class BookmarkInsertedConsumer : IConsumer<BookmarkInserted>
    {
        private readonly BookmarkManagerContext _context;
        private readonly IWebpageService _webpageService;

        public BookmarkInsertedConsumer(
            BookmarkManagerContext context,
            IWebpageService webpageService,
            ILogger<BookmarkInsertedConsumer> logger)
        {
            _context = context;
            _webpageService = webpageService;
        }

        public async Task ExecuteAsync(BookmarkInserted message, Action ack)
        {
            var (title, description, imageUrl) = await _webpageService.GetPageInformation(message.Url);

            var bookmark = await _context.Bookmarks.FindAsync(message.Id);
            bookmark.Update(title, description, imageUrl);

            _context.Update(bookmark);

            using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();

            ack();
            await transaction.CommitAsync();
        }
    }
}
