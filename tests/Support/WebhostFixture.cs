﻿using BookmarkerManager;
using BookmarkManager.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BookmarkManager.Tests.Support
{
    public class WebhostFixture : IDisposable
    {
        public WebhostFixture()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .ConfigureTestServices(services =>
                {
                    var descriptor = services
                        .Single(x => x.ServiceType == typeof(DbContextOptions<BookmarkManagerContext>));
                    services.Remove(descriptor);
                    descriptor = services
                        .Single(x => x.ServiceType == typeof(BookmarkManagerContext));
                    services.Remove(descriptor);
                    services
                        .AddDbContext<BookmarkManagerContext>(
                            options =>
                                options.UseSqlServer(CreateConnectionStringWithUniqueDbName()),
                            ServiceLifetime.Singleton);
                })
                .UseStartup<Startup>();

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

        private static string CreateConnectionStringWithUniqueDbName()
        {
            return "Server=localhost;Initial Catalog=" +
                Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "") +
                ";User ID=sa;Password=Password1;";
        }

    }
}
