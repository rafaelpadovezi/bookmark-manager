using System.Threading.Tasks;

namespace BookmarkManager.Domain.Services
{
    public interface IWebpageService
    {
        Task<(string Title, string Description, string ImageUrl)> GetPageInformationAsync(string url);
    }
}