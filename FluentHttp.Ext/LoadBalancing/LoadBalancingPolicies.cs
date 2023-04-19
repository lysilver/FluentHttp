namespace FluentHttp.Ext.LoadBalancing
{
    public static class LoadBalancingPolicies
    {
        /// <summary>
        /// 随机
        /// </summary>
        public static string Random => nameof(Random);

        /// <summary>
        /// 轮询
        /// </summary>
        public static string RoundRobin => nameof(RoundRobin);

        /// <summary>
        /// 加权随机
        /// </summary>
        public static string WeightedRandom => nameof(WeightedRandom);
    }
}