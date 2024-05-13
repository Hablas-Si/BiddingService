using System.Threading.Tasks;
using BiddingService.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using BiddingService.Models;

namespace BiddingService.Repositories
{
    public class BiddingRepository : IBiddingRepository
    {
        private readonly IMongoCollection<Bid> BidCollection;

        public BiddingRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            // trækker connection string og database navn og collectionname fra program.cs aka fra terminalen ved export. Dette er en constructor injection.
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            BidCollection = database.GetCollection<Bid>(mongoDBSettings.Value.CollectionName);
        }

        public int GetHighestBid(Guid auctionID)
        {
            return 200; //dummy value till auctionService is implemented
        }

        public bool ValidateBid(Bid bid)
        {
            bid.Accepted = bid.Amount > GetHighestBid(bid.Auction);
            return bid.Accepted;
        }

        public async Task SubmitBid(Bid bid)
        {
            bid.Accepted = ValidateBid(bid);

            if (bid.Accepted)
            {
                Console.WriteLine($"Bid submitted: {bid}");
                await BidCollection.InsertOneAsync(bid);
            }
            else
            {
                Console.WriteLine($"Bid rejected: {bid}");
                await BidCollection.InsertOneAsync(bid);
            }
        }
        public async Task<List<Bid>> GetAuctionBids(Guid auctionID)
        {
            // Define a filter to find bids with the given auctionID
            var filter = Builders<Bid>.Filter.Eq(b => b.Auction, auctionID);

            // Retrieve bids matching the filter
            var bids = await BidCollection.Find(filter).ToListAsync();

            //Return values
            return bids;
        }
    }
}