using System.Collections.Generic;
using System.Linq;

namespace FluentHttp.SourceGenerator
{
    public static class StringExt
    {
        public static string ToQueryString(this Dictionary<string, object> valuePairs)
        {
            var properties = from p in valuePairs
                             where p.Value != null
                             select p.Key + "=" + p.Value.ToString() + "";
            return string.Join("&", properties.ToArray());
        }
    }
}