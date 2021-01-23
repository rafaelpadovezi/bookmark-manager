using BookmarkManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
