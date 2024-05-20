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

        public int GetHighestBid(Guid auctionID)
        {
            return 200; //dummy value till auctionService is implemented
        }

        public bool ValidateBid(Bid bid)
        {
            bid.Accepted = bid.Amount > GetHighestBid(bid.Auction);
            return bid.Accepted;
        }

        //public async Task SubmitBid(Bid bid, Guid auctionID)
        //{
        //    bid.Accepted = ValidateBid(bid);

        //    if (bid.Accepted)
        //    {
        //        Console.WriteLine($"Bid submitted: {bid}");
        //        await BidCollection.InsertOneAsync(bid);
        //        var hBid = new HighBid
        //        {
        //            userName = bid.BidOwner,
        //            Amount = bid.Amount
        //        };
        //        await PostToOtherService(hBid, auctionID);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Bid rejected: {bid}");
        //        await BidCollection.InsertOneAsync(bid);
        //    }
        //}
        public async Task<List<Bid>> GetAuctionBids(Guid auctionID)
        {
            // Define a filter to find bids with the given auctionID
            var filter = Builders<Bid>.Filter.Eq(b => b.Auction, auctionID);

            // Retrieve bids matching the filter
            var bids = await BidCollection.Find(filter).ToListAsync();

            //Return values
            return bids;
        }

        //public async Task PostToOtherService(HighBid bid, Guid auctionID)
        //{
        //    try
        //    {
        //        // Serialize the bid object into JSON
        //        string jsonBid = JsonSerializer.Serialize(bid);

        //        // Create an instance of HttpClient
        //        using (var client = new HttpClient())
        //        {
        //            // Set the content type header to "application/json"
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //            // Construct the URL with auctionID using string interpolation
        //            string url = $"http://localhost:5188/Auction/{auctionID}/bid";

        //            // Make the PUT request to the endpoint of the other service
        //            var response = await client.PutAsync(url, new StringContent(jsonBid, Encoding.UTF8, "application/json"));

        //            // Check if the request was successful
        //            if (response.IsSuccessStatusCode)
        //            {
        //                // Log the successful submission
        //                Console.WriteLine($"Bid submitted to other service for auction {auctionID}: {bid}");
        //            }
        //            else
        //            {
        //                // Log if the request failed
        //                Console.WriteLine($"Failed to submit bid to other service for auction {auctionID}: {bid}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log any exceptions that occur
        //        Console.WriteLine($"An error occurred while submitting bid to other service: {ex.Message}");
        //    }
        //}


        /*VALIDATION OF BID*/

        //In-Memory storage of highest bid
        private static ConcurrentDictionary<Guid, Lazy<Task<int>>> highestBids = new ConcurrentDictionary<Guid, Lazy<Task<int>>>();

        // Method to get or initialize the highest bid for a given auction ID
        public async Task<int> GetOrInitializeHighestBidAsync(Guid auctionID)
        {
            var lazyResult = highestBids.GetOrAdd(auctionID, id =>
            new Lazy<Task<int>>(() => FetchHighestBidFromServiceAsync(id)));

            // Ensure the Lazy task is awaited properly
            var highestBid = await lazyResult.Value;
            Console.WriteLine($"Fetched highest bid for auction {auctionID}: {highestBid}");
            return highestBid;
        }

        // Method to validate and update the highest bid if the new bid is higher
        public async Task<bool> SubmitBid(Bid bid)
        {
            int currentHighestBid = await GetOrInitializeHighestBidAsync(bid.Auction);

            if (bid.Amount > currentHighestBid)
            {
                // Update the highest bid in memory (Still using lazy because otherwize the type freaks out)
                highestBids[bid.Auction] = new Lazy<Task<int>>(() => Task.FromResult(bid.Amount));

                bid.Accepted = true;
                await BidCollection.InsertOneAsync(bid);

                var newBid = new HighBid { Amount = bid.Amount, userName = bid.BidOwner };

                // Post the new highest bid to the external service (assuming this is required)
                await PutHighestBidToExternalServiceAsync(bid.Auction, newBid);
                
            }
            else
            {
                bid.Accepted = false;
                await BidCollection.InsertOneAsync(bid);
            }

            return bid.Accepted;
        }

        // Method to fetch the highest bid from an external service by retrieving the entire auction element
        private async Task<int> FetchHighestBidFromServiceAsync(Guid auctionID)
        {
            var httpClient = new HttpClient();

            // Replace the URL with the actual endpoint of the external service
            var response = await httpClient.GetAsync($"http://localhost:5188/Auction/{auctionID}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var auction = JsonSerializer.Deserialize<Auction>(content, options);

            return auction.HighBid?.Amount ?? 0;
        }

        private async Task PutHighestBidToExternalServiceAsync(Guid auctionID, HighBid newBid)
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