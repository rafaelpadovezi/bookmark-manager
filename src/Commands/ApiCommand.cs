using BookmarkManager.Domain.Services;
using CliFx;
using CliFx.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookmarkManager.Commands
{
    [Command("api")]
    public class ApiCommand : ICommand
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
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<OutboxSendingService>();
                });
    }
}
