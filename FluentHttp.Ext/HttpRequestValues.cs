namespace FluentHttp.Ext
{
    public class HttpRequestValues<T>
    {
        public HttpRequestValues(T? value, string appId, string url, string method, string? auth)
        {
            Value = value;
            AppId = appId;
            Url = url;
            Method = method;
            Auth = auth;
        }

        public T? Value { get; set; }

        public string AppId { get; set; } = null!;

        public string Url { get; set; } = null!;

        public string Method { get; set; } = null!;

        /// <summary>
        /// —È÷§
        /// </summary>
        public string? Auth { get; set; }

        public Dictionary<string, object>? Header { get; set; }
    }
}