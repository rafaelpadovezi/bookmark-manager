using BookmarkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarkManager.Infrastructure
{
    public class BookmarkManagerContext : DbContext
    {
        public BookmarkManagerContext(DbContextOptions<BookmarkManagerContext> options)
            : base(options)
        {
        }

        public DbSet<Bookmark> Bookmarks { get; set; }
    }
}
