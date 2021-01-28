using BookmarkManager.Services;
using System;
using System.IO;
using Xunit;

namespace BookmarkManager.Tests.Unit
{
    public class WebpageServiceTests
    {
        [Fact(DisplayName = "Should parse html with all fields")]
        public void ShouldParseHtmlWithAllFields()
        {
            // arrange
            var file = File.ReadAllText(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Unit", "data", "complete-page.html"));

            // act
            var (title, description, imageUrl) = WebpageService.ParseHtml(file);

            // assert
            Assert.Equal(".NET | Free. Cross-platform. Open Source.", title);
            Assert.Equal(".NET is a developer platform with tools and libraries for" +
                " building any type of app, including web, mobile, desktop, games, " +
                "IoT, cloud, and microservices.", description);
            Assert.Equal("https://dotnet.microsoft.com/static/images/redesign/social/square.png", imageUrl);
        }

        [Fact(DisplayName = "Should parse html with title only")]
        public void ShouldParseHtmlWithTitleOnly()
        {
            // arrange
            var file = File.ReadAllText(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Unit", "data", "page-with-title.html"));

            // act
            var (title, description, imageUrl) = WebpageService.ParseHtml(file);

            // assert
            Assert.Equal("Page with title", title);
            Assert.Null(description);
            Assert.Null(imageUrl);
        }

        [Fact(DisplayName = "Should return null to empty string")]
        public void ShouldReturnNullToEmptyString()
        {
            // act
            var (title, description, imageUrl) = WebpageService.ParseHtml(string.Empty);

            // assert
            Assert.Null(title);
            Assert.Null(description);
            Assert.Null(imageUrl);
        }

        [Fact(DisplayName = "Should throw exception if argument is null")]
        public void ShouldThrowExceptionIfArgumentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => WebpageService.ParseHtml(null));
        }
    }
}
