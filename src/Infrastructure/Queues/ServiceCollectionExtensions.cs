using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookmarkManager.Infrastructure.Queues
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueue<TMessage>(
            this IServiceCollection services, string queueName)
        {
            services.AddScoped<IQueue<TMessage>, Queue<TMessage>>(
                x => new Queue<TMessage>(
                    x.GetRequiredService<RabbitMQConnectionFactory>(),
                    x.GetRequiredService<ILogger<Queue<TMessage>>>(),
                    queueName));

            return services;
        }
    }
}
