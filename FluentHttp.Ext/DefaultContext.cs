using System.Text.Json;

namespace FluentHttp.Ext
{
    public class DefaultContext : IContext
    {

        public DefaultContext()
        {
        }

        public ValueTask ConfigureRequest(string appId, HttpRequestMessage request)
        {
            return ValueTask.CompletedTask;
        }

        public JsonSerializerOptions? GetJsonSerializerOptions(string appId)
        {
            return null;
        }
    }
}