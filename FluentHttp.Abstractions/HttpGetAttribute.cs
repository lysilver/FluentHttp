﻿using System;

namespace FluentHttp.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpGetAttribute : HttpAttribute
    {
    }
}