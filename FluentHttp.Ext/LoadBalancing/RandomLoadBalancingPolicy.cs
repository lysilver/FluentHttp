using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHttp.Ext.LoadBalancing
{
    public class RandomLoadBalancingPolicy : ILoadBalancingPolicy
    {
        public string Name => LoadBalancingPolicies.Random;

        public ServiceInfo? PickDestination(string appid, List<ServiceInfo> services)
        {
            if (services is null || services.Count == 0)
            {
                return null;
            }
            return services[Random.Shared.Next(services.Count)];
        }
    }
}