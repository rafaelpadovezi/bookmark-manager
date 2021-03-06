using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using BookmarkManager.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Consumers
{
    public interface IConsumer<TMessage>
    {
        Task ExecuteAsync(TMessage message, Action ack);
    }

    public class BookmarkInsertedConsumer : IConsumer<Bookmark>
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

        public async Task ExecuteAsync(Bookmark message, Action ack)
        {
            var (title, description, imageUrl) = await _webpageService.GetPageInformation(bookmark.Url);

            bookmark.Update(title, description, imageUrl);

            _context.Update(bookmark);

            using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();

            ack();
            await transaction.CommitAsync();
        }
    }
}
