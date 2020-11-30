using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;


namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines
{
    /// <summary>
    /// Import Commerce entities Pipeline
    /// </summary>
    [PipelineDisplayName("ImportEntitiesPipeline")]
    public interface IImportEntitiesPipeline : IPipeline<ImportEntitiesArgument, bool, CommercePipelineExecutionContext>
    {
    }
}
