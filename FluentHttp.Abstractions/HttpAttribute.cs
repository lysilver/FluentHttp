using System;

namespace FluentHttp.Abstractions
{
    public class HttpAttribute : Attribute
    {
        public string AppId { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// 返回string 类型
        /// </summary>
        public bool ReturnString { get; set; }
    }
}