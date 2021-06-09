using BookmarkManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarkManager.Infrastructure.DbContexts
{
    public class BookmarkManagerContext : DbContext
    {
        public BookmarkManagerContext(DbContextOptions<BookmarkManagerContext> options)
            : base(options)
        {
        }

        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    }
}
