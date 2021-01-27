using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookmarkManager.Consumers;
using BookmarkManager.Infrastructure.Queue;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                        .AddRabbitMQConnection(Configuration.GetSection("RabbitMQ"))
                        .AddHostedService<BookmarkInsertedConsumer>();
                });
    }
}
