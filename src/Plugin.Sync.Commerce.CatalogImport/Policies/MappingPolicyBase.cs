using Sitecore.Commerce.Core;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class MappingPolicyBase : Policy
    {
        public string SyncedItemsList { get; set; }
        public List<MappingConfiguration> MappingConfigurations { get; set; }
    }
}
