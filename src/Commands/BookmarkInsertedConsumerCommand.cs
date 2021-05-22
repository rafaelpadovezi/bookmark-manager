using BookmarkManager.Consumers;
using BookmarkManager.Domain.Services;
using BookmarkManager.Infrastructure.Consumer;
using BookmarkManager.Infrastructure.DbContexts;
using BookmarkManager.Infrastructure.RabbitMQ;
using CliFx;
using CliFx.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Commands
{
    [Command("bookmark-inserted-consumer")]
    public class BookmarkInsertedConsumerCommand : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await CreateConsumer(Array.Empty<string>()).Build().RunAsync();
        }

        public static IHostBuilder CreateConsumer(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
                {
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                    });
                })
                .ConfigureServices(services =>
                {
                    services
                        // application core
                        .AddScoped<IWebpageService, WebpageService>()
                        .AddScoped<BookmarkInsertedConsumer>()
                        // infra
                        .AddHttpClient()
                        .AddConsumerService(queueConsumer =>
                            queueConsumer.Subscribe<BookmarkInsertedConsumer>(
                                "bookmark.inserted",
                                consumer => consumer.UpdateBookmarkDetailsAsync)
                        )
                        .AddDbContext<BookmarkManagerContext>(options =>
                            options.UseSqlServer(Program.Configuration.GetConnectionString("BookmarkManagerContext")))
                        .AddRabbitMQConnection(Program.Configuration.GetSection("RabbitMQ"))
                        .AddScoped<IQueueConsumer, RabbitMQClient>();
                });
    }
}
