using BookmarkManager.Consumers;
using BookmarkManager.Dtos;
using BookmarkManager.Infrastructure;
using BookmarkManager.Infrastructure.Queue;
using BookmarkManager.Models;
using BookmarkManager.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace BookmarkerManager
{
    public class Program
    {
        private static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables().Build();

            switch (args[0])
            {
                case "api":
                    CreateHostBuilder(args).Build().Run();
                    break;
                case "bookmark-inserted-consumer":
                    CreateConsumer(args).Build().Run();
                    break;
                default:
                    throw new ArgumentException("Argument must be valid");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static IHostBuilder CreateConsumer(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
                        .AddScoped<IQueue<BookmarkInserted>, BookmarkInsertedQueue>();
                });
    }
}
