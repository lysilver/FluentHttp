using FluentHttp.Ext.LoadBalancing;
using System.Text.Json;

namespace FluentHttp.Ext
{
    public sealed class HttpClientAdapter : IHttpClientAdapter
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

        public ValueTask ConfigureRequest(string appId, HttpRequestMessage request)
        {
            return _context.ConfigureRequest(appId, request);
        }

        public JsonSerializerOptions? GetJsonSerializerOptions(string appId)
        {
            return _context.GetJsonSerializerOptions(appId);
        }

        public async ValueTask<string> GetUrl(string appId, string url)
        {
            if (string.IsNullOrWhiteSpace(appId) || _serviceDiscovery is null)
            {
                return url;
            }
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
                    baseUrl = cluster.Destinations?.FirstOrDefault()?.BaseUrl;
                }
                // client.BaseAddress = new Uri(baseUrl);
                if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
                {
                    var path = url?.TrimStart('/') ?? string.Empty;
                    url = baseUrl.TrimEnd('/') + (path.Length == 0 ? string.Empty : "/" + path);
                }
            }
            return url;
        }

        public ValueTask<Dictionary<string, ClusterConfig>> GetUrls()
        {
            if (_serviceDiscovery is null)
            {
                return ValueTask.FromResult(new Dictionary<string, ClusterConfig>());
            }
            return _serviceDiscovery.GetUrls();
        }

        public JsonSerializerOptions? JsonSerializerOptions(string appId)
        {
            return _context.GetJsonSerializerOptions(appId);
        }
    }
}