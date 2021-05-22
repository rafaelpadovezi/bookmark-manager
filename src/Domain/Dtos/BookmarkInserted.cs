using System;

namespace BookmarkManager.Domain.Dtos
{
    public class BookmarkInserted
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
    }
}
