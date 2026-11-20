using System.Text.Json;

namespace FluentHttp.Ext
{
    public interface IHttpClientAdapter
    {
        /// <summary>
        /// url
        /// </summary>
        /// <returns></returns>
        ValueTask<Dictionary<string, ClusterConfig>> GetUrls();

        ValueTask<string> GetUrl(string appId, string url);

        ValueTask ConfigureRequest(string appId, HttpRequestMessage request);

        /// <summary>
        /// 获取指定服务的 JSON 序列化配置
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        JsonSerializerOptions? GetJsonSerializerOptions(string appId);
    }
}