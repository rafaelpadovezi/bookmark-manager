using BookmarkManager.Domain.Dtos;
using BookmarkManager.Domain.Models;
using System.Threading.Tasks;

namespace BookmarkManager.Domain.Services
{
    public interface IBookmarkService
    {
        Task<Bookmark> AddBookmarkAsync(AddBookmarkRequest request);
    }
}