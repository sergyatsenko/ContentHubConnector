using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Models
{
    public class CatalogEntityDataModel: Model
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string CatalogName { get; set; }
        //public string ParentCategoryName { get; set; }
        public Dictionary<string, string> EntityFields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string CommerceEntityId { get; set; }
        public List<string> ParentEntityIDs { get; set; }
        public List<string> ParentEntityNames { get; set; }
        public decimal? ListPrice { get; set; }
    }
}
