using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<T, S>(this IDictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Empty dictionary");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
            }
        }

    }
}