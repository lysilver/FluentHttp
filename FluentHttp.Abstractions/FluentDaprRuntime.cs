using System;
using System.Collections.Generic;

namespace FluentHttp.Abstractions
{
    public class FluentDaprRuntime
    {
        private static readonly Lazy<FluentDaprRuntime> lazy =
        new Lazy<FluentDaprRuntime>(() => new FluentDaprRuntime());

        private static Dictionary<string, Func<Dictionary<string, object>>> keys = new Dictionary<string, Func<Dictionary<string, object>>>();

        private FluentDaprRuntime()
        { }

        public static FluentDaprRuntime Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public void AddHeader(string key, Func<Dictionary<string, object>> func)
        {
            keys.Add(key, func);
        }

        public Func<Dictionary<string, object>> GetHeader(string key)
        {
            keys.TryGetValue(key, out Func<Dictionary<string, object>> header);
            return header;
        }
    }
}