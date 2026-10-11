using CachingProxy.Helpers;
using CachingProxy.Interfaces;
using CachingProxy.Models;

namespace CachingProxy.Endpoints
{
    public static class ProxyEndpoints
    {
        public static void MapProxyEndpoints(this WebApplication app)
        {
            app.MapGet("/{**path}", async (HttpContext httpContext, IHttpClientFactory factory, IResponseCache cache, string? path) =>
            {
                // Create Key by getting the BaseAddress From a generated client.
                HttpClient client = factory.CreateClient("origin");

                string cacheKey = $"GET:{client.BaseAddress}{path}{httpContext.Request.QueryString}";

                // If Cache HIT :
                if (await cache.GetCachedResponseAsync(cacheKey) is CachedResponse cachedResponseData)
                {
                    httpContext.Response.StatusCode = cachedResponseData.StatusCode;
                    foreach (var (name, value) in cachedResponseData.Headers)
                    {
                        httpContext.Response.Headers[name] = value;
                    }
                    httpContext.Response.Headers["X-Cache"] = "HIT";
                    await httpContext.Response.Body.WriteAsync(cachedResponseData.Body);
                    return;
                }

                // If cache didnt HIT :
                string requestPath = $"{path}{httpContext.Request.QueryString}";
                using var response = await client.GetAsync(requestPath, httpContext.RequestAborted);

                // Cache the response :
                CachedResponse responseToCache = new CachedResponse()
                {
                    StatusCode = (int)response.StatusCode,
                    Body = await response.Content.ReadAsByteArrayAsync()
                };
                foreach (var (name, value) in response.Headers)
                {
                    responseToCache.Headers[name] = string.Join(", ", value);
                }
                foreach (var (name, value) in response.Content.Headers)
                {
                    responseToCache.Headers[name] = string.Join(", ", value);
                }

                // Remove Transfer-Encoding Before Caching
                responseToCache.Headers.Remove("Transfer-Encoding");

                if (response.IsSuccessStatusCode)
                {
                    await cache.CacheResponseAsync(cacheKey, responseToCache);
                }

                // forward the response
                httpContext.Response.StatusCode = ((int)response.StatusCode);
                foreach (var (name, value) in responseToCache.Headers)
                {
                    httpContext.Response.Headers[name] = value;
                }
                httpContext.Response.Headers["X-Cache"] = "MISS";
                await httpContext.Response.Body.WriteAsync(responseToCache.Body);
                return;



            });
        }

    }
}
