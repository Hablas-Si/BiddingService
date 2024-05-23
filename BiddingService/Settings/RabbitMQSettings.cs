namespace BiddingService.Settings
{
    public class RabbitMQSettings
    {
        public string Hostname { get; set; } = "localhost";
        public string QueueName { get; set; } = "BidToAuc";
    }
}
