namespace FluentHttp.Ext.LoadBalancing
{
    public class WeightedRandomLoadBalancingPolicy : ILoadBalancingPolicy
    {
        public string Name => LoadBalancingPolicies.WeightedRandom;

        public ServiceInfo? PickDestination(string appid, List<ServiceInfo> services)
        {
            if (services is null || services.Count == 0)
            {
                return null;
            }
            // 排除Weight 为 -1
            var validServices = services.Where(s => s.Weight > 0).ToList();
            if (validServices.Count == 0)
            {
                return null;
            }
            int  _totalWeight = validServices.Sum(i => i.Weight);
            long randomValue = Random.Shared.NextInt64(_totalWeight);
            long cumulative = 0;
            foreach (var item in validServices)
            {
                cumulative += item.Weight;
                if (randomValue < cumulative)
                {
                    return item;
                }
            }
            return validServices.Last();
        }
    }
}