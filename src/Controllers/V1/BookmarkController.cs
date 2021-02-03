using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Models;
using BookmarkManager.Services;
using BookmarkManager.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookmarkManager.Controllers.V1
{
    [Route("api/v1/bookmarks")]
    public class BookmarkController : ControllerBase
    {
        private readonly BookmarkManagerContext _context;
        private readonly IBookmarkService _bookmarkService;

        public BookmarkController(
            BookmarkManagerContext context,
            IBookmarkService bookmarkService)
        {
            _context = context;
            _bookmarkService = bookmarkService;
        }

        [HttpGet]
        public async Task<PagedResult<Bookmark>> Get(string content = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Bookmarks.AsQueryable();
            if (!string.IsNullOrEmpty(content))
                query = query.Where(x => x.DisplayName.ToLower().Contains(content.ToLower()));
            var count = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Bookmark>(count, items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bookmark>> GetBookmark(Guid id)
        {
            var bookmark = await _context.Bookmarks.FindAsync(id);

            if (bookmark == null)
                return NotFound();

            return bookmark;
        }

        [HttpPost]
        public async Task<ActionResult<Bookmark>> AddBookmark([FromBody] AddBookmarkRequest request)
        {
            var addedBookmark = await _bookmarkService.AddBookmarkAsync(request);

            return CreatedAtAction(nameof(GetBookmark), new { addedBookmark.Id }, addedBookmark);
        }

        [HttpPost("transaction")]
        public async Task<ActionResult<Bookmark>> AddBookmarkWithTransaction([FromBody] AddBookmarkRequest request)
        {
            var addedBookmark = await _bookmarkService.AddBookmarkWithTransactionAsync(request);

            return CreatedAtAction(nameof(GetBookmark), new { addedBookmark.Id }, addedBookmark);
        }
    }
}
