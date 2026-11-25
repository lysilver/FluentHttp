using System.Collections.Generic;

namespace FluentHttp.Abstractions
{

    public enum HttpReturnType
    {
        Json,           // 默认：反序列化为 TResponse
        String,         // 返回原始字符串
        ResponseMessage,// 返回 HttpResponseMessage
        Stream          // 返回 Stream
    }

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