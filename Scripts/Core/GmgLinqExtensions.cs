using System.Collections.Generic;
using System.Linq;

namespace GestureManager.Scripts.Core
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgLinqExtensions
    {
        private static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
        {
            return items.Select((item, inx) => new {item, inx})
                .GroupBy(x => x.inx / maxItems)
                .Select(grouping => grouping.Select(x => x.item));
        }

        public static IEnumerable<(T, T, bool)> BatchTwo<T>(this IEnumerable<T> items)
        {
            return from tuple in items.Batch(2)
                select tuple.ToList()
                into list
                let lPair = list[0]
                let rPair = list.Count > 1 ? list[1] : list[0]
                select (lPair, rPair, list.Count > 1);
        }
    }
}