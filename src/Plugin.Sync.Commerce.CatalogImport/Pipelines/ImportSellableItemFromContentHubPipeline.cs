using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    public class ImportSellableItemFromContentHubPipeline :  CommercePipeline<ImportCatalogEntityArgument, ImportCatalogEntityArgument>, IImportSellableItemFromContentHubPipeline
    {
        public ImportSellableItemFromContentHubPipeline(IPipelineConfiguration<IImportSellableItemFromContentHubPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}