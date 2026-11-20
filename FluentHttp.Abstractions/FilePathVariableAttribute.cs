using System;

/// <summary>
/// path
/// </summary>
namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FilePathVariableAttribute : Attribute
    {
    }
}