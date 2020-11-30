using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Ryder.Commerce.CatalogSync.Pipelines;
using Plugin.Ryder.Commerce.CatalogSync.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Ryder.Commerce.CatalogSync.Commands
{
    public class SyncProductToChannelAdvisorCommand : CommerceCommand
    {
        private readonly ISyncProductToChannelAdvisorPipline _pipeline;

        public SyncProductToChannelAdvisorCommand(ISyncProductToChannelAdvisorPipline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        public async Task<SyncProductToChannelAdvisorArgument> Process(CommerceContext commerceContext)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var arg = new SyncProductToChannelAdvisorArgument(new SellableItem());
                var result = await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));
                return result;
            }
        }
    }
}