using FluentHttp.Ext.LoadBalancing;
using System.Text.Json;

namespace FluentHttp.Ext
{
    public class HttpClientAdapter : IHttpClientAdapter
    {
        private readonly IContext _context;
        private readonly IChooseUrl _chooseUrl;
        private readonly IServiceDiscovery _serviceDiscovery;

        public HttpClientAdapter(IContext context, IChooseUrl chooseUrl,
            IServiceDiscovery serviceDiscovery)
        {
            _context = context;
            _chooseUrl = chooseUrl;
            _serviceDiscovery = serviceDiscovery;
        }

        public async Task<Dictionary<string, object>?> GetHeader(string appId)
        {
            if (_context is null)
            {
                return null;
            }
            return await _context.GetHeader(appId);
        }

        public async Task<string> GetUrl(string appId, string url)
        {
            if (_serviceDiscovery is not null)
            {
                var services = await _serviceDiscovery.GetUrls();
                if (services is null)
                {
                    return url;
                }
                if (services.TryGetValue(appId, out ClusterConfig? cluster) && cluster is not null)
                {
                    string? baseUrl;
                    if (_chooseUrl is not null)
                    {
                        baseUrl = _chooseUrl.GetUrl(cluster);
                    }
                    else
                    {
                        baseUrl = cluster.Destinations.FirstOrDefault()?.BaseUrl;
                    }
                    // client.BaseAddress = new Uri(baseUrl);
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        url = $"{baseUrl}/{url}";
                    }
                }
            }
            return url;
        }

        public Task<Dictionary<string, ClusterConfig>?> GetUrls()
        {
            return _serviceDiscovery.GetUrls();
        }

        public JsonSerializerOptions? JsonSerializerOptions()
        {
            return _context.JsonSerializerOptions();
        }
    }
}