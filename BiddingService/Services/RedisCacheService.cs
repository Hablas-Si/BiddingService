using StackExchange.Redis;
using System.Text.Json;
using BiddingService.Models;
using Microsoft.Extensions.Logging;

namespace BiddingService.Services
{
    public class RedisCacheService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(string connectionString, string redisPassword, ILogger<RedisCacheService> logger)
        {
            _logger = logger;
            try
            {
                _logger.LogInformation("Initializing RedisCacheService with connection string: {ConnectionString}", connectionString);
                ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
                options.Password = redisPassword;

                _redis = ConnectionMultiplexer.Connect(options);
                _database = _redis.GetDatabase();
                _logger.LogInformation("RedisCacheService initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RedisCacheService.");
                throw;
            }
        }

        public async Task SetAuctionDetailsAsync(Guid auctionId, LocalAuctionDetails auctionDetails)
        {
            try
            {
                var json = JsonSerializer.Serialize(auctionDetails);
                await _database.StringSetAsync(auctionId.ToString(), json, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Set auction details for Auction ID: {AuctionId}", auctionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting auction details for Auction ID: {AuctionId}", auctionId);
                throw;
            }
        }

        public async Task<LocalAuctionDetails> GetAuctionDetailsAsync(Guid auctionId)
        {
            try
            {
                var json = await _database.StringGetAsync(auctionId.ToString());
                _logger.LogInformation("Get auction details for Auction ID: {AuctionId}", auctionId);
                return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<LocalAuctionDetails>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auction details for Auction ID: {AuctionId}", auctionId);
                throw;
            }
        }
    }
}
