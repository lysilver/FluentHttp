namespace FluentHttp.Ext.LoadBalancing
{
    /// <summary>
    /// 代码来自 https://github.com/microsoft/reverse-proxy/
    /// </summary>
    public interface ILoadBalancingPolicy
    {
        string Name { get; }

        ServiceInfo? PickDestination(string appid, List<ServiceInfo> services);
    }
}