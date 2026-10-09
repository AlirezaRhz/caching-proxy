using CachingProxy.Endpoints;
using System.CommandLine;

Option<int> portOption = new Option<int>("--port")
{
    Description = "The port to listen on.",
    Required = true
};

Option<string> urlOption = new Option<string>("--origin")
{
    Description = "The URL to proxy requests to.",
    Required = true
};

RootCommand rootCommand = new RootCommand()
{
    Description = "A caching proxy."
};

rootCommand.Options.Add(portOption);
rootCommand.Options.Add(urlOption);

rootCommand.SetAction(parseResult =>
{
    if (parseResult.GetValue(portOption) is int proxyPort && parseResult.GetValue(urlOption) is string proxyUrl)
    {
        RunProxy(proxyPort, proxyUrl);
    }
});

ParseResult result = rootCommand.Parse(args);
return result.Invoke();

void RunProxy(int port, string url)
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddOpenApi();

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