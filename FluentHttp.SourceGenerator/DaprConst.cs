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
                    { "PATCH", "FluentHttp.Abstractions.HttpPatchAttribute" },
                    { "HEAD", "FluentHttp.Abstractions.HttpHeadAttribute" },
                    { "PATH", "FluentHttp.Abstractions.PathVariableAttribute" },
                    { "FilePath","FluentHttp.Abstractions.FilePathVariableAttribute" },
                    { "HttpFileUpload","FluentHttp.Abstractions.HttpFileUploadAttribute" },
                    { "HttpFileUploadStream","FluentHttp.Abstractions.HttpFileUploadStreamAttribute" },
                    { "HttpResponseMessage","FluentHttp.Abstractions.HttpResponseMessageAttribute" }
                };

        public static readonly string Get = "GET";
        public static readonly string Path = "PATH";
        public static readonly string Post = "POST";
        public static readonly string Delete = "DELETE";

        public static readonly string Head = "HEAD";

        public static readonly string AppId = "AppId";

        public static readonly string Url = "Url";

        public static readonly string HttpReturnType = "HttpReturnType";

        public static readonly string Name = "Name";

        public static readonly string ClientName = "ClientName";

        public static readonly string UseJsonPatch = "UseJsonPatch";

        public static readonly string UrlPrefix = "/";

        public static readonly string MemberHttpMethod = "HttpMethod";

        /// <summary>
        /// Environment.NewLine 分析器不能用，需要禁止分析器
        /// </summary>
        public static string NewLine { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n";
    }
}