using BiddingService.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using StackExchange.Redis;
using BiddingService.Services;
using BiddingService.Settings;

namespace BiddingService.Repositories
{
    public class BiddingRepository : IBiddingRepository
    {
        private readonly IMongoCollection<Bid> BidCollection;

        private readonly RedisCacheService _redisCacheService;

        private readonly RabbitMQPublisher _publisher;

        public BiddingRepository(IOptions<MongoDBSettings> mongoDBSettings, RedisCacheService redisCacheService, RabbitMQPublisher publisher)
        {
            // trækker connection string og database navn og collectionname fra program.cs aka fra terminalen ved export. Dette er en constructor injection.
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionAuctionDB);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            BidCollection = database.GetCollection<Bid>(mongoDBSettings.Value.CollectionName);
            _redisCacheService = redisCacheService;
            _publisher = publisher;
        }
        //Method gets all bids posted to a specific auction. Both accepted and unaccepted ones
        public async Task<List<Bid>> GetAuctionBids(Guid auctionID)
        {
            // Define a filter to find bids with the given auctionID
            var filter = Builders<Bid>.Filter.Eq(b => b.Auction, auctionID);

            // Retrieve bids matching the filter
            var bids = await BidCollection.Find(filter).ToListAsync();

            //Return values
            return bids;
        }

        // Method to validate and update the highest bid if the new bid is higher
        public async Task<bool> SubmitBid(Bid bid)
        {
            Console.WriteLine("SUBMIT BID TRIGGERED, REPO");
            var auctionDetails = await GetOrCheckAuctionDetails(bid.Auction); // Gets current high bid and end time

            Console.WriteLine("GET DETAILS PASSED");

            //This checks if given bid is higher than the existing one, and if auction is still live
            if (bid.Amount > auctionDetails.HighestBid && DateTime.UtcNow.AddHours(2) < auctionDetails.EndTime)
            {

                Console.WriteLine("BID ACCEPTED, STATEMENT TRIGGERED");

                // Update the highest bid in Redis cache
                auctionDetails.HighestBid = bid.Amount;
                await _redisCacheService.SetAuctionDetailsAsync(bid.Auction, auctionDetails);

                // Declaring bid status as accepted
                bid.Accepted = true;

                // Inserting bid into bid service database
                await BidCollection.InsertOneAsync(bid);

                // Preparing new high bid object
                var newBid = new BidMessage { AuctionId = bid.Id, Amount = bid.Amount, Bidder = bid.BidOwner };

                // Post the new highest bid to AuctionService
                await SubmitValidatedBid(newBid);
            }
            else
            {

                // Insert into bid service database to retain bid history
                await BidCollection.InsertOneAsync(bid);
            }

            return bid.Accepted;
        }

        /*VALIDATION OF BIDS*/

        // Method to get or check the highest bid for a given auction ID
        public async Task<LocalAuctionDetails> GetOrCheckAuctionDetails(Guid auctionID)
        {
            Console.WriteLine("GET OR CHECK ENTERED");
            // Attempt to retrieve auction details from Redis cache
            var auctionDetails = await _redisCacheService.GetAuctionDetailsAsync(auctionID);

            Console.WriteLine("REDIS CHECKED");

            // If auction details are found in cache, return them
            if (auctionDetails != null)
            {
                Console.WriteLine("DETAILS IN REDIS, RETURNED");
                return auctionDetails;
            }

            Console.WriteLine("DETAILS NOT IN REDIS, FETCHING EXTERNAL");
            // Auction details not found in cache, fetch from external source
            var details = await GetAuctionDetailsExternal(auctionID);

            // Store the fetched auction details in Redis cache
            await _redisCacheService.SetAuctionDetailsAsync(auctionID, details);

            return details;
        }

        // Method to fetch the highest bid from an external service by retrieving the entire auction element
        private async Task<LocalAuctionDetails> GetAuctionDetailsExternal(Guid auctionID)
        {
            Console.WriteLine("DETAILS EXTERNAL ENTERED");
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"http://172.20.0.10:8080/api/Auction/{auctionID}");
            response.EnsureSuccessStatusCode();

            Console.WriteLine("RESPONSE FROM AUCSERVICE RECIEVED");

            var content = await response.Content.ReadAsStringAsync();

            // Ignore case sensitivity in property names as they differ slightly between services
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var auction = JsonSerializer.Deserialize<Auction>(content, options);

            // Return auction details
            return new LocalAuctionDetails
            {
                HighestBid = auction.HighBid.Amount,
                EndTime = auction.End
            };
        }

        //Method to submit a bid to auction-service, after it has been validated
        private async Task SubmitValidatedBid(BidMessage newBid)
        {

            var message = newBid;

            // Entering bid into RabbitMQ queue
            _publisher.PublishBidMessage(message);

            //This works but we should look into adding acknowledgement messaging to validate publish action has succeeded.
        }


    }
}