namespace BiddingService.Settings
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = "mongodb+srv://admin:admin@auctionhouse.dfo2bcd.mongodb.net/";
        public string DatabaseName { get; set; } = "BiddingDB";
        public string CollectionName { get; set; } = "BidCollection";
    }
}