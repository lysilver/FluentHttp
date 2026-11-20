namespace FluentHttp.Ext
{
    public sealed class ServiceInfo
    {
        /// <summary>
        /// url
        /// </summary>
        public string BaseUrl { get; set; } = null!;

        public int Weight { get; set; } = -1;

        public override string ToString()
        {
            return $"Weight:{Weight},BaseUrl:{BaseUrl}";
        }
    }

    public enum HttpContentType
    {
        Json,
        JsonPatch,
        FormUrlEncoded,
        MultipartFormData
    }
}