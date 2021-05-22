// unset

using BookmarkManager.Infrastructure.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BookmarkManager.Infrastructure.Consumer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsumerService(
            this IServiceCollection services, Action<IQueueConsumer> startConsumerAction) =>
            services.AddHostedService(serviceProvider =>
                new ConsumerService(
                    serviceProvider.GetRequiredService<IQueueConsumer>(),
                    startConsumerAction));
    }
}