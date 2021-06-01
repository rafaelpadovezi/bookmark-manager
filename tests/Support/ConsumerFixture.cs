using BookmarkManager.Commands;
using BookmarkManager.Infrastructure.DbContexts;
using BookmarkManager.Infrastructure.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Contrib.HttpClient;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace BookmarkManager.Tests.Support
{
    public class ConsumerFixture<T> : IDisposable where T : IHostedService
    {
        public IServiceProvider ServiceProvider { get; }
        public BookmarkManagerContext DbContext { get; }
        public Mock<HttpMessageHandler> MockHttpMessageHandler { get; } = new();

        public ConsumerFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            ;
            var appHost = BookmarkInsertedConsumerCommand
                .CreateConsumer(Array.Empty<string>(), configuration)
                .ConfigureServices(services =>
                {
                    services.AddScoped<IQueueProducer, RabbitMqClient>();
                    services.AddTransient(_ =>
                    {
                        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
                        mockHttpClientFactory
                            .Setup(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(MockHttpMessageHandler.CreateClient());
                        return mockHttpClientFactory.Object;
                    });
                })
                .UseEnvironment("Testing")
                .Build();

            ServiceProvider = appHost.Services.CreateScope().ServiceProvider;
            var hostedService = ServiceProvider.GetRequiredService<IHostedService>();
            hostedService.StartAsync(CancellationToken.None).Wait();

            DbContext = ServiceProvider.GetRequiredService<BookmarkManagerContext>();

            DbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
        }
    }
}