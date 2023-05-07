using Microsoft.Extensions.Configuration;

namespace FluentHttp.Ext
{
    public class DefaultServiceDiscovery : IServiceDiscovery
    {
        private readonly string Node = "YL:FluentHttp";
        private readonly IConfiguration _configuration;

        public DefaultServiceDiscovery(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<Dictionary<string, ClusterConfig>?> GetUrls()
        {
            var services = _configuration.GetSection(Node).Get<Dictionary<string, ClusterConfig>>();
            return Task.FromResult(services);
        }
    }
}