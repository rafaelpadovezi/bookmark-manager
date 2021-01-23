using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly BookmarkManagerContext _context;

        public BookmarkService(BookmarkManagerContext context)
        {
            _context = context;
        }

        public async Task<Bookmark> AddBookmarkAsync(AddBookmarkRequest request)
        {
            var bookmark = new Bookmark(request.Url);

            using var transation = await _context.Database.BeginTransactionAsync();

            _context.Add(bookmark);
            await _context.SaveChangesAsync();

            // todo send message to queue

            await transation.CommitAsync();

            return bookmark;
        }
    }
}
