using BookmarkManager.Domain.Dtos;
using BookmarkManager.Domain.Dtos.Validations;
using Xunit;

namespace BookmarkManager.Tests.Unit
{
    public class AddBookmarkRequestValidatorTests
    {
        AddBookmarkRequestValidator _validator = new AddBookmarkRequestValidator();

        [Theory(DisplayName = "Should be valid")]
        [InlineData("https://excalidraw.com/")]
        [InlineData("https://www.google.com/search?sxsrf=ALeKk03knZbRSdp2-vONKM_AWSRCTeHrJw%3A1611420370470&ei=0lIMYIuXHPSz5OUPmqCROA&q=test&oq=test&gs_lcp=CgZwc3ktYWIQAzIECAAQQzICCAAyAggAMgIIADICCAAyAggAMgIIADICCAAyAggAMgIIADoFCAAQkQI6AgguOgoILhDHARCvARBDOggILhDHARCvAToHCC4QQxCTAjoECC4QQzoKCC4QxwEQowIQQzoNCC4QxwEQowIQQxCTAlDnklxY3Z5cYIugXGgCcAB4AIABkwGIAaQGkgEDMC42mAEAoAEBqgEHZ3dzLXdpesABAQ&sclient=psy-ab&ved=0ahUKEwiLyI_WwLLuAhX0GbkGHRpQBAcQ4dUDCA0&uact=5")]
        public void ShouldBeValid(string url)
        {
            var request = new AddBookmarkRequest(url);
            Assert.True(_validator.Validate(request).IsValid);
        }

        [Theory(DisplayName = "Should be invalid")]
        [InlineData("teste")]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldBeInvalid(string url)
        {
            var request = new AddBookmarkRequest(url);
            Assert.False(_validator.Validate(request).IsValid);
        }
    }
}
