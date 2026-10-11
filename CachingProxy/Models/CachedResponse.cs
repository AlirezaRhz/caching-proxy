using System.Net;

namespace CachingProxy.Models
{
    public class CachedResponse
    {
        public int StatusCode { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public byte[] Body { get; set; } = default!;
    }
}
