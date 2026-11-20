using Microsoft.Extensions.Options;

namespace FluentHttp.Ext
{
    public sealed class DefaultServiceDiscovery : IServiceDiscovery
    {
        private readonly Dictionary<string, ClusterConfig> _configs;

        public DefaultServiceDiscovery(IOptions<Dictionary<string, ClusterConfig>> options)
        {
            _configs = options.Value;
        }

        public ValueTask<Dictionary<string, ClusterConfig>> GetUrls()
        {
            return ValueTask.FromResult(_configs);
        }
    }
}