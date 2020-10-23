using System;
using System.Collections.Generic;
using System.Linq;


namespace RedditBotDetector.Extensions {
    public static class LinqExtensions {
        public static IEnumerable<T1> Distinct<T1, T2>(this IEnumerable<T1> self, Func<T1, T2> selector) {
            return self.GroupBy(selector)
                .Select(g => g.First());
        }
    }
}