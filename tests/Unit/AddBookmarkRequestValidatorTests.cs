using BookmarkManager.Dtos;
using BookmarkManager.Dtos.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BookmarkManager.Tests.Unit
{
    public class AddBookmarkRequestValidatorTests
    {
        AddBookmarkRequestValidator validator = new AddBookmarkRequestValidator();

        [Theory(DisplayName = "Should be valid")]
        [InlineData("https://excalidraw.com/")]
        [InlineData("https://www.google.com/search?sxsrf=ALeKk03knZbRSdp2-vONKM_AWSRCTeHrJw%3A1611420370470&ei=0lIMYIuXHPSz5OUPmqCROA&q=test&oq=test&gs_lcp=CgZwc3ktYWIQAzIECAAQQzICCAAyAggAMgIIADICCAAyAggAMgIIADICCAAyAggAMgIIADoFCAAQkQI6AgguOgoILhDHARCvARBDOggILhDHARCvAToHCC4QQxCTAjoECC4QQzoKCC4QxwEQowIQQzoNCC4QxwEQowIQQxCTAlDnklxY3Z5cYIugXGgCcAB4AIABkwGIAaQGkgEDMC42mAEAoAEBqgEHZ3dzLXdpesABAQ&sclient=psy-ab&ved=0ahUKEwiLyI_WwLLuAhX0GbkGHRpQBAcQ4dUDCA0&uact=5")]
        public void ShouldBeValid(string url)
        {
            var request = new AddBookmarkRequest { Url = url };
            Assert.True(validator.Validate(request).IsValid);
        }

        [Theory(DisplayName = "Should be invalid")]
        [InlineData("teste")]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldBeInvalid(string url)
        {
            var request = new AddBookmarkRequest { Url = url };
            Assert.False(validator.Validate(request).IsValid);
        }
    }
}
