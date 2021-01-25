using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookmarkManager.Services
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

            using var transation = await _context.Database.BeginTransactionAsync();

            _context.Add(bookmark);
            await _context.SaveChangesAsync();

            SendMessage(bookmark);

            await transation.CommitAsync();

            return bookmark;
        }

        private void SendMessage(Bookmark bookmark)
        {
            var factory = new ConnectionFactory() { HostName = "localhost",
            UserName="rabbitmq",
            Password = "rabbitmq"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "bookmark_inserted",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

            var message = new { bookmark.Id, bookmark.Url };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(exchange: "",
                                 routingKey: "bookmark_inserted",
                                 basicProperties: null,
                                 body: body);
            _logger.LogInformation("Sent {message}", message);
        }
    }
}
