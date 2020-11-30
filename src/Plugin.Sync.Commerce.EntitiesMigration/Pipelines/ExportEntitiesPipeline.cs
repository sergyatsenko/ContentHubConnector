using Microsoft.Extensions.Logging;
using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines
{
    /// <summary>
    /// Import Commerce Entities Pipeline
    /// </summary>
    public class ExportEntitiesPipeline : CommercePipeline<ExportEntitiesArgument, EntityCollectionModel>, IExportEntitiesPipeline
    {
        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="loggerFactory">loggerFactory</param>
        public ExportEntitiesPipeline(IPipelineConfiguration<IExportEntitiesPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}