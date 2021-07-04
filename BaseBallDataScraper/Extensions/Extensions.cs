using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseBallDataScraper.Extensions
{
    static class Extensions
    {
        public static IEnumerable<KeyValuePair<TKey, TValue>> Merge<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> firstDict,
            IEnumerable<KeyValuePair<TKey, TValue>> secondDict)
            => firstDict.Concat(secondDict).DistinctByKey();

        public static IEnumerable<KeyValuePair<TKey, TValue>> DistinctByKey<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
            => source.Distinct(OnlyKeyComparer<TKey, TValue>.Instance);

        public class OnlyKeyComparer<Tkey, TValue> : IEqualityComparer<KeyValuePair<Tkey, TValue>>
        {
            public static OnlyKeyComparer<Tkey, TValue> Instance { get; } = new OnlyKeyComparer<Tkey, TValue>();

            public bool Equals(KeyValuePair<Tkey, TValue> x, KeyValuePair<Tkey, TValue> y)
                => EqualityComparer<Tkey>.Default.Equals(x.Key, y.Key);

            public int GetHashCode(KeyValuePair<Tkey, TValue> obj) => obj.Key.GetHashCode();
        }
    }
}
