namespace FluentHttp.Ext
{
    public interface IHttpClientAdapter
    {
        Task<TResponse?> Http<TResponse, TRequest>(HttpRequestValues<TRequest> httpRequest);
    }
}