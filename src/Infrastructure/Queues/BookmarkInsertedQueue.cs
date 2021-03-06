using BookmarkManager.Dtos;
using BookmarkManager.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookmarkManager.Infrastructure
{

    public sealed class BookmarkInsertedQueue : IQueue<BookmarkInserted>
    {
        private const string _queueName = "bookmark.inserted";
        private readonly RabbitMQConnectionFactory _rabbitMQConnectionFactory;
        private readonly ILogger<BookmarkInsertedQueue> _logger;
        private readonly IModel _channel;

        public BookmarkInsertedQueue(
            RabbitMQConnectionFactory rabbitMQConnectionFactory,
            ILogger<BookmarkInsertedQueue> logger)
        {
            _rabbitMQConnectionFactory = rabbitMQConnectionFactory;
            _logger = logger;

            var connection = _rabbitMQConnectionFactory.Connection.Value;
            _channel = connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public async Task RunInTransaction(Func<Task> task)
        {
            _channel.TxSelect();
            try
            {
                await task();
                _channel.TxCommit();
                _logger.LogDebug("Broker tx commited");
            }
            catch
            {
                _logger.LogInformation("Broker tx is being rollbacked after exception");
                _channel.TxRollback();
                throw;
            }
        }

        public void Publish(BookmarkInserted bookmark)
        {
            var message = new { bookmark.Id, bookmark.Url };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation("Sent {message}", message);
        }

        public void Subscribe(Func<BookmarkInserted, Action, Task> func)
        {
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var bookmark = JsonSerializer.Deserialize<BookmarkInserted>(
                    Encoding.UTF8.GetString(body));

                _logger.LogInformation("Receveid {@message}", bookmark);

                Action ackAction = () => _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                try
                {
                    await func(bookmark, ackAction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    throw;
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
