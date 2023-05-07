using FluentHttp.Ext;
using FluentHttp.Ext.LoadBalancing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YL.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExt
    {
        /// <summary>
        /// 自行实现IContext
        /// 如果添加了自行替换
        /// services.Replace(ServiceDescriptor.Singleton(typeof(IDapr), typeof(DefaultContext)));
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpHeader(this IServiceCollection services)
        {
            services.TryAddSingleton<IContext, DefaultContext>();
            return services;
        }

        public static IServiceCollection AddLoadBalancing(this IServiceCollection services)
        {
            services.AddSingleton<ILoadBalancingPolicy, RandomLoadBalancingPolicy>();
            services.AddSingleton<ILoadBalancingPolicy, RoundRobinLoadBalancingPolicy>();
            services.AddSingleton<ILoadBalancingPolicy, WeightedRandomLoadBalancingPolicy>();
            services.AddSingleton<IChooseUrl, DefaultChooseUrl>();
            return services;
        }

        public static IServiceCollection AddFluentHttpPool(this IServiceCollection services)
        {
            services.AddSingleton(new HttpRequestValuesPool<object>(30));
            services.AddSingleton(new ObjectPool<HttpRequestValues<object>>(() =>
            {
                return new HttpRequestValues<object>();
            }, 30));
            return services;
        }
    }
}