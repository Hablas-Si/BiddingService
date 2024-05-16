using System.Threading.Tasks;
using BiddingService.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using BiddingService.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

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

        public async Task SubmitBid(Bid bid, Guid auctionID)
        {
            bid.Accepted = ValidateBid(bid);

            if (bid.Accepted)
            {
                Console.WriteLine($"Bid submitted: {bid}");
                await BidCollection.InsertOneAsync(bid);
                var hBid = new HighBid
                {
                    userName = bid.BidOwner,
                    Amount = bid.Amount
                };
                await PostToOtherService(hBid, auctionID);
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

        public async Task PostToOtherService(HighBid bid, Guid auctionID)
        {
            try
            {
                // Serialize the bid object into JSON
                string jsonBid = JsonSerializer.Serialize(bid);

                // Create an instance of HttpClient
                using (var client = new HttpClient())
                {
                    // Set the content type header to "application/json"
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Construct the URL with auctionID using string interpolation
                    string url = $"http://localhost:5188/Auction/{auctionID}/bid";

                    // Make the PUT request to the endpoint of the other service
                    var response = await client.PutAsync(url, new StringContent(jsonBid, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Log the successful submission
                        Console.WriteLine($"Bid submitted to other service for auction {auctionID}: {bid}");
                    }
                    else
                    {
                        // Log if the request failed
                        Console.WriteLine($"Failed to submit bid to other service for auction {auctionID}: {bid}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                Console.WriteLine($"An error occurred while submitting bid to other service: {ex.Message}");
            }
        }
    }
}