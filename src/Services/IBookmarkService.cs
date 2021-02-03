using BookmarkManager.Dtos;
using BookmarkManager.Models;
using System.Threading.Tasks;

namespace BookmarkManager.Services
{
    public interface IBookmarkService
    {
        Task<Bookmark> AddBookmarkAsync(AddBookmarkRequest request);
        Task<Bookmark> AddBookmarkWithTransactionAsync(AddBookmarkRequest request);
    }
}