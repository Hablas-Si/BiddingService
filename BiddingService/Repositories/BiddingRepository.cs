using BiddingService.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using MongoDB.Bson.IO;
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

        public async Task<List<Bid>> GetAuctionBids(Guid auctionID)
        {
            // Define a filter to find bids with the given auctionID
            var filter = Builders<Bid>.Filter.Eq(b => b.Auction, auctionID);

            // Retrieve bids matching the filter
            var bids = await BidCollection.Find(filter).ToListAsync();

            //Return values
            return bids;
        }

        //In-Memory storage of highest bid
        private static ConcurrentDictionary<Guid, Lazy<Task<int>>> highestBids = new ConcurrentDictionary<Guid, Lazy<Task<int>>>();

        // Method to validate and update the highest bid if the new bid is higher
        public async Task<bool> SubmitBid(Bid bid)
        {
            int currentHighestBid = await GetOrCheckHighBid(bid.Auction); //Gets current high bid

            if (bid.Amount > currentHighestBid)
            {
                // Update the highest bid in memory (Still using lazy because otherwize the type freaks out)
                highestBids[bid.Auction] = new Lazy<Task<int>>(() => Task.FromResult(bid.Amount));

                bid.Accepted = true;
                await BidCollection.InsertOneAsync(bid); //Inserting bid into bidserviec database

                var newBid = new HighBid { Amount = bid.Amount, userName = bid.BidOwner }; //Preparing new highbid object

                // Post the new highest bid to the external service
                await SubmitValidatedBid(bid.Auction, newBid);

            }
            else
            {
                bid.Accepted = false;
                await BidCollection.InsertOneAsync(bid); //Inserts into bidservice database to retain bid-history
            }

            return bid.Accepted;
        }

        /*VALIDATION OF BIDS*/

        // Method to get or check the highest bid for a given auction ID
        public async Task<int> GetOrCheckHighBid(Guid auctionID)
        {
            var lazyResult = highestBids.GetOrAdd(auctionID, id =>
            new Lazy<Task<int>>(() => GetHighestBidExternal(id)));

            // Ensure the Lazy task is awaited properly
            var highestBid = await lazyResult.Value;
            Console.WriteLine($"Fetched highest bid for auction {auctionID}: {highestBid}");
            return highestBid;
        }

        // Method to fetch the highest bid from an external service by retrieving the entire auction element
        private async Task<int> GetHighestBidExternal(Guid auctionID)
        {
            var httpClient = new HttpClient();

            
            var response = await httpClient.GetAsync($"http://localhost:5188/Auction/{auctionID}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //Ignore case sensitivity in property names as they differ slightly between services :/
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var auction = JsonSerializer.Deserialize<Auction>(content, options);

            //Return the amount of the highest bid if it exists; otherwise, return 0 [Implemented to avoid weird null-errors]
            return auction.HighBid?.Amount ?? 0;
        }

        //MEthod to submit a bid to auction-service, after it has been validated
        private async Task SubmitValidatedBid(Guid auctionID, HighBid newBid)
        {
            var httpClient = new HttpClient();
            var requestContent = new StringContent(JsonSerializer.Serialize(newBid), Encoding.UTF8, "application/json");

            // Replace the URL with the actual endpoint of the external service
            var response = await httpClient.PutAsync($"http://localhost:5188/Auction/{auctionID}/bid", requestContent);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();
        }

        
    }
}