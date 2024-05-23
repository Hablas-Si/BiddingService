using RabbitMQ.Client;
using System.Text.Json;
using BiddingService.Models;

namespace BiddingService.Services
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMQPublisher(string hostname, string queueName)
        {
            _queueName = queueName;

            var factory = new ConnectionFactory() { HostName = hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void PublishBidMessage(BidMessage message)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
