using BookmarkManager.Dtos;
using BookmarkManager.Models;
using BookmarkManager.Tests.Support;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace BookmarkManager.Tests.Integration.Controllers.V1
{
    public class AddBookmarkRequestValidatorTests : WebhostFixture
    {
        [Fact(DisplayName = "Should return not found if bookmark does not exist")]
        public async Task GetBookmarkAsync_ShoudReturnNotFoundIfBookmarDoesNotExit()
        {
            // arrange
            var id = Guid.NewGuid();

            // act
            var result = await Client.GetAsync($"api/v1/bookmarks/{id}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact(DisplayName = "Should get bookmark")]
        public async Task GetBookmarkAsync_ShoudGetBookmark()
        {
            // arrange
            var id = Guid.Parse("0f899b5d-eb91-4b6a-8aa1-149fee29cc30");
            DbContext.Bookmarks.AddRange(
                new Bookmark("https://excalidraw.com/") { Id = id },
                new Bookmark("https://www.guidgenerator.com/"));
            DbContext.SaveChanges();

            // act
            var result = await Client.GetAsync($"api/v1/bookmarks/{id}");

            // assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var bookmark = await result.Content.ReadFromJsonAsync<Bookmark>();
            Assert.Equal("https://excalidraw.com/", bookmark.Url);
        }

        [Fact(DisplayName = "Should add bookmark")]
        public async Task AddBookmarkAsync_ShoudAddBookmark()
        {
            // arrange
            var request = new AddBookmarkRequest { Url = "http://www.google.com" };

            // act
            var result = await Client.PostAsJsonAsync($"api/v1/bookmarks", request);

            // assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }
    }
}
