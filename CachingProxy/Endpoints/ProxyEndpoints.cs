
namespace CachingProxy.Endpoints
{
    public static class ProxyEndpoints
    {
        public static void MapProxyEndpoints(this WebApplication app)
        {
            app.MapGet("/", async (IHttpClientFactory factory) =>
            {
                HttpClient client = factory.CreateClient("origin");
                var response = await client.GetAsync("/");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Problem("Couldn't reach proxy Origin");
                }

                return Results.Ok(5);
            });
        }
    }
}
