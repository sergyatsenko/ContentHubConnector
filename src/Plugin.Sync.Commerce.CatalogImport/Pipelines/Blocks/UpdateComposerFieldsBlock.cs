using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Update Composer Template fields on Commerce Entity
    /// </summary>
    [PipelineDisplayName("UpdateComposerFieldsBlock")]
    public class UpdateComposerFieldsBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private readonly ComposerCommander _composerCommander;
        private readonly CommerceCommander _commerceCommander;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public UpdateComposerFieldsBlock(CommerceCommander commerceCommander, ComposerCommander composerCommander)
        {
            _commerceCommander = commerceCommander;
            _composerCommander = composerCommander;
        }

        /// <summary>
        /// Execute block entry point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
           
                
            CommerceEntity entity = null;
            var entityDataModel = context.GetModel<CatalogEntityDataModel>();
            Condition.Requires(entityDataModel, "CatalogEntityDataModel is required to exist in order for CommercePipelineExecutionContext to run").IsNotNull();
            Condition.Requires(entityDataModel.EntityId, "EntityId is reguired in input JSON data").IsNotNullOrEmpty();
            Condition.Requires(entityDataModel.CommerceEntityId, "Commerce Entity ID cannot be identified based on input JSON data").IsNotNullOrEmpty();

            if (entityDataModel != null)
            {
                entity = await _commerceCommander.Command<FindEntityCommand>().Process(context.CommerceContext, typeof(CommerceEntity), entityDataModel.CommerceEntityId);
                if (entity == null)
                {
                    var errorMessage = $"Error: Commerce Entity with ID={entityDataModel.EntityId} not found, UpdateComposerFieldsBlock cannot be executed.";
                    Log.Error(errorMessage);
                    context.Abort(errorMessage, this);
                    return arg;
                }
                await ImportComposerViewsFields(entity, entityDataModel.EntityFields, context.CommerceContext);
            }
            else
            {
                var errorMessage = $"Error: SellableItemEntityData or CategoryEntityData model is required to be present in CommercePipelineExecutionContext for UpdateComposerFieldsBlock to run.";
                Log.Error(errorMessage);
                context.Abort(errorMessage, this);
                return arg;
            }
            
            
            return arg;
        }
        #endregion
        
        private async Task<bool> ImportComposerViewsFields(CommerceEntity commerceEntity, Dictionary<string, string> entityFields, CommerceContext context)
        {
            //Get root/master view of the target entity, composer views, if any, will be included in Child views of this master view
            var masterView = await _commerceCommander.Command<GetEntityViewCommand>().Process(
                context, 
                commerceEntity.Id,
                commerceEntity.EntityVersion,
                context.GetPolicy<KnownCatalogViewsPolicy>().Master,
                string.Empty,
                string.Empty);

            if (masterView == null)
            {
                Log.Error($"Master view not found on Commerce Entity, Entity ID={commerceEntity.Id}");
                throw new ApplicationException($"Master view not found on Commerce Entity, Entity ID={commerceEntity.Id}");
            }

            if (masterView.ChildViews == null || masterView.ChildViews.Count == 0)
            {
                Log.Error($"No composer-generated views found on Sellable Item entity, Entity ID={commerceEntity.Id}");
                throw new ApplicationException($"No composer-generated views found on Sellable Item entity, Entity ID={commerceEntity.Id}");
            }

            //Now iterate through child views and then their child fields, looking for matching names
            var isUpdated = false;
            foreach (EntityView view in masterView.ChildViews)
            {
                EntityView composerViewForEdit = null;
                foreach (var viewField in view.Properties)
                {
                    //Found matching field that need to be updated
                    if (entityFields.Keys.Contains(viewField.Name))
                    {
                        //Retrieve the composer view to update...
                        if (composerViewForEdit == null)
                        {
                            composerViewForEdit = Task.Run<EntityView>(async () => await commerceEntity.GetComposerView(view.ItemId, _commerceCommander, context)).Result;
                        }
                        //...and update the field value
                        if (composerViewForEdit != null)
                        {
                            var composerProperty = composerViewForEdit.GetProperty(viewField.Name);
                            if (composerViewForEdit != null)
                            {
                                composerProperty.ParseValueAndSetEntityView(entityFields[viewField.Name]);
                                isUpdated = true;
                            }
                        }
                    }
                }
            }

            if (isUpdated)
            {
                return await _composerCommander.PersistEntity(context, commerceEntity);
            }

            return false;
        }

    }
}