using BookmarkManager.Consumers;
using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Infrastructure.Queue;
using BookmarkManager.Infrastructure.Queues;
using BookmarkManager.Services;
using BookmarkManager.Utils;
using CliFx;
using CliFx.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookmarkerManager
{
    public class Program
    {
        private static IConfigurationRoot Configuration { get; set; }

        public static async Task<int> Main()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables().Build();

            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
        }
        
        [Command("api")]
        public class FirstCommand : ICommand
        {
            public async ValueTask ExecuteAsync(IConsole console)
            {
                await CreateHostBuilder(Array.Empty<string>()).Build().RunAsync();
            }

            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureLogging(builder =>
                    {
                        builder.AddSimpleConsole(options =>
                        {
                            options.IncludeScopes = true;
                        });
                    })
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    });
        }

        [Command("bookmark-inserted-consumer")]
        public class SecondCommand : ICommand
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
                            .AddScoped<IConsumer<BookmarkInserted>, BookmarkInsertedConsumer>()
                            // infra
                            .AddHttpClient()
                            .AddHostedService<ConsumerService>()
                            .AddDbContext<BookmarkManagerContext>(options =>
                                options.UseSqlServer(Configuration.GetConnectionString("BookmarkManagerContext")))
                            .AddRabbitMQConnection(Configuration.GetSection("RabbitMQ"))
                            .AddQueue<BookmarkInserted>("bookmark.inserted");
                    });
        }
    }
}
