using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Threading;

namespace BookmarkManager.Infrastructure.RabbitMQ
{
    public class RabbitMqConnection : IDisposable
    {
        private bool _disposedValue;

        // https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan
        private IConnection Connection { get; }
        // Since channels are not thread safe but should be reused, the `ThreadLocal`
        // guarantees the creation of one channel per thread. 
        // From rabbitmq client docs:
        //   For applications that use multiple threads/processes for processing,
        //   it is very common to open a new channel per thread/process and not share
        //   channels between them.
        // https://www.rabbitmq.com/channels.html#basics
        private ThreadLocal<IModel> ThreadSafeChannel { get; }
        internal IModel Channel => ThreadSafeChannel.Value;

        public RabbitMqConnection(IOptions<RabbitMqOptions> options)
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
            ThreadSafeChannel = new ThreadLocal<IModel>(
                () => Connection.CreateModel(),
                trackAllValues: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var channel in ThreadSafeChannel.Values)
                    {
                        channel.Close();
                    }
                    Connection.Close();
                    // Should these objects be disposed?
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
