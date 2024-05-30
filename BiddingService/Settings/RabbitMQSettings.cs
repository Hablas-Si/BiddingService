namespace BiddingService.Settings
{
    public class RabbitMQSettings
    {
        public string Hostname { get; set; } = "backend";
        public string QueueName { get; set; } = "BidToAuc";
    }
}
