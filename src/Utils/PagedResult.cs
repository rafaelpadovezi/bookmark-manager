using System.Collections.Generic;

namespace BookmarkManager.Utils
{
    public record PagedResult<T>(int TotalCount, IEnumerable<T> Items);
}
