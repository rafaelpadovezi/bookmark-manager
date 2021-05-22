using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookmarkManager.Domain.Services
{
    public class WebpageService : IWebpageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WebpageService> _logger;

        public WebpageService(
            IHttpClientFactory httpClientFactory,
            ILogger<WebpageService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<(string Title, string Description, string ImageUrl)> GetPageInformationAsync(string url)
        {
            if (url == null)
                throw new ArgumentNullException(url);

            var client = _httpClientFactory.CreateClient();

            HttpResponseMessage result;
            try
            {
                result = await client.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Request to {url} got HttpRequestException", url);
                return (null, null, null);
            }

            if (result.IsSuccessStatusCode)
            {
                return ParseHtml(await result.Content.ReadAsStringAsync());
            }

            _logger.LogError("Request to {url} returned {statusCode}: {content}",
                url,
                result.StatusCode,
                await result.Content.ReadAsStringAsync());
            return (null, null, null);
        }

        public static (string Title, string Description, string ImageUrl) ParseHtml(string html)
        {
            if (html == null)
                throw new ArgumentNullException(html);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var metaTags = doc.DocumentNode.SelectNodes("/html/head/meta");

            var title =
                metaTags?.Where(x => x.Attributes["property"]?.Value == "og:title")
                    .FirstOrDefault()?.Attributes["content"]?.Value
                ??
                doc.DocumentNode.SelectNodes("/html/head/title")?
                    .FirstOrDefault()?.InnerText;

            var description = metaTags?
                .Where(x => x.Attributes["property"]?.Value == "og:description")
                .FirstOrDefault()?.Attributes["content"]?.Value;

            var imageUrl = metaTags?
                .Where(x => x.Attributes["property"]?.Value == "og:image")
                .FirstOrDefault()?.Attributes["content"]?.Value;

            return (title, description, imageUrl);
        }
    }
}
