using System;

namespace BookmarkManager.Dtos
{
    public class BookmarkInserted
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
    }
}
