using System;

/// <summary>
/// path
/// </summary>
namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class PathVariableAttribute : Attribute
    {
    }
}