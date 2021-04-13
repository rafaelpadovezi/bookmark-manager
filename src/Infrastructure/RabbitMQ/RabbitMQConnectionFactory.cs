using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;

namespace BookmarkManager.Infrastructure
{
    public class RabbitMQConnectionFactory : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private bool _disposedValue;

        // https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan
        internal Lazy<IConnection> Connection => new(() => _factory.CreateConnection());

        public RabbitMQConnectionFactory(IOptions<RabbitMQOptions> options)
        {
            _factory = new ConnectionFactory
            {
                HostName = options.Value.HostName,
                UserName = options.Value.UserName,
                Password = options.Value.Password,
                Port = options.Value.Port,
                VirtualHost = options.Value.VHost,
                AutomaticRecoveryEnabled = options.Value.AutomaticRecoveryEnabled
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Connection.IsValueCreated)
                    {
                        Connection.Value.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
