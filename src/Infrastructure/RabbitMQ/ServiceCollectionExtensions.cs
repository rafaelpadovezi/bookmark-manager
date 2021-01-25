using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookmarkManager.Infrastructure.Queue
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQConnection(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddSingleton<RabbitMQConnectionFactory>()
                .Configure<RabbitMQOptions>(config);

            return services;
        }
    }
}
