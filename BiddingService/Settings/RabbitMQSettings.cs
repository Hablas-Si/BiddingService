namespace BiddingService.Settings
{
    public class RabbitMQSettings
    {
        public string Hostname { get; set; } = "auth-test-env-rabbitmq-1";
        public string QueueName { get; set; } = "BidToAuc";
    }
}
