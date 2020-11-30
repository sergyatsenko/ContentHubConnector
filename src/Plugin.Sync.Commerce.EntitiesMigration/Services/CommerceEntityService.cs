using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Services
{
    /// <summary>
    /// Composer Template Service
    /// </summary>
    public class CommerceEntityService : ICommerceEntityService
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        private readonly PersistEntityCommand _persistEntityCommand;
        private readonly DeleteEntityCommand _deleteEntityCommand;
        private readonly FindEntityCommand _findEntityCommand;
        private readonly AssociateCategoryToParentCommand _associateCategoryToParentCommand;
        private readonly AssociateSellableItemToParentCommand _associateSellableItemToParentCommand;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public CommerceEntityService(
            CommerceCommander commerceCommander,
            PersistEntityCommand persistEntityCommand,
            DeleteEntityCommand deleteEntityCommand,
            FindEntityCommand findEntityCommand,
            AssociateCategoryToParentCommand associateCategoryToParentCommand,
            AssociateSellableItemToParentCommand associateSellableItemToParentCommand,
            FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _commerceCommander = commerceCommander;
            _persistEntityCommand = persistEntityCommand;
            _deleteEntityCommand = deleteEntityCommand;
            _findEntityCommand = findEntityCommand;
            _associateCategoryToParentCommand = associateCategoryToParentCommand;
            _associateSellableItemToParentCommand = associateSellableItemToParentCommand;
        }

        /// <summary>
        /// Retrieve all entities of given type
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>List of all composer templates</returns>
        public EntityCollectionModel ExportCommerceEntities<T>(CommerceContext context) where T : CommerceEntity
        {
            CommerceList<T> commerceList = _findEntitiesInListCommand.Process<T>(context, CommerceEntity.ListName<T>(), 0, int.MaxValue).Result;
            List<CommerceEntity> entityList;
            if (commerceList == null)
            {
                entityList = null;
            }
            else
            {
                entityList = commerceList?.Items?.Cast<CommerceEntity>().ToList();
            }
            if (entityList == null)
            {
                entityList = new List<CommerceEntity>();
            }

            return new EntityCollectionModel
            {
                EntityType = typeof(T),
                Entities = entityList
            };
        }

        /// <summary>
        /// Save Commmerce Entities collection into Commerce DB
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ImportCommerceEntities(EntityCollectionModel entityModel, CommercePipelineExecutionContext context)
        {
            try
            {
                foreach (var commerceEntity in entityModel.Entities)
                {
                    var entity = Cast(commerceEntity, entityModel.EntityType);
                    var existingEntity = await _findEntityCommand.Process(context.CommerceContext, entity.GetType(), entity.Id);
                    if (existingEntity != null)
                    {
                        // Try to increase version count instead of delete
                        DeleteEntityArgument result = await _deleteEntityCommand.Process(context.CommerceContext, existingEntity.Id);
                        if (!result.Success)
                        {
                            Log.Error($"{this.GetType().Name}: Deletion of {existingEntity.Id} failed - new Entity was not imported");
                        }
                        else
                        {
                            entity.Version = 0;
                        }
                    }

                    //entity.EntityVersion = 1;
                    //entity.Version = 0;
                    entity.IsPersisted = false;
                    context.CommerceContext.AddUniqueEntityByType(entity);


                    var persistResult = await this._persistEntityCommand.Process(context.CommerceContext, entity);
                }

                if (entityModel.EntityType == typeof(Category))
                {
                    await AssociateCategoriesToParent(entityModel, context);
                }

                if (entityModel.EntityType == typeof(SellableItem))
                {
                    await AssociateSellableItemsToParent(entityModel, context);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Cast collection to given type
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="castTo"></param>
        /// <returns></returns>
        private dynamic Cast(dynamic obj, Type castTo)
        {
            return Convert.ChangeType(obj, castTo);
        }

        /// <summary>
        /// Asociate sellable items to parent category
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task AssociateSellableItemsToParent(EntityCollectionModel entityModel, CommercePipelineExecutionContext context)
        {
            CommerceList<Catalog> catalogs = _findEntitiesInListCommand.Process<Catalog>(context.CommerceContext, CommerceEntity.ListName<Catalog>(), 0, int.MaxValue).Result;
            CommerceList<Category> categories = _findEntitiesInListCommand.Process<Category>(context.CommerceContext, CommerceEntity.ListName<Category>(), 0, int.MaxValue).Result;
            foreach (var entity in entityModel.Entities)
            {
                var sellableItem = entity as SellableItem;
                if (!string.IsNullOrEmpty(sellableItem.ParentCatalogList) || !string.IsNullOrEmpty(sellableItem.ParentCategoryList))
                {
                    var parentCatalog = catalogs.Items.FirstOrDefault(i => i.SitecoreId.Equals(sellableItem.ParentCatalogList));
                    if (parentCatalog != null)
                    {
                        if (string.IsNullOrEmpty(sellableItem.ParentCategoryList))
                        {
                            await _associateSellableItemToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCatalog.Id, sellableItem.Id);
                        }
                        else
                        {
                            foreach (string categorySitecoreId in sellableItem.ParentCategoryList.Split('|'))
                            {
                                var parentCategory = categories.Items.FirstOrDefault(i => i.SitecoreId.Equals(categorySitecoreId));
                                if (parentCategory != null)
                                {
                                    await _associateSellableItemToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCategory.Id, sellableItem.Id);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Associate Commerce Catgories to given Catalog and Parent Categories
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task AssociateCategoriesToParent(EntityCollectionModel entityModel, CommercePipelineExecutionContext context)
        {
            CommerceList<Catalog> catalogs = _findEntitiesInListCommand.Process<Catalog>(context.CommerceContext, CommerceEntity.ListName<Catalog>(), 0, int.MaxValue).Result;
            CommerceList<Category> categories = _findEntitiesInListCommand.Process<Category>(context.CommerceContext, CommerceEntity.ListName<Category>(), 0, int.MaxValue).Result;
            foreach (var entity in entityModel.Entities)
            {
                var category = entity as Category;
                if (!string.IsNullOrEmpty(category.ParentCatalogList) || !string.IsNullOrEmpty(category.ParentCategoryList))
                {
                    var parentCatalog = catalogs.Items.FirstOrDefault(i => i.SitecoreId.Equals(category.ParentCatalogList));
                    if (parentCatalog != null)
                    {
                        if (string.IsNullOrEmpty(category.ParentCategoryList))
                        {
                            await _associateCategoryToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCatalog.Id, category.Id);
                        }
                        else
                        {
                            foreach (string categorySitecoreId in category.ParentCategoryList.Split('|'))
                            {
                                var parentCategory = categories.Items.FirstOrDefault(i => i.SitecoreId.Equals(categorySitecoreId));
                                if (parentCategory != null)
                                {
                                    await _associateCategoryToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCategory.Id, category.Id);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
