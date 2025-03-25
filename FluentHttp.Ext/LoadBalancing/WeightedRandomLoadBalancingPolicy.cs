namespace FluentHttp.Ext.LoadBalancing
{
    public class WeightedRandomLoadBalancingPolicy : ILoadBalancingPolicy
    {
        public string Name => LoadBalancingPolicies.WeightedRandom;
        private int _totalWeight;

        public ServiceInfo? PickDestination(string appid, List<ServiceInfo> services)
        {
            if (services is null || services.Count == 0)
            {
                return null;
            }
            // 排除Weight 为 -1
            services = services.Where(c => c.Weight > -1).ToList();
            _totalWeight = services.Sum(i => i.Weight);
            double randomValue = Random.Shared.NextDouble() * _totalWeight;
            foreach (var item in services)
            {
                if (randomValue < item.Weight)
                {
                    return item;
                }
                Interlocked.Add(ref _totalWeight, -item.Weight);
                randomValue -= item.Weight;
            }
            return null;
        }
    }
}