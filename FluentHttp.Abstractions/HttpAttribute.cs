using System;

namespace FluentHttp.Abstractions
{
    public class HttpAttribute : Attribute
    {
        public string AppId { get; set; }

        /// <summary>
        /// Url 前缀为/ 不会拼接FluentHttp特性上的Url
        /// 否则拼接特性上的Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 返回string 类型
        /// </summary>
        public bool ReturnString { get; set; }
    }
}