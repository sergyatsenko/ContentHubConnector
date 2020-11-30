using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Ryder.Commerce.CatalogSync.Entities;
using Plugin.Ryder.Commerce.CatalogSync.Extensions;
using Plugin.Ryder.Commerce.CatalogSync.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;
using Serilog;
using Newtonsoft.Json;
using Plugin.Ryder.Commerce.CatalogSync.Policies;
using Plugin.Ryder.Commerce.ChannelAdvSync.Helpers;
using Plugin.Ryder.Commerce.ChannelAdvSync.Models;

namespace Plugin.Ryder.Commerce.CatalogSync.Pipelines.Blocks
{
    [PipelineDisplayName("SyncProductToChannelAdvisorBlock")]
    public class SyncProductToChannelAdvisorBlock : PipelineBlock<SyncProductToChannelAdvisorArgument, SyncProductToChannelAdvisorArgument, CommercePipelineExecutionContext>
    {
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;

        public SyncProductToChannelAdvisorBlock(ISyncSellableItemPipeline SyncProductToChannelAdvisorPipline, IFindEntitiesInListPipeline findEntitiesInListPipeline)
        {
            _findEntitiesInListPipeline = findEntitiesInListPipeline;
        }

        public override async Task<SyncProductToChannelAdvisorArgument> Run(SyncProductToChannelAdvisorArgument arg, CommercePipelineExecutionContext context)
        {
            var clientHelper = new ClientHelper(context.CommerceContext);
            var policy = context.CommerceContext.GetPolicy<SellableItemMappingPolicy>();

            FindEntitiesInListArgument findResult = await this._findEntitiesInListPipeline.Run(new FindEntitiesInListArgument(typeof(SellableItem), policy.CreatedSellableItems, 0, 1), context);
            //Push item to channel advisor
            if (findResult != null && findResult.List != null && findResult.List.TotalItemCount > 0)
            {
                var response = clientHelper.CreatePostRequest(CAMapper.ConvertSellableItemToProductRequest(findResult.List.Items[0] as SellableItem));
                arg.IsSuccesful = response.IsSuccessful;
                arg.ErrorMessage = response.ErrorMessage;
                arg.ResponseContent = response.Content;
            }
            else
            {
                arg.ResponseContent = "No products found to push to channel advisor.";
            }
            
            return await Task.FromResult(arg);
        }
        
    }
}