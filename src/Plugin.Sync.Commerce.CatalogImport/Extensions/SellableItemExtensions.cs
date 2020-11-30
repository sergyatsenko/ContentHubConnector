using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;

namespace Plugin.Sync.Commerce.CatalogImport.Extensions
{
    public static class SellableItemExtensions
    {
        //public static void AddListPrice(this ImportSellableItemResponse sellableItem, decimal sellableItemListPrice)
        //{
        //    var listPricingPolicy = sellableItem.SellableItem.GetPolicy<ListPricingPolicy>();
        //    listPricingPolicy.ClearPrices();
        //    if (sellableItemListPrice == 0)
        //    {
        //        return;
        //    }
        //    listPricingPolicy.AddPrice(new Money("USD", sellableItemListPrice));
        //}

        public static decimal GetLisPrice(this SellableItem sellableItem)
        {
            if (!sellableItem.HasPolicy(typeof(ListPricingPolicy)))
            {
                return 0;
            }
            var listPricingPolicy = sellableItem.GetPolicy<ListPricingPolicy>();
            if (listPricingPolicy== null  || listPricingPolicy.Prices==null || listPricingPolicy.Prices.Count() < 1)
            {
                return 0;
            }

            return listPricingPolicy.Prices.ToList()[0].Amount;
        }

        public static void AddVariantListPrice(this ImportCatalogEntityResponse sellableItem, decimal sellableItemListPrice, ItemVariationComponent variant)
        {
            if (sellableItemListPrice == 0)
            {
                return;
            }

            var listPricingPolicy = variant.GetPolicy<ListPricingPolicy>();
            listPricingPolicy.AddPrice(new Money("USD", sellableItemListPrice));
        }

        //public static void UpdateSellableItem(this SellableItemResponse sellableItem, Dictionary<string, string> sellableItemModel)
        //{
        //    sellableItem.SellableItem.Name = sellableItemModel["Name"];
        //    sellableItem.SellableItem.DisplayName = sellableItemModel["DisplayName"];
        //    sellableItem.SellableItem.Tags = new List<Tag>(){new Tag(sellableItemModel["SamClass"])};
        //}
    }
}