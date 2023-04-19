using FluentHttp.Ext.LoadBalancing;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FluentHttp.Ext
{
    public class DefaultContext : IContext
    {
        private readonly Func<Dictionary<string, object>> func = () =>
        {
            Dictionary<string, object> keys = new()
                {
                    { "trace_id", Guid.NewGuid().ToString()}
                };
            return keys;
        };

        private readonly string Node = "YL:FluentHttp";
        private readonly IConfiguration _configuration;

        public DefaultContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<Dictionary<string, object>> GetHeader(string appId)
        {
            return Task.FromResult(func.Invoke());
        }

        public Task<Dictionary<string, ClusterConfig>?> GetUrls()
        {
            var services = _configuration.GetSection(Node).Get<Dictionary<string, ClusterConfig>>();
            return Task.FromResult(services);
        }

        public JsonSerializerOptions? JsonSerializerOptions()
        {
            return null;
        }
    }
}