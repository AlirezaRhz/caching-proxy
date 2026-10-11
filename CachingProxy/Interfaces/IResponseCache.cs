using CachingProxy.Models;

namespace CachingProxy.Interfaces
{
    public interface IResponseCache
    {
        Task CacheResponseAsync(string key, CachedResponse response, TimeSpan? absoluteExpireTime, TimeSpan? unusedExpireTime);

        Task<CachedResponse?> GetCachedResponseAsync(string key);
    }
}
