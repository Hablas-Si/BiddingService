namespace BiddingService.Settings
{
    public class MongoDBSettings
    {
        public string ConnectionAuctionDB { get; set; } = null!;
        public string DatabaseName { get; set; } = "BiddingDB";
        public string CollectionName { get; set; } = "BidCollection";
    }
}