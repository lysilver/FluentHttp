using FluentHttp.Ext;
using FluentHttp.Ext.LoadBalancing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YL.Extensions.DependencyInjection;

namespace FluentHttp.Test
{
    public class TestLoadBalancing
    {
        private readonly IDictionary<string, ILoadBalancingPolicy> _policies;
        private readonly List<ServiceInfo> serviceInfos;

        public TestLoadBalancing()
        {
            var services = new ServiceCollection();
            services.AddLoadBalancing();
            var policies = services.BuildServiceProvider()
                .GetServices(typeof(ILoadBalancingPolicy)) as IEnumerable<ILoadBalancingPolicy>;
            _policies = policies?.ToDictionaryByUniqueId(x => x.Name) ?? throw new ArgumentNullException(nameof(policies));
            serviceInfos = new List<ServiceInfo>()
            {
                new ServiceInfo
                {
                    BaseUrl="http://localhost:9000",
                    Weight = 1
                },new ServiceInfo
                {
                    BaseUrl="http://localhost:9002",
                    Weight = 2
                },
                new ServiceInfo
                {
                    BaseUrl="http://localhost:9003",
                    Weight = 3
                }
            };
        }

        [Theory]
        [InlineData("Random")]
        [InlineData("RoundRobin")]
        [InlineData("WeightedRandom")]
        public void TestRandomLoadBalancingPolicy(string policiy)
        {
            var policy = _policies.GetRequiredServiceById(policiy);
            var list = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var url = policy.PickDestination("order", serviceInfos)?.ToString();
                if (url is not null)
                {
                    list.Add(url);
                }
            }
            Assert.Equal(10, list.Count);
            Assert.All(serviceInfos, item =>
            {
                list.Contains(item.BaseUrl);
            });
        }

        [Fact]
        public void TestRoundRobinLoadBalancingPolicy()
        {
            var col = new ClusterConfig
            {
                AppId = "order",
                LoadBalancingPolicy = LoadBalancingPolicies.Random,
                Destinations = new List<ServiceInfo>()
                {
                    new ServiceInfo
                    {
                        BaseUrl="",
                    },
                }
            };
            var policy = _policies.GetRequiredServiceById(LoadBalancingPolicies.RoundRobin);
            var list = new List<string>();
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                var url = policy.PickDestination("order", serviceInfos)?.ToString();
                if (url is not null)
                {
                    list.Add(url);
                }
            }
            var group = list.GroupBy(c => c).Select(c => new
            {
                c.Key,
                Count = c.Count()
            }).ToList();
            Assert.Equal(count, list.Count);
        }

        [Fact]
        public async Task TestWeightedRandomLoadBalancingPolicy()
        {
            var policy = _policies.GetRequiredServiceById(LoadBalancingPolicies.WeightedRandom);
            var tasks = new List<Task>();
            int count = 0;
            ConcurrentStack<string> list = new ConcurrentStack<string>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var url = policy.PickDestination("order", serviceInfos)?.ToString();
                    if (url is not null)
                    {
                        list.Push(url);
                    }
                    Interlocked.Increment(ref count);
                }));
            }
            await Task.WhenAll([.. tasks]);
            var asd = list.GroupBy(c => c).ToList();
            tasks.Clear();
            Interlocked.Exchange(ref count, 0);
            var atomic = new AtomicCounterWeightedRandom<ServiceInfo>();
            ConcurrentStack<string> list2 = new ConcurrentStack<string>();
            serviceInfos.ForEach(c =>
            {
                atomic.Add(c, c.Weight);
            });
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                    {
                        var url = policy.PickDestination("order", serviceInfos)?.ToString();
                        if (url is not null)
                        {
                            list2.Push(url);
                        }
                        Interlocked.Increment(ref count);
                    }));
            }
            await Task.WhenAll([.. tasks]);
        }
    }
}