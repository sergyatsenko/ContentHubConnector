using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class JsonExtensions
    {
        public static T SelectValue<T>(this JToken jObj, string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) return default(T);

            var token = jObj.SelectToken(jsonPath);
            return token != null ? token.Value<T>() : default(T);
        }

        public static Dictionary<string, string> SelectMappedValues(this JToken jObj, Dictionary<string, string> mappings)
        {
            var fieldValues = new Dictionary<string, string>();
            foreach (var key in mappings.Keys)
            {
                if (!string.IsNullOrEmpty(mappings[key]))
                {
                    var value = jObj.SelectValue<string>(mappings[key]);
                    if (!string.IsNullOrEmpty(value))
                    {
                        fieldValues.Add(key, value);
                    }
                }
            }

            return fieldValues;
        }
    }
}
