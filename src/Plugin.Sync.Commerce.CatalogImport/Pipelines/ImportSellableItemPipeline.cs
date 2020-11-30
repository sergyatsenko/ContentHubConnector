using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    public class ImportSellableItemPipeline :  CommercePipeline<ImportCatalogEntityArgument, ImportCatalogEntityArgument>, IImportSellableItemPipeline
    {
        public ImportSellableItemPipeline(IPipelineConfiguration<IImportSellableItemPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}