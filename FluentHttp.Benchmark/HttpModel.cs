namespace FluentHttp.Benchmark
{
    public sealed class HttpModel
    {
        public string AppId { get; set; } = null!;

        public string HttpMethod { get; set; } = "GET";

        public string Url { get; set; } = null!;
    }
}