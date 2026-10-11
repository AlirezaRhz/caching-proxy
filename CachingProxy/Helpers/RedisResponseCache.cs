using CachingProxy.Interfaces;
using CachingProxy.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace CachingProxy.Helpers
{
    public class RedisResponseCache : IResponseCache
    {
        public TimeSpan? AbsoluteExpireTime { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan? UnusedExpireTime { get; set; } = TimeSpan.FromSeconds(60);

        public IDistributedCache _cache { get; set; }

        public RedisResponseCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task CacheResponseAsync(string key, CachedResponse response)
        {
            await DistributedCacheExtensions.SetRecordAsync<CachedResponse>(_cache, key, response, AbsoluteExpireTime, UnusedExpireTime);
        }

        public async Task<CachedResponse?> GetCachedResponseAsync(string key)
        {
            return await DistributedCacheExtensions.GetRecordAsync<CachedResponse?>(_cache, key);
        }
    }
}
