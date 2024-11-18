using BookmarkManager.Commands;
using BookmarkManager.Infrastructure.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Xunit;

namespace BookmarkManager.Tests.Support
{
    [Collection("Database collection")]
    public class WebHostFixture : IDisposable
    {
        public WebHostFixture()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseEnvironment("Testing")
                .UseStartup<ApiCommand.Startup>();

            var server = new TestServer(builder);

            ServiceProvider = server.Host.Services;
            Client = server.CreateClient();
            DbContext = ServiceProvider.GetRequiredService<BookmarkManagerContext>();

            DbContext.Database.EnsureCreated();
        }

        public HttpClient Client { get; }
        public IServiceProvider ServiceProvider { get; }
        public BookmarkManagerContext DbContext { get; }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
        }
    }
}