using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    [PipelineDisplayName("AddSellableItemToUpdatedSellableItemsListBlock")]
    public class AddSellableItemToUpdatedSellableItemsListBlock : PipelineBlock<PersistEntityArgument, PersistEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IAddListEntitiesPipeline _addListEntitiesPipeline;
        public AddSellableItemToUpdatedSellableItemsListBlock(IAddListEntitiesPipeline addListEntitiesPipeline)
        {
            _addListEntitiesPipeline = addListEntitiesPipeline;
        }
        public override async Task<PersistEntityArgument> Run(PersistEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var policy = context.CommerceContext.GetPolicy<MappingPolicyBase>();
            ListEntitiesArgument listArgument = new ListEntitiesArgument(new string[1] { arg.Entity.Id }, policy.SyncedItemsList);
            ListEntitiesArgument addToListResult = await this._addListEntitiesPipeline.Run(listArgument, context);
            return arg;
        }
    }
}