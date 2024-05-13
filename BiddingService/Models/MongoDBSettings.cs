namespace Models
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = "BiddingDB";
        public string CollectionName { get; set; } = "BidCollection";
    }
}