namespace FluentHttp.Ext
{
    public class ClusterConfig
    {
        public string AppId { get; set; } = null!;

        public string? LoadBalancingPolicy { get; set; }

        public List<ServiceInfo> Destinations { get; set; } = null!;
    }
}