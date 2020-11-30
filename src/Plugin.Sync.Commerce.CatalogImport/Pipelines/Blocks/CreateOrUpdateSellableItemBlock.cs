using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Rules.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{

    /// <summary>
    /// Import data into an existing or a new SellableItem entity
    /// </summary>
    [PipelineDisplayName("CreateOrUpdateSellableItemBlock")]
    public class CreateOrUpdateSellableItemBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly GetManagedListCommand _getManagedListCommand;
        private readonly DeleteRelationshipCommand _deleteRelationshipCommand;
        private readonly CommerceCommander _commerceCommander;
        private readonly CommerceEntityImportHelper _importHelper;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public CreateOrUpdateSellableItemBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander,
            GetManagedListCommand getManagedListCommand, DeleteRelationshipCommand deleteRelationshipCommand)
        {
            _commerceCommander = commerceCommander;
            _deleteRelationshipCommand = deleteRelationshipCommand;
            _getManagedListCommand = getManagedListCommand;
            _importHelper = new CommerceEntityImportHelper(commerceCommander, composerCommander);
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            //TODO: add an option to only import data if SellableItem already exists (don't create a new one)
            //TODO: add an option to only import data if SellableItem don't exist (don't update existing ones)

            var entityData = context.GetModel<CatalogEntityDataModel>();

            Condition.Requires(entityData, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityData.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.EntityName, "EntityName is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityData.CatalogName, "ParentCatalogName Name is reguired to be present in input JSON data or set default in SellabeItemMappingPolicy").IsNotNullOrEmpty();
            if (!((entityData.ParentEntityIDs != null && entityData.ParentEntityIDs.Count() > 0) || arg.MappingConfiguration.AllowSycToRoot))
            {
                var errorMessage = $"Cannot save SellableItem Entity withID == {entityData.EntityId} when Parent relationships not defined and AllowSycToRoot is set to false in mapping configuration.";
                Log.Error(errorMessage);
                context.Abort(errorMessage, this);
                //TODO: cleanup response
                return arg;
            }

            try
            {
                entityData.CommerceEntityId = $"{CommerceEntity.IdPrefix<SellableItem>()}{entityData.EntityId}";
                //Get or create sellable item
                var sellableItem = await GetOrCreateSellableItem(entityData, context);
                await DisassociateParentCategories(sellableItem, context.CommerceContext).ConfigureAwait(false);
                if (entityData.ParentEntityIDs != null && entityData.ParentEntityIDs.Count() > 0)
                {
                    foreach (var parentEntityId in entityData.ParentEntityIDs)
                    {
                        if (!string.IsNullOrEmpty(parentEntityId))
                        {
                            sellableItem = await AssociateSellableItemWithParent(entityData.CatalogName, parentEntityId, sellableItem, arg.MappingConfiguration.AllowSycToRoot, context.CommerceContext).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    sellableItem = await AssociateSellableItemWithParent(entityData.CatalogName, null, sellableItem, arg.MappingConfiguration.AllowSycToRoot, context.CommerceContext).ConfigureAwait(false);
                }

                //Check code running before this - this persist might be redindant
                //var persistResult = await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context);
                if (sellableItem == null)
                {
                    var errorMessage = $"Error persisting changes to SellableItem Entity withID == {entityData.EntityId}.";
                    Log.Error(errorMessage);
                    context.Abort(errorMessage, this);
                    //TODO: cleanup response
                    return arg;
                }

                return arg;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error creating or updating SellableItem Entity {entityData.EntityId}. {ex.Message}";
                Log.Error(ex, errorMessage);
                context.Abort(errorMessage, ex);
                return arg;
            }
        }
        #endregion

        #region Private methods
        public async Task DisassociateParentCategories(SellableItem sellableItem, CommerceContext context)
        {

            var parentCategorySitecoreIds = sellableItem?.ParentCategoryList?.Split('|');
            if (parentCategorySitecoreIds == null || parentCategorySitecoreIds.Length == 0)
            {
                return;
            }

            var categoryList = await _getManagedListCommand.Process(context, CommerceEntity.ListName<SellableItem>()).ConfigureAwait(false);
            var allCategories = categoryList?.Items?.Cast<Category>();

            if (allCategories != null)
            {
                foreach (var parentCategorySitecoreId in parentCategorySitecoreIds)
                {
                    var parentCategory = allCategories.FirstOrDefault(c => c.SitecoreId == parentCategorySitecoreId);
                    if (parentCategory != null)
                    {
                        await _deleteRelationshipCommand.Process(context, parentCategory.Id, sellableItem.Id, "CategoryToSellableItem");
                    }
                }
            }
        }
        /// <summary>
        /// Associate SellableItem with parent Catalog and Category(if exists)
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="parentCategoryName"></param>
        /// <param name="sellableItem"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<SellableItem> AssociateSellableItemWithParent(string catalogName, string parentCategoryName, SellableItem sellableItem, bool allowSyncToRoot, CommerceContext context)
        {
            string parentCategoryCommerceId = null;
            if (!string.IsNullOrEmpty(parentCategoryName))
            {
                var categoryCommerceId = $"{CommerceEntity.IdPrefix<Category>()}{catalogName}-{parentCategoryName}";
                var parentCategory = await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(Category), categoryCommerceId) as Category;
                parentCategoryCommerceId = parentCategory?.Id;
            }

            //TODO: Delete old relationships
            //var deassociateResult = await _commerceCommander.Command<DeleteRelationshipCommand>().Process(context, oldParentCategory.Id, sellableItem.Id, "CategoryToSellableItem");

            var catalogCommerceId = $"{CommerceEntity.IdPrefix<Catalog>()}{catalogName}";
            if (!string.IsNullOrEmpty(parentCategoryCommerceId))
            {
                await _commerceCommander.Command<AssociateSellableItemToParentCommand>().Process(context,
                catalogCommerceId,
                parentCategoryCommerceId,
                sellableItem.Id);

                return await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(SellableItem), sellableItem.Id) as SellableItem;
            }
            else if(allowSyncToRoot)
            {
                await _commerceCommander.Command<AssociateSellableItemToParentCommand>().Process(context,
                catalogCommerceId,
                catalogCommerceId,
                sellableItem.Id);

                return await _commerceCommander.Command<FindEntityCommand>().Process(context, typeof(SellableItem), sellableItem.Id) as SellableItem;
            }

            return sellableItem;
        }

        /// <summary>
        /// Find and return an existing Category or create a new one
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<SellableItem> GetOrCreateSellableItem(CatalogEntityDataModel entityData, CommercePipelineExecutionContext context)
        {
            SellableItem sellableItem = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), entityData.CommerceEntityId) as SellableItem;
            if (sellableItem == null)
            {
                sellableItem = await _commerceCommander.Command<CreateSellableItemCommand>().Process(context.CommerceContext,
                    entityData.EntityId,
                    entityData.EntityName,
                    entityData.EntityFields.ContainsKey("DisplayName") ? entityData.EntityFields["DisplayName"] : entityData.EntityName,
                    entityData.EntityFields.ContainsKey("Description") ? entityData.EntityFields["Description"] : string.Empty,
                    entityData.EntityFields.ContainsKey("Brand") ? entityData.EntityFields["Brand"] : string.Empty,
                    entityData.EntityFields.ContainsKey("Manufacturer") ? entityData.EntityFields["Manufacturer"] : string.Empty,
                    entityData.EntityFields.ContainsKey("TypeOfGoods") ? entityData.EntityFields["TypeOfGoods"] : string.Empty);
            }
            else
            {
                sellableItem.DisplayName = entityData.EntityFields.ContainsKey("DisplayName") ? entityData.EntityFields["DisplayName"] : entityData.EntityName;
                sellableItem.Description = entityData.EntityFields.ContainsKey("Description") ? entityData.EntityFields["Description"] : string.Empty;
                sellableItem.Brand = entityData.EntityFields.ContainsKey("Brand") ? entityData.EntityFields["Brand"] : string.Empty;
                sellableItem.Manufacturer = entityData.EntityFields.ContainsKey("Manufacturer") ? entityData.EntityFields["Manufacturer"] : string.Empty;
                sellableItem.TypeOfGood = entityData.EntityFields.ContainsKey("TypeOfGoods") ? entityData.EntityFields["TypeOfGoods"] : string.Empty;

                await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context).ConfigureAwait(false);
            }

            //if (entityData.ListPrice != null && entityData.ListPrice.HasValue && entityData.ListPrice > 0)
            //{
            //    var moneyPrice = new Money("USD", entityData.ListPrice.Value);
            //    sellableItem.ListPrice = moneyPrice;
            //    var pricingPolicy = sellableItem.GetPolicy<ListPricingPolicy>();
            //    pricingPolicy.AddPrice(new Money("USD", entityData.ListPrice.Value));
            //}
            //else
            //{
            //    sellableItem.ListPrice = null;
            //    var pricingPolicy = sellableItem.GetPolicy<ListPricingPolicy>();
            //    pricingPolicy.ClearPrices();
            //}

            await _commerceCommander.Pipeline<IPersistEntityPipeline>().Run(new PersistEntityArgument(sellableItem), context).ConfigureAwait(false);

            return await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(SellableItem), sellableItem.Id) as SellableItem;
        }

        #endregion
    }
}