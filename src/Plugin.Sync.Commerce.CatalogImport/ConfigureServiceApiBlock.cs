using Microsoft.AspNetCore.OData.Builder;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport
{
    /// <summary>
    /// Defines a block which configures the OData model
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName("CatalogSyncConfigureServiceApiBlock")]
    public class ConfigureServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            var importCategory = modelBuilder.Action("ImportCategory");
            importCategory.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importSellableItem = modelBuilder.Action("ImportSellableItem");
            importSellableItem.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importSellableItemFromContentHub = modelBuilder.Action("ImportSellableItemFromContentHub");
            importSellableItemFromContentHub.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importSellableItemsFromContentHub = modelBuilder.Action("ImportSellableItemsFromContentHub");
            importSellableItemsFromContentHub.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var importCategoryFromContentHub = modelBuilder.Action("ImportCategoryFromContentHub");
            importCategoryFromContentHub.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var processAzureQueue = modelBuilder.Action("ProcessAzureQueue");
            processAzureQueue.ReturnsFromEntitySet<CommerceCommand>("Commands");

            return Task.FromResult(modelBuilder);
        }
    }
}
