using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;

namespace BookmarkManager.Infrastructure.RabbitMQ
{
    public class RabbitMQConnectionFactory : IDisposable
    {
        private bool _disposedValue;

        // https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan
        internal IConnection Connection { get; }

        public RabbitMQConnectionFactory(IOptions<RabbitMQOptions> options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Value.HostName,
                UserName = options.Value.UserName,
                Password = options.Value.Password,
                Port = options.Value.Port,
                VirtualHost = options.Value.VHost,
                AutomaticRecoveryEnabled = options.Value.AutomaticRecoveryEnabled
            };
            Connection = factory.CreateConnection();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Connection.Dispose();
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
