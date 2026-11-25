using System.Text.Json;

namespace FluentHttp.Ext
{
    /// <summary>
    /// 上下文
    /// </summary>
    public interface IContext
    {
        ValueTask ConfigureRequest(string appId, HttpRequestMessage request);

        /// <summary>
        /// 获取指定服务的 或者指定接口 JSON 序列化配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        JsonSerializerOptions? GetJsonSerializerOptions(string appId);
    }
}