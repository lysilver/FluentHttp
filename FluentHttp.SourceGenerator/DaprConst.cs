using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluentHttp.SourceGenerator
{
    public static class DaprConst
    {
        public static readonly Dictionary<string, string> HttpMethod = new Dictionary<string, string>
                {
                    { "HTTP", "FluentHttp.Abstractions.FluentHttpAttribute" },
                    { "GET", "FluentHttp.Abstractions.HttpGetAttribute" },
                    { "POST", "FluentHttp.Abstractions.HttpPostAttribute" },
                    { "DELETE", "FluentHttp.Abstractions.HttpDeleteAttribute" },
                    { "PUT", "FluentHttp.Abstractions.HttpPutAttribute" },
                    { "PATH", "FluentHttp.Abstractions.PathVariableAttribute" },
                    { "FilePath","FluentHttp.Abstractions.FilePathVariableAttribute" },
                    { "HttpFileUpload","FluentHttp.Abstractions.HttpFileUploadAttribute" }
                };

        public static readonly string Dapr = "DAPR";
        public static readonly string Get = "GET";
        public static readonly string Path = "PATH";
        public static readonly string Post = "POST";
        public static readonly string Delete = "DELETE";

        public static readonly string Header = "Header";

        public static readonly string AppId = "AppId";

        public static readonly string Url = "Url";

        public static readonly string ReturnString = "ReturnString";

        public static readonly string Name = "Name";

        public static readonly string Auth = "Auth";

        public static readonly string UrlPrefix = "/";

        /// <summary>
        /// Environment.NewLine 分析器不能用，需要禁止分析器
        /// </summary>
        public static string NewLine { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n";
    }
}