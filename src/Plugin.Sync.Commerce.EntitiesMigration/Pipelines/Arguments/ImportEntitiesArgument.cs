using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments
{
    /// <summary>
    /// ImportComposerTemplatesArgument
    /// </summary>
    public class ImportEntitiesArgument : PipelineArgument
    {
        /// <summary>s
        /// Import Type
        /// </summary>
        public EntityCollectionModel EntityModel { get; set; }

        /// <summary>
        /// c'tor
        /// </summary>s
        public ImportEntitiesArgument(EntityCollectionModel entities)
        {
            this.EntityModel = entities;
        }
    }
}