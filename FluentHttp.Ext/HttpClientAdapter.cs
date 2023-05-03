using FluentHttp.Ext.LoadBalancing;

namespace FluentHttp.Ext
{
    public class HttpClientAdapter : IHttpClientAdapter
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IContext _context;
        private readonly IChooseUrl _chooseUrl;

        public HttpClientAdapter(IHttpClientFactory clientFactory, IContext context, IChooseUrl chooseUrl)
        {
            _clientFactory = clientFactory;
            _context = context;
            _chooseUrl = chooseUrl;
        }

        public async Task<TResponse?> Http<TResponse, TRequest>(HttpRequestValues<TRequest> httpRequest)
        {
            using var client = _clientFactory.CreateClient();
            httpRequest.Url = HttpExt.GetUrl(_context, _chooseUrl, httpRequest.AppId, httpRequest.Url);
            return await client.Http<TResponse, TRequest>(_context, httpRequest);
        }
    }
}