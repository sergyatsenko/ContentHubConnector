using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    /// <summary>
    /// Import mapped Content Hub entity into SellableItem in SXC
    /// </summary>
    public class ImportSellableItemFromContentHubCommand : CommerceCommand
    {
        private readonly IImportSellableItemFromContentHubPipeline _pipeline;

        public ImportSellableItemFromContentHubCommand(IImportSellableItemFromContentHubPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        public async Task<ImportCatalogEntityArgument> Process(CommerceContext commerceContext, ImportCatalogEntityArgument args)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var result = await this._pipeline.Run(args, new CommercePipelineExecutionContextOptions(commerceContext));
                return result;
            }
        }
    }
}