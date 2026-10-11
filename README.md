# CachingProxy

A command-line caching proxy server built with .NET 10 and ASP.NET Core. It forwards incoming requests to an origin server and stores the responses in Redis. Repeated requests are served straight from the cache instead of hitting the origin again.

## How it works

1. You start the proxy with a port and an origin URL.
2. A request arrives at `http://localhost:<port>/some/path`.
3. The proxy looks for a cached response for that request in Redis.
   - **Cache hit:** the stored response is returned immediately.
   - **Cache miss:** the request is forwarded to the origin, the response is stored in Redis, and then returned to the client.

All cache keys are prefixed with `CachingProxy_`, so the proxy can share a Redis instance with other applications without interfering with them.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running Redis server (default: `localhost:6379`)

```bash
docker run -d --name redis -p 6379:6379 redis
```

## Usage

```
CachingProxy --port <port> --origin <url>
CachingProxy --clear-cache
```

| Option          | Description                                                                   |
| --------------- | ----------------------------------------------------------------------------- |
| `--port`        | The port the proxy listens on.                                                |
| `--origin`      | The URL requests are proxied to. `https://` is assumed if no scheme is given. |
| `--clear-cache` | Deletes all cached responses created by this proxy, then exits.               |

### Start the proxy

From the project directory:

```bash
dotnet run -- --port 3000 --origin http://example.com
```

Or run the built executable:

```bash
./CachingProxy.exe --port 3000 --origin http://example.com
```

### Clear the cache

```bash
dotnet run -- --clear-cache
```

This only removes keys starting with `CachingProxy_`. Other data in your Redis instance is left untouched. It can be run while the proxy is running.

## Try it out

Start the proxy, then send the same request twice:

```bash
curl -s -o /dev/null -w "%{time_total}s\n" http://localhost:3000/
curl -s -o /dev/null -w "%{time_total}s\n" http://localhost:3000/
```

The first request goes to the origin and is noticeably slower. The second is served from Redis and should be much faster.

The proxy also has an X-Cache Header with MISS or HIT values
