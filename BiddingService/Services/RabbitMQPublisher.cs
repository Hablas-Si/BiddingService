using RabbitMQ.Client;
using System.Text.Json;
using BiddingService.Models;

namespace BiddingService.Services
{
    // Service class for publishing messages to RabbitMQ
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection; // RabbitMQ connection
        private readonly IModel _channel; 
        private readonly string _queueName; // Name of the queue to publish messages to

        // Constructor to initialize the RabbitMQPublisher with hostname and queue name
        public RabbitMQPublisher(string hostname, string queueName)
        {
            _queueName = queueName;

            // Create a connection factory with the specified hostname
            var factory = new ConnectionFactory() { HostName = hostname };

            // Create a connection to the RabbitMQ server
            _connection = factory.CreateConnection();

            // Create a channel, which is the communication line with RabbitMQ
            _channel = _connection.CreateModel();

            // Declare a queue with the specified name and properties
            _channel.QueueDeclare(
                queue: _queueName,
                durable: false, // The queue will not survive a broker restart
                exclusive: false, // The queue can be accessed by other connections
                autoDelete: false, // The queue will not be deleted automatically when not in use
                arguments: null); // no additional arguments
        }

        // Method to publish a bid message to the RabbitMQ queue
        public void PublishBidMessage(BidMessage message)
        {
            // Serialize the message to a UTF-8 byte array
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            // Publish the message to the queue with the specified routing key
            _channel.BasicPublish(
                exchange: "", // Default exchange
                routingKey: _queueName, // Queue name as routing key
                basicProperties: null, // No additional properties
                body: body); // The serialized message body
        }

        // Dispose method to close the RabbitMQ connection and channel
        public void Dispose()
        {
            // Close the channel if it is not null
            _channel?.Close();

            // Close the connection if it is not null
            _connection?.Close();
        }
    }
}
