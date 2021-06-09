using BookmarkManager.Consumers;
using BookmarkManager.Domain.Dtos;
using BookmarkManager.Domain.Models;
using BookmarkManager.Infrastructure.Consumer;
using BookmarkManager.Infrastructure.RabbitMQ;
using BookmarkManager.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq.Contrib.HttpClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BookmarkManager.Tests.Integration.Consumers
{
    public class BookmarkInsertedConsumerTests : ConsumerFixture<ConsumerService>
    {
        [Fact(DisplayName = "Should update bookmark details")]
        public async Task ShouldUpdateBookmarkDetails()
        {
            // Arrange
            MockHttpMessageHandler
                .SetupRequest("https://localhost/54321")
                .ReturnsResponse(
                    "<html>" +
                    "<head>" +
                    "<meta property=\"og:description\" content=\"A very funny website\">" +
                    "</head>" +
                    "</html>"
                    );
            var bookmark = new Bookmark { Url = "https://localhost/54321" };
            DbContext.Add(bookmark);
            await DbContext.SaveChangesAsync();
            var queueProducer = ServiceProvider.GetRequiredService<IQueueProducer>();
            queueProducer.Publish("test-queue", new BookmarkInserted(bookmark.Id, bookmark.Url));
            var queueConsumer = ServiceProvider.GetRequiredService<IQueueConsumer>();
            var semaphore = new SemaphoreSlim(0, 1);
            // Act
            queueConsumer.Subscribe<BookmarkInsertedConsumer>("test-queue", consumer =>
            {
                return async payload =>
                {
                    await consumer.UpdateBookmarkDetailsAsync(payload);
                    semaphore.Release();
                };
            });
            // Assert
            Assert.True(await semaphore.WaitAsync(50000), "Didn't received message.");
            var updatedBookmark = DbContext.Bookmarks.AsNoTracking().Single(x => x.Id == bookmark.Id);
            Assert.Equal("A very funny website", updatedBookmark.Description);
        }
    }
}
