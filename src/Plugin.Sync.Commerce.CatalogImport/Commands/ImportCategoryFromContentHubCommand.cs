using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Commands
{
    /// <summary>
    /// Import mapped Content Hub entity into Category in SXC
    /// </summary>
    public class ImportCategoryFromContentHubCommand : CommerceCommand
    {
        private readonly IImportCategoryFromContentHubPipeline _pipeline;

        public ImportCategoryFromContentHubCommand(IImportCategoryFromContentHubPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
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