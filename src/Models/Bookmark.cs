using BookmarkManager.Utils;
using System;

namespace BookmarkManager.Models
{
    public class Bookmark : Entity
    {
        public Bookmark() { }

        public Bookmark(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            DisplayName = Url;
        }

        public string DisplayName { get; init; }
        public string Url { get; init; }
        public string Description { get; init; }
        public string ImageUrl { get; init; }
        public string Title { get; init; }
    }
}
