using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines
{
    /// <summary>
    /// Import Commerce entities Pipeline
    /// </summary>
    public class ImportEntitiesPipeline : CommercePipeline<ImportEntitiesArgument, bool>, IImportEntitiesPipeline
    {
        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="loggerFactory">loggerFactory</param>
        public ImportEntitiesPipeline(IPipelineConfiguration<IImportEntitiesPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}