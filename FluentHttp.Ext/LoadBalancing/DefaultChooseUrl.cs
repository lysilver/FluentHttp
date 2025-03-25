namespace FluentHttp.Ext.LoadBalancing
{
    public class DefaultChooseUrl : IChooseUrl
    {
        private readonly IEnumerable<ILoadBalancingPolicy> _policies;
        private readonly IDictionary<string, ILoadBalancingPolicy> _policiesDict;

        public DefaultChooseUrl(IEnumerable<ILoadBalancingPolicy> policies)
        {
            _policies = policies;
            _policiesDict = _policies.ToDictionaryByUniqueId(x => x.Name) ?? throw new ArgumentNullException(nameof(policies));
        }

        public string? GetUrl(ClusterConfig clusterConfig)
        {
            var policy = _policiesDict.GetRequiredServiceById(clusterConfig.LoadBalancingPolicy);
            var service = policy.PickDestination(clusterConfig.AppId, clusterConfig.Destinations);
            return service?.BaseUrl;
        }
    }
}