using CachingProxy.Endpoints;
using CachingProxy.Helpers;
using CachingProxy.Interfaces;
using StackExchange.Redis;
using System.CommandLine;

Option<int> portOption = new Option<int>("--port")
{
    Description = "The port to listen on.",
};

Option<string> urlOption = new Option<string>("--origin")
{
    Description = "The URL to proxy requests to.",
};

Option<bool?> clearCacheOption = new Option<bool?>("--clear-cache")
{
    Description = "Clear cached responses."
};

RootCommand rootCommand = new RootCommand()
{
    Description = "A caching proxy."
};

rootCommand.Options.Add(portOption);
rootCommand.Options.Add(urlOption);
rootCommand.Options.Add(clearCacheOption);

rootCommand.SetAction(parseResult =>
{
    if (parseResult.GetValue(portOption) is int proxyPort && parseResult.GetValue(urlOption) is string proxyUrl)
    {
        RunProxy(proxyPort, proxyUrl);
    }
    else if (parseResult.GetValue(clearCacheOption) is bool)
    {
        ClearCache();
    }
});

ParseResult result = rootCommand.Parse(args);
return result.Invoke();

void RunProxy(int port, string url)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("redis");
        options.InstanceName = "CachingProxy_"; // Used to not interfere with other running applications that are using Redis
    });

    // Basically : Whenever a class asks for an IResponseCache, give it a RedisResponseCache!
    builder.Services.AddSingleton<IResponseCache, RedisResponseCache>();

    if (!url.StartsWith("https://") && !url.StartsWith("http://"))
    {
        url = $"https://{url}";
    }

    // Register a new client to access the origin uri anywhere in the app with dependency injection
    builder.Services.AddHttpClient("origin", c => c.BaseAddress = new Uri(url));

    var app = builder.Build();

    app.MapProxyEndpoints();

    //// Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
    //    app.MapOpenApi();
    //}

    //app.UseHttpsRedirection();

    app.Run($"http://localhost:{port}");
}

void ClearCache()
{
    var builder = WebApplication.CreateBuilder();

    string connectionString =
        builder.Configuration.GetConnectionString("redis")!;

    using var redis =
        ConnectionMultiplexer.Connect(connectionString);

    var db = redis.GetDatabase();
    int deleted = 0;

    foreach (var endpoint in redis.GetEndPoints())
    {
        var server = redis.GetServer(endpoint);

        if (!server.IsConnected || server.IsReplica)
            continue;

        foreach (var key in server.Keys(
            database: db.Database,
            pattern: "CachingProxy_*",
            pageSize: 500))
        {
            if (db.KeyDelete(key))
                deleted++;
        }
    }

    Console.WriteLine($"Cleared {deleted} cached records.");
}