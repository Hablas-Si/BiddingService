namespace BiddingService.Settings
{
    public class RabbitMQSettings
    {
        public string Hostname { get; set; } = "rabbitmq";
        public string QueueName { get; set; } = "BidToAuc";
    }
}
