using BookmarkManager.Domain.Models;
using Xunit;

namespace BookmarkManager.Tests.Unit
{
    public class BookmarkTests
    {
        [Fact(DisplayName = "Should set display name as url")]
        public void ShouldSetDisplayNameAsUrl()
        {
            var bookmark = new Bookmark("https://excalidraw.com/");

            Assert.Equal("https://excalidraw.com/", bookmark.DisplayName);
        }
    }
}