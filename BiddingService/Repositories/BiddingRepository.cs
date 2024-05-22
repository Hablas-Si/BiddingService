using BiddingService.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using MongoDB.Bson.IO;

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

        //In-Memory storage of ongoing auctions to avoid constant calls between services
        private static ConcurrentDictionary<Guid, Lazy<Task<LocalAuctionDetails>>> StoredAuctions = new ConcurrentDictionary<Guid, Lazy<Task<LocalAuctionDetails>>>();

        // Method to validate and update the highest bid if the new bid is higher
        public async Task<bool> SubmitBid(Bid bid)
        {
            var auctionDetails = await GetOrCheckAuctionDetails(bid.Auction); // Gets current high bid and end time


            if (bid.Amount > auctionDetails.HighestBid && DateTime.UtcNow.AddHours(2) < auctionDetails.EndTime)
            {
                // Update the highest bid
                StoredAuctions[bid.Auction] = new Lazy<Task<LocalAuctionDetails>>(() => Task.FromResult(new LocalAuctionDetails
                {
                    HighestBid = bid.Amount,
                    EndTime = auctionDetails.EndTime //If we don't re-add this, it sets itself to 01-01-0001 which breaks the validation.
                }));

                //Declaring bid status as accepted
                bid.Accepted = true;

                // Inserting bid into bid service database
                await BidCollection.InsertOneAsync(bid);

                // Preparing new high bid object
                var newBid = new HighBid { Amount = bid.Amount, userName = bid.BidOwner }; 

                // Post the new highest bid to AuctionService
                await SubmitValidatedBid(bid.Auction, newBid);
            }
            else
            {
                // Declare bid status as false - validation has failed
                bid.Accepted = false;

                // Insert into bid service database to retain bid history
                await BidCollection.InsertOneAsync(bid);
            }

            return bid.Accepted;
        }

        /*VALIDATION OF BIDS*/

        // Method to get or check the highest bid for a given auction ID
        public async Task<LocalAuctionDetails> GetOrCheckAuctionDetails(Guid auctionID)
        {
            var lazyResult = StoredAuctions.GetOrAdd(auctionID, id =>
                new Lazy<Task<LocalAuctionDetails>>(async () =>
                {
                    var auctionDetails = await GetAuctionDetailsExternal(id);
                    Console.WriteLine($"Fetched highest bid for auction {id}: {auctionDetails.HighestBid}");
                    return auctionDetails;
                }));

            var details = await lazyResult.Value;
            return details;
        }

        // Method to fetch the highest bid from an external service by retrieving the entire auction element
        private async Task<LocalAuctionDetails> GetAuctionDetailsExternal(Guid auctionID)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"http://localhost:5188/Auction/{auctionID}");
            response.EnsureSuccessStatusCode();

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