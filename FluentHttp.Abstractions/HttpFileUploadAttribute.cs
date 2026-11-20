using System;

namespace FluentHttp.Abstractions
{
    /// <summary>
    /// TODO
    /// 通过文件路径上传文件，文件的绝对路径
    /// 参数最多只能有2个
    /// 第二个为formdata参数 类型为字典类型
    /// 单文件参数类型 string
    /// 多文件 List<string>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpFileUploadAttribute : HttpAttribute
    {
    }
}