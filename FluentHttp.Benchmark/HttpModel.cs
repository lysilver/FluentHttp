namespace FluentHttp.Benchmark
{
    public class HttpModel
    {
        public string AppId { get; set; } = null!;

        public string HttpMethod { get; set; } = "GET";

        public string Url { get; set; } = null!;
    }
}