using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class MappingPolicyBase : Policy
    {
        public string SyncedItemsList { get; set; }
        public List<MappingConfiguration> MappingConfigurations { get; set; }
    }
}
