using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<CacheService> _logger;

        // Reusable JSON options for serialization
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public CacheService(
            IDistributedCache distributedCache, 
            IConnectionMultiplexer redisConnection, 
            ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _redisConnection = redisConnection;
            _logger = logger;
        }

        // =========================================================================
        // 1. GET METHOD
        // =========================================================================
        public async Task<T?> GetAsync<T>(string cacheKey)
        {
            try
            {
                // Step A: Ask Redis for the string stored under this key
                var cachedJsonString = await _distributedCache.GetStringAsync(cacheKey);

                // Step B: Cache Miss Check
                if (string.IsNullOrEmpty(cachedJsonString))
                {
                    return default;
                }

                _logger.LogInformation("⚡ [REDIS CACHE HIT] Key found: {Key}", cacheKey);

                // Step C: Convert the JSON text back into the C# generic object T
                return JsonSerializer.Deserialize<T>(cachedJsonString, _jsonOptions);
            }
            catch (Exception ex)
            {
                // Resilience: If Redis container drops, log error and return null so DB handles the request
                _logger.LogError(ex, "⚠️ [REDIS ERROR] Failed to fetch cache key: {Key}", cacheKey);
                return default;
            }
        }

        // =========================================================================
        // 2. SET METHOD
        // =========================================================================
        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expirationTime = null)
        {
            try
            {
                // Step A: Convert the C# object into a JSON string
                var jsonString = JsonSerializer.Serialize(value, _jsonOptions);

                // Step B: Configure the TTL countdown timer (default to 10 minutes)
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromMinutes(10)
                };

                // Step C: Save string to Redis RAM with expiration
                await _distributedCache.SetStringAsync(cacheKey, jsonString, options);

                _logger.LogInformation("💾 [REDIS CACHE SET] Saved key: {Key} for {Minutes} mins", 
                    cacheKey, (expirationTime ?? TimeSpan.FromMinutes(10)).TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ [REDIS ERROR] Failed to set cache key: {Key}", cacheKey);
            }
        }

        // =========================================================================
        // 3. REMOVE (SINGLE KEY)
        // =========================================================================
        public async Task RemoveAsync(string cacheKey)
        {
            try
            {
                await _distributedCache.RemoveAsync(cacheKey);
                _logger.LogInformation("🗑️ [REDIS CACHE DELETED] Removed key: {Key}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ [REDIS ERROR] Failed to remove cache key: {Key}", cacheKey);
            }
        }

        // =========================================================================
        // 4. REMOVE BY PREFIX (BULK PATTERN DELETION)
        // =========================================================================
        public async Task RemoveByPrefixAsync(string prefixKey)
        {
            try
            {
                // Step A: Get connection to the Redis server and database
                var endpoints = _redisConnection.GetEndPoints();
                var server = _redisConnection.GetServer(endpoints[0]);
                var db = _redisConnection.GetDatabase();

                // Step B: Search for all keys matching the prefix pattern (e.g., "TEcommerce_categories_list_*")
                var pattern = $"TEcommerce_{prefixKey}*";
                var matchingKeys = server.Keys(pattern: pattern).ToArray();

                if (matchingKeys.Length > 0)
                {
                    // Step C: Execute bulk deletion in a single atomic Redis command
                    await db.KeyDeleteAsync(matchingKeys);
                    _logger.LogInformation("🗑️ [REDIS BULK DELETED] Deleted {Count} keys matching '{Pattern}'", matchingKeys.Length, pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ [REDIS ERROR] Bulk delete failed for prefix: {Prefix}", prefixKey);
            }
        }
    }
}