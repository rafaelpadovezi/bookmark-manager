using System.Threading.Tasks;

namespace BookmarkManager.Services
{
    public interface IWebpageService
    {
        Task<(string Title, string Description, string ImageUrl)> GetPageInformation(string url);
    }
}