using CachingProxy.Interfaces;
using CachingProxy.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace CachingProxy.Helpers
{
    public class RedisResponseCache : IResponseCache
    {
        public IDistributedCache _cache { get; set; }

        public RedisResponseCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task CacheResponseAsync(string key, CachedResponse response, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            await _cache.SetRecordAsync(key, response, absoluteExpireTime, unusedExpireTime);
        }

        public async Task<CachedResponse?> GetCachedResponseAsync(string key)
        {
            return await _cache.GetRecordAsync<CachedResponse>(key);
        }
    }
}
