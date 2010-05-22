using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NekoVampire.Extension.Collections;

namespace NekoVampire.Extension.Collections
{
    public static class IDictionary
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> value)
        {
            foreach (var val in value)
            {
                try
                {
                    dictionary.Add(val);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static IDictionary<TDictionaryKey, TValue> Sort<TDictionaryKey, TValue, TSortKey>(this IDictionary<TDictionaryKey, TValue> dictionary, Func<KeyValuePair<TDictionaryKey, TValue>, TSortKey> keySelector, IComparer<TSortKey> comparer)
        {
            return dictionary.QuickSort(keySelector, comparer ?? Comparer<TSortKey>.Default).ToDictionary(item => item.Key, item => item.Value);
        }

        public static IDictionary<TDictionaryKey, TValue> Sort<TDictionaryKey, TValue, TSortKey>(this IDictionary<TDictionaryKey, TValue> dictionary, Func<KeyValuePair<TDictionaryKey, TValue>, TSortKey> keySelector)
        {
            return dictionary.Sort(keySelector, Comparer<TSortKey>.Default);
        }

        public static IDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            return dictionary.Sort(x => x.Key, comparer ?? Comparer<TKey>.Default);
        }

        public static IDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary.Sort(x => x.Key, Comparer<TKey>.Default);
        }
    }
}
