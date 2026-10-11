using CachingProxy.Models;

namespace CachingProxy.Interfaces
{
    public interface IResponseCache
    {
        Task CacheResponseAsync(string key, CachedResponse response);

        Task<CachedResponse?> GetCachedResponseAsync(string key);
    }
}
