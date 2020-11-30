using Newtonsoft.Json.Linq;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class JsonDataModel : Model
    {
        public JsonDataModel(JObject jsonData)
        {
            Condition.Requires<JObject>(jsonData).IsNotNull("jsonData can not be null");
            this.JsonData = jsonData;
        }
        public object JsonData { get; set; }
    }
}
