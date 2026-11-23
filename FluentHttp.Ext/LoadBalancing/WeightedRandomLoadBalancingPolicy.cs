namespace FluentHttp.Ext.LoadBalancing
{
    public sealed class WeightedRandomLoadBalancingPolicy : ILoadBalancingPolicy
    {
        public string Name => LoadBalancingPolicies.WeightedRandom;

        public ServiceInfo? PickDestination(string appid, List<ServiceInfo> services)
        {
            if (services is null || services.Count == 0)
            {
                return null;
            }

            // 排除Weight 为非正值
            ServiceInfo? lastValid = null;
            int totalWeight = 0;
            foreach (var service in services)
            {
                if (service.Weight > 0)
                {
                    lastValid = service;
                    totalWeight += service.Weight;
                }
            }
            if (lastValid is null)
            {
                return null;
            }

            long randomValue = Random.Shared.NextInt64(totalWeight);
            long cumulative = 0;
            foreach (var service in services)
            {
                if (service.Weight <= 0)
                {
                    continue;
                }
                cumulative += service.Weight;
                if (randomValue < cumulative)
                {
                    return service;
                }
            }
            return lastValid;
        }
    }
}