using System.Text.Json;

namespace FluentHttp.Ext
{
    /// <summary>
    /// 上下文
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// 获取header
        /// 用于服务之间传递header
        /// string basic = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(options.User +":"+ options.Pwd));
        /// Authorization
        /// Authorization 会从Auth对应的value获取
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetHeader(string appId);

        /// <summary>
        /// url
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, ClusterConfig>?> GetUrls();

        /// <summary>
        /// JsonSerializerOptions 请使用单例构建
        /// </summary>
        /// <returns></returns>
        JsonSerializerOptions? JsonSerializerOptions();
    }
}