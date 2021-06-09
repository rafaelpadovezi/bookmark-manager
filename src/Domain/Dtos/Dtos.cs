using System;

namespace BookmarkManager.Domain.Dtos
{
    public record BookmarkInserted(Guid Id, string Url);

    public record AddBookmarkRequest(string Url);
}
