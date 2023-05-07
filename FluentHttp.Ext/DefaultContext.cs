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

        public DefaultContext()
        {
        }

        public Task<Dictionary<string, object>> GetHeader(string appId)
        {
            return Task.FromResult(func.Invoke());
        }

        public JsonSerializerOptions? JsonSerializerOptions()
        {
            return null;
        }
    }
}