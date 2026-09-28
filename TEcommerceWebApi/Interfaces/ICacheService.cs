using System;
using System.Threading.Tasks;

namespace TEcommerceWebApi.Interfaces
{
    public interface ICacheService
    {
        // 1. Read any C# object (DTO, List, PaginatedResult) from Redis
        Task<T?> GetAsync<T>(string cacheKey);

        // 2. Save any C# object into Redis with a Time-To-Live (TTL) expiration
        Task SetAsync<T>(string cacheKey, T value, TimeSpan? expirationTime = null);

        // 3. Delete one specific key from Redis (Single-item invalidation)
        Task RemoveAsync(string cacheKey);

        // 4. Delete all keys matching a wildcard pattern (e.g. "categories_list_*")
        Task RemoveByPrefixAsync(string prefixKey);
    }
}