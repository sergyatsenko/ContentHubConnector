using Newtonsoft.Json.Linq;
using Sitecore.Framework.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Dictionary<string, string> QueryMappedValuesFromRoot(this JToken jData, IEnumerable<string> rootPaths)
        {
            var results = new Dictionary<string, string>();
            if (rootPaths != null)
            {
                foreach (var rootPath in rootPaths)
                {
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        var rootToken = jData.SelectToken(rootPath);
                        if (rootToken != null)
                        {
                            //var dict = rootToken.ToDictionary<string>();
                            foreach (var prop in rootToken.Children<JProperty>())
                            {
                                if (prop != null && !string.IsNullOrEmpty(prop.Name) && prop.Value != null && prop.Value.Type != JTokenType.Object)
                                {
                                    results.Add(prop.Name, (string)prop);
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }

        public static T QueryMappedValue<T>(this JToken jData, string fieldName, string fieldPath, IEnumerable<string> rootPaths)
        {
            if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldPath))
            {
                return jData.SelectValue<T>(fieldPath);
            }

            foreach (var rootPath in rootPaths)
            {
                if (!string.IsNullOrEmpty(rootPath))
                {
                    var rootToken = jData.SelectToken(rootPath);
                    if (rootToken != null)
                    {
                        return rootToken.SelectValue<T>(rootPath);
                    }
                }
            }

            return default(T);
        }
    }
}
