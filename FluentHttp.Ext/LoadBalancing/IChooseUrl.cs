namespace FluentHttp.Ext.LoadBalancing
{
    public interface IChooseUrl
    {
        string? GetUrl(ClusterConfig clusterConfig);
    }
}