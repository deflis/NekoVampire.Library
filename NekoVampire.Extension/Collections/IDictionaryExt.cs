using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
