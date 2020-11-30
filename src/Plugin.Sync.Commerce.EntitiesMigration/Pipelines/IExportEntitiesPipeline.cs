using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines
{
    /// <summary>
    /// Export Commerce Entities Pipeline
    /// </summary>
    [PipelineDisplayName("ExportEntitiesPipeline")]
    public interface IExportEntitiesPipeline : IPipeline<ExportEntitiesArgument, EntityCollectionModel, CommercePipelineExecutionContext>
    {
    }
}
