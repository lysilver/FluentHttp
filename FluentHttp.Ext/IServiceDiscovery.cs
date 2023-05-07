namespace FluentHttp.Ext
{
    public interface IServiceDiscovery
    {
        Task<Dictionary<string, ClusterConfig>?> GetUrls();
    }
}