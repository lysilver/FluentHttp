namespace FluentHttp.Ext
{
    public interface IServiceDiscovery
    {
        ValueTask<Dictionary<string, ClusterConfig>> GetUrls();
    }
}