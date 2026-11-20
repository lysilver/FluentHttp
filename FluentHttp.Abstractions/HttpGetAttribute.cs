using System;

namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpGetAttribute : HttpAttribute
    {
    }
}