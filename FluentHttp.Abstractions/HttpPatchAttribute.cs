using System;

namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpPatchAttribute : HttpAttribute
    {
        public bool UseJsonPatch { get; set; } = true;
    }
}