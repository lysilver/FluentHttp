using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentHttp.Ext.LoadBalancing
{
    public class RoundRobinLoadBalancingPolicy : ILoadBalancingPolicy
    {
        private readonly ConditionalWeakTable<string, AtomicCounter> _counters = new();
        public string Name => LoadBalancingPolicies.RoundRobin;

        public ServiceInfo? PickDestination(string appid, List<ServiceInfo> services)
        {
            if (services is null || services.Count == 0)
            {
                return null;
            }
            var counter = _counters.GetOrCreateValue(appid);

            // Increment returns the new value and we want the first return value to be 0.
            var offset = counter.Increment() - 1;

            // Preventing negative indicies from being computed by masking off sign.
            // Ordering of index selection is consistent across all offsets.
            // There may be a discontinuity when the sign of offset changes.
            return services[(offset & 0x7FFFFFFF) % services.Count];
        }
    }
}