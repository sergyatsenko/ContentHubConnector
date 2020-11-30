using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogExport.Util
{
    public static class JsonExtensions
    {
        public static T SelectValue<T>(this JToken jObj, string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) return default(T);

            var token = jObj.SelectToken(jsonPath);
            return token != null ? token.Value<T>() : default(T);
        }
    }
}
