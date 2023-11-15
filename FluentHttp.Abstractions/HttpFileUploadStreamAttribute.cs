using System;

namespace FluentHttp.Abstractions
{
    /// <summary>
    /// 通过文件流上传文件
    /// 参数最多只能有2个
    /// 第二个为formdata参数 类型为字典类型
    /// 单文件参数类型 FileStream
    /// 多文件 List<FileStream>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpFileUploadStreamAttribute : Attribute
    {
    }
}