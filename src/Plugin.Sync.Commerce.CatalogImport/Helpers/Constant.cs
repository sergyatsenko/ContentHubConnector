using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Helpers
{
    public static class Constant
    {
        public struct KnownCustomImagesView
        {
            public const string ViewName = "SellableItemCustomImages";
            public const string EditCustomImages = "EditCustomImages";
        }

        public struct KnownSellableItemStatusView
        {
            public const string ViewName = "SellableItemStatus";
            public const string EditSellableItemStatus = "EditSellableItemStatus";
        }
    }
}
