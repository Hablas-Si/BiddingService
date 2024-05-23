using StackExchange.Redis;
using System.Text.Json;
using BiddingService.Models;

namespace BiddingService.Services
{
    public class RedisCacheService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(string connectionString)
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            options.Password = "0rIwX58ixdvj6btmfJrxvsxaMn3s4uta";
            _redis = ConnectionMultiplexer.Connect(options);
            _database = _redis.GetDatabase();
        }

        public async Task SetAuctionDetailsAsync(Guid auctionId, LocalAuctionDetails auctionDetails)
        {
            var json = JsonSerializer.Serialize(auctionDetails);
            await _database.StringSetAsync(auctionId.ToString(), json, TimeSpan.FromMinutes(10)); // Optional expiry time
        }

        public async Task<LocalAuctionDetails> GetAuctionDetailsAsync(Guid auctionId)
        {
            var json = await _database.StringGetAsync(auctionId.ToString());
            return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<LocalAuctionDetails>(json);
        }
    }
}
