using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    public class ImportCategoryPipeline : CommercePipeline<ImportCatalogEntityArgument, ImportCatalogEntityArgument>, IImportCategoryPipeline
    {
        public ImportCategoryPipeline(IPipelineConfiguration<IImportCategoryPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}