using System;

/// <summary>
/// 只能放在接口类上
/// </summary>
namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class FluentHttpAttribute : Attribute
    {
        public string AppId { get; set; } = null!;

        /// <summary>
        /// baseUrl
        /// 方法Url 前缀为/ 不会拼接FluentHttp特性上的Url
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 实现的类名
        /// 为空 ITest => Test
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// HttpClient 的命名实例
        /// 用于 AddHttpClient("xxx") 注册
        /// </summary>
        public string? ClientName { get; set; }
    }
}