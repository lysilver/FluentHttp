using System.Collections.Generic;

namespace FluentHttp.Abstractions
{
    public static class DaprConst
    {
        public static readonly Dictionary<string, string> HttpMethod = new Dictionary<string, string>
                {
                    { "HTTP", "FluentHttp.Abstractions.HttpAttribute" },
                    { "GET", "FluentHttp.Abstractions.HttpGetAttribute" },
                    { "POST", "FluentHttp.Abstractions.HttpPostAttribute" },
                    { "DELETE", "FluentHttp.Abstractions.HttpDeleteAttribute" },
                    { "PUT", "FluentHttp.Abstractions.HttpPutAttribute" },
                    { "PATH", "FluentHttp.Abstractions.PathVariableAttribute" }
                };

        public static readonly string Dapr = "DAPR";
        public static readonly string Get = "GET";
        public static readonly string Path = "PATH";
        public static readonly string Post = "POST";
        public static readonly string Delete = "DELETE";

        public static readonly string Header = "Header";

        public static readonly string AppId = "AppId";

        public static readonly string Url = "Url";
    }
}