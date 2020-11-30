using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Data.Clones.ItemSourceUriProviders;
using Sitecore.Data.Comparers;
using Sitecore.Data.Query;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Extract Commerce Entity fields from input JSON using Entity's MappingPolicy to find matching fields in input JSON
    /// </summary>
    [PipelineDisplayName("ExtractCatalogEntityFieldsFromJsonDataBlock")]
    public class ExtractCatalogEntityFieldsFromJsonDataBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        public ExtractCatalogEntityFieldsFromJsonDataBlock()
        {
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var mappingConfiguration = arg.MappingConfiguration;

            var jsonData = arg.Entity as JObject;
            Condition.Requires(jsonData, "Commerce Entity JSON parameter is required").IsNotNull();
            context.AddModel(new JsonDataModel(jsonData));

            var entityDataModel = context.GetModel<CatalogEntityDataModel>();

            var rootEntityFields = mappingConfiguration.FieldPaths.Where(s => !arg.RelatedEntities.ContainsKey(s.Key)).ToDictionary(k => k.Key, v => v.Value);

            var entityData = new CatalogEntityDataModel
            {
                EntityId = jsonData.SelectValue<string>(mappingConfiguration.EntityIdPath),
                EntityName = jsonData.SelectValue<string>(mappingConfiguration.EntityNamePath),
                CatalogName = mappingConfiguration.CatalogName,
                EntityFields = jsonData.SelectMappedValues(rootEntityFields),
            };

            var refEntityFields = mappingConfiguration.FieldPaths.Where(s => arg.RelatedEntities.ContainsKey(s.Key)).ToDictionary(k => k.Key, v => v.Value);
            if (refEntityFields != null && refEntityFields.Count > 0 && arg.RelatedEntities != null && arg.RelatedEntities.Count > 0)
            {
                foreach (var key in arg.RelatedEntities.Keys)
                {
                    if (arg.RelatedEntities[key] != null && refEntityFields.ContainsKey(key))
                    {
                        var fieldValues = new List<string>();
                        foreach (var refEntity in arg.RelatedEntities[key])
                        {
                            if (refEntity != null)
                            {
                                var fieldValue = refEntity.SelectValue<string>(refEntityFields[key]);
                                if (fieldValue != null)
                                {
                                    fieldValues.Add(fieldValue);
                                }
                            }
                        }
                        if (fieldValues.Any())
                        {
                            entityData.EntityFields.Add(key, string.Join("|", fieldValues));
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(entityData.CatalogName))
            {
                entityData.CatalogName = mappingConfiguration.CatalogName;
            }

            entityData.ParentEntityIDs = new List<string>();
            entityData.ParentEntityNames = new List<string>();
            if (!string.IsNullOrEmpty(mappingConfiguration.ParentEntityIdPath))
            {
                if (arg.RelatedEntities.ContainsKey("ParentEntityIdPath") && arg.RelatedEntities["ParentEntityIdPath"] != null)
                {
                    foreach (var parentEntity in arg.RelatedEntities["ParentEntityIdPath"])
                    {
                        var parentEntityId = parentEntity.SelectValue<string>(mappingConfiguration.ParentEntityIdPath);
                        if (!string.IsNullOrEmpty(parentEntityId) && parentEntityId != entityData.EntityId)
                        {
                            entityData.ParentEntityIDs.Add(parentEntityId);
                        }
                    }
                }
                else
                {
                    var parentEntityId = arg.Entity.SelectValue<string>(mappingConfiguration.ParentEntityIdPath);
                    if (!string.IsNullOrEmpty(parentEntityId) && parentEntityId != entityData.EntityId)
                    {
                        entityData.ParentEntityIDs.Add(parentEntityId);
                    }
                }

                if (arg.RelatedEntities.ContainsKey("ParentEntityNamePath") && arg.RelatedEntities["ParentEntityNamePath"] != null)
                {
                    foreach (var parentEntity in arg.RelatedEntities["ParentEntityNamePath"])
                    {
                        var parentEntityName = parentEntity.SelectValue<string>(mappingConfiguration.ParentEntityNamePath);
                        if (!string.IsNullOrEmpty(parentEntityName) && parentEntityName != entityData.EntityName)
                        {
                            entityData.ParentEntityNames.Add(parentEntityName);
                        }
                    }
                }
                else
                {
                    var parentEntityName = arg.Entity.SelectValue<string>(mappingConfiguration.ParentEntityNamePath);
                    if (!string.IsNullOrEmpty(parentEntityName) && parentEntityName != entityData.EntityName)
                    {
                        entityData.ParentEntityNames.Add(parentEntityName);
                    }
                }
            }

            context.AddModel(entityData);
            await Task.CompletedTask;

            return arg;
        }
    }
}
