using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class AzureServiceBusQueuePolicy : Policy
    {
        public string TokenCacheName { get; set; }
        public string ProtocolAndHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
