namespace FluentHttp.Benchmark
{
    internal sealed class HttpModelPool : AbstractObjectPool<HttpModel>
    {
        public HttpModelPool() : base()
        {
        }

        public HttpModelPool(int poolSize) : base(poolSize)
        {
        }

        public override HttpModel CreateInstance()
        {
            return new HttpModel();
        }
    }
}