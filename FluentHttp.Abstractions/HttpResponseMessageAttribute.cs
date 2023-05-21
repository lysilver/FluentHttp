using System;

namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpResponseMessageAttribute : Attribute
    {
        public string AppId { get; set; }

        /// <summary>
        /// Url 前缀为/ 不会拼接FluentHttp特性上的Url
        /// 否则拼接特性上的Url
        /// </summary>
        public string Url { get; set; }

        public HttpMethodEnum HttpMethod { get; set; }
    }
}