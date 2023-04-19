using System;

namespace FluentHttp.Abstractions
{
    /// <summary>
    /// 通过文件路径上传文件，文件的绝对路径
    /// 参数只能有一个
    /// 单文件参数类型 string
    /// 多文件 List<string>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpFileUploadAttribute : HttpAttribute
    {
    }
}