namespace Plugin.Sync.Commerce.EntitiesMigration
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.OData.Builder;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("EntitiesMigrationConfigureServiceApiBlock")]
    public class ConfigureServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Add cutom APIs to CE
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            var importEntities = modelBuilder.Action("ImportEntities");
            importEntities.Parameter<string>("ImportType");
            importEntities.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var exportEntities = modelBuilder.Action("ExportEntities");
            //exportComposerTemplate.Parameter<string>("Ids");
            exportEntities.Parameter<string>("entityType");
            exportEntities.ReturnsFromEntitySet<CommerceCommand>("Commands"); 

            return Task.FromResult(modelBuilder);
        }
    }
}
