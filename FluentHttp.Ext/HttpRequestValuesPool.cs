namespace FluentHttp.Ext
{
    public class HttpRequestValuesPool<T> : AbstractObjectPool<HttpRequestValues<T>>
    {
        public HttpRequestValuesPool() : base()
        {
        }

        public HttpRequestValuesPool(int poolSize) : base(poolSize)
        {
        }

        public override HttpRequestValues<T> CreateInstance()
        {
            return new HttpRequestValues<T>();
        }
    }
}