using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHttp.Ext.LoadBalancing
{
    public static class ServiceLookupHelper
    {
        public static IDictionary<string, T> ToDictionaryByUniqueId<T>(this IEnumerable<T> services, Func<T, string> idSelector)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            foreach (var service in services)
            {
                if (!result.TryAdd(idSelector(service), service))
                {
                    throw new ArgumentException($"More than one {typeof(T)} found with the same identifier.", nameof(services));
                }
            }

            return result;
        }

        public static T GetRequiredServiceById<T>(this IDictionary<string, T> services, string? id)
        {
            var lookup = string.IsNullOrWhiteSpace(id) ? LoadBalancingPolicies.Random : id;
            if (!services.TryGetValue(lookup, out var result))
            {
                throw new ArgumentException($"No {typeof(T)} was found for the id '{lookup}'.", nameof(id));
            }
            return result;
        }
    }
}