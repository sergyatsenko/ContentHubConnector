using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments
{
    public class ExportEntitiesArgument : PipelineArgument
    {
        public string EntityIDs { get; set; }
        public string EntityType { get; set; }
        public ExportEntitiesArgument()
        {
        }
    }
}
