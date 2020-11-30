using Sitecore.Commerce.Core;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class ContentHubConnectionPolicy : Policy
    {
        public List<ContentHubConnectionSettings> Connections { get; set; }
    }
}
