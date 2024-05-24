using StackExchange.Redis; 
using System.Text.Json; 
using BiddingService.Models; 

namespace BiddingService.Services
{
    // Service class for interacting with Redis cache
    public class RedisCacheService
    {
        private readonly ConnectionMultiplexer _redis; 
        private readonly IDatabase _database; 

        // Constructor to initialize the RedisCacheService with a connection string
        public RedisCacheService(string connectionString, string RedisPW)
        {
            // Parse the connection string and set the password
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            options.Password = RedisPW; //this should add in the PW as it is no longer part of the connection string itself

            // Connect to Redis using the configured options
            _redis = ConnectionMultiplexer.Connect(options);

            // Get a database instance from the connection multiplexer
            _database = _redis.GetDatabase();
        }

        // Method to set auction details in Redis cache asynchronously
        public async Task SetAuctionDetailsAsync(Guid auctionId, LocalAuctionDetails auctionDetails)
        {
            // Serialize the auction details object to JSON
            var json = JsonSerializer.Serialize(auctionDetails);

            // Set the JSON string in Redis cache with an optional expiry time of 10 minutes
            await _database.StringSetAsync(auctionId.ToString(), json, TimeSpan.FromMinutes(10));
        }

        // Method to get auction details from Redis cache asynchronously
        public async Task<LocalAuctionDetails> GetAuctionDetailsAsync(Guid auctionId)
        {
            // Get the JSON string from Redis cache for the specified auction ID
            var json = await _database.StringGetAsync(auctionId.ToString());

            // Deserialize the JSON string to a LocalAuctionDetails object
            return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<LocalAuctionDetails>(json);
        }
    }
}
