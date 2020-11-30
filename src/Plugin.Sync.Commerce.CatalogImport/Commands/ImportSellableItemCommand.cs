using System;
using System.Threading.Tasks;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    public class ImportSellableItemCommand : CommerceCommand
    {
        private readonly IImportSellableItemPipeline _pipeline;

        public ImportSellableItemCommand(IImportSellableItemPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
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