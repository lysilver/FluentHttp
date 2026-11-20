using System;

namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HttpAttribute : Attribute
    {
        public string? AppId { get; set; }

        /// <summary>
        /// Url 前缀为/ 不会拼接FluentHttp特性上的Url
        /// 否则拼接特性上的Url
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 返回类型
        /// Json 默认反序列化为 TResponse
        /// String 返回原始字符串
        /// ResponseMessage 返回 HttpResponseMessage
        /// Stream 返回 Stream
        /// </summary>
        public HttpReturnType HttpReturnType { get; set; } = HttpReturnType.Json;
    }
}