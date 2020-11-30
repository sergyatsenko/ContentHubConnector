using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    public class ImportCategoryCommand : CommerceCommand
    {
        private readonly IImportCategoryPipeline _pipeline;

        public ImportCategoryCommand(IImportCategoryPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
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