using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookmarkManager.Infrastructure.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQConnection(
            this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddSingleton<RabbitMQConnectionFactory>()
                .Configure<RabbitMQOptions>(config);
        }
    }
}
