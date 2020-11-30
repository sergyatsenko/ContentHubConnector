using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Helpers;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
//using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Data.Eventing.Remote;
using Sitecore.Diagnostics;
using Sitecore.Framework.Caching;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Retrieve Content Hub entity via its API
    /// TODO: refactor this to use CH nuget instead of direct HTTP REST calls
    /// </summary>
    [PipelineDisplayName("GetContentHubEntityBlock")]
    public class GetContentHubEntityBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        private ContentHubHelper _contentHubHelper;
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public GetContentHubEntityBlock(ICacheManager cacheManager)
        {
            _contentHubHelper = new ContentHubHelper(cacheManager);
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            try
            {
                Assert.IsNotNull(arg, "arg is required");
                Assert.IsNotNull(arg.MappingConfiguration, "MappingConfiguration is required");
                Assert.IsNotNullOrEmpty(arg.MappingConfiguration.SourceName, "SourceName must be set in MappingConfiguration");
                
                var contentHubPolicy = context.GetPolicy<ContentHubConnectionPolicy>();
                Assert.IsNotNull(contentHubPolicy, "ContentHubConnectionPolicy is required, check your configuration");
                var contentHubConnectionSettings = contentHubPolicy.Connections.FirstOrDefault(c => c.InstanceName.Equals(arg.MappingConfiguration.SourceName, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(contentHubConnectionSettings, $"contentHubConnectionSettings not found for arg.SourceInstanceName=={arg.MappingConfiguration.SourceName}");
                var entityObject = await _contentHubHelper.GetEntityById(arg.ContentHubEntityId, contentHubConnectionSettings).ConfigureAwait(false);
                if (entityObject != null)
                {
                    arg.Entity = entityObject;

                    if (arg.MappingConfiguration?.RelatedEntityPaths != null)
                    {
                        var addedPaths = new Dictionary<string, string>();
                        arg.RelatedEntities = new Dictionary<string, List<JObject>>();
                        foreach (var key in arg.MappingConfiguration.RelatedEntityPaths.Keys)
                        {
                            if (arg.MappingConfiguration.RelatedEntityPaths[key] != null && arg.MappingConfiguration.RelatedEntityPaths[key].Count > 0)
                            {
                                var pathValues = string.Join("|", arg.MappingConfiguration.RelatedEntityPaths[key]).ToLower();
                                if (!addedPaths.ContainsKey(pathValues))
                                {
                                    var relatedEntities = await GetRelatedEntitiesRecursively(entityObject, arg.MappingConfiguration.RelatedEntityPaths[key], contentHubConnectionSettings).ConfigureAwait(false);
                                    if (relatedEntities != null)
                                    {
                                        arg.RelatedEntities.Add(key, relatedEntities.Values.ToList());
                                        addedPaths.Add(pathValues, key);
                                    }
                                }
                                else
                                {
                                    arg.RelatedEntities.Add(key, arg.RelatedEntities[addedPaths[pathValues]]);
                                }
                            }
                        }
                    }
                }
                
                return arg;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error retrieving Content Hub Entity {arg.ContentHubEntityId}.";
                Log.Error(errorMessage, ex, this);
                context.Abort(errorMessage, ex);
                return arg;
            }
        }

        #endregion

        #region Private methods
        private async Task<Dictionary<string, JObject>> GetRelatedEntitiesRecursively(JObject startEntity, List<string> relatedEntityPaths, ContentHubConnectionSettings contentHubConnectionSettings)
        {
            try
            {
                if (relatedEntityPaths != null && relatedEntityPaths.Count > 0)
                {
                    var relatedEntity = startEntity;
                    var relatedObjects = new Dictionary<string, JObject>();
                    for (int i = 0; i < relatedEntityPaths.Count; i++)
                    {
                        var relatedEntityTokens = relatedEntity.SelectTokens(relatedEntityPaths[i]);

                        if (relatedEntityTokens != null && relatedEntityTokens.Count() > 0)
                        {
                            foreach (var relatedEntityToken in relatedEntityTokens)
                            {
                                var relatedEntityUrl = relatedEntityToken.Value<string>();
                                if (!string.IsNullOrEmpty(relatedEntityUrl) && !relatedObjects.ContainsKey(relatedEntityUrl))
                                {
                                    relatedEntity = await _contentHubHelper.GetEntityByUrl(relatedEntityUrl, contentHubConnectionSettings).ConfigureAwait(false);
                                    if (relatedEntity == null)
                                    {
                                        return null;
                                    }
                                    else if (i == relatedEntityPaths.Count - 1)
                                    {
                                        relatedObjects.Add(relatedEntityUrl, relatedEntity);
                                    }
                                    else
                                    {
                                        var remainingPaths = relatedEntityPaths.GetRange(i + 1, relatedEntityPaths.Count - i - 1);
                                        var recursiveResults = await GetRelatedEntitiesRecursively(relatedEntity, remainingPaths, contentHubConnectionSettings).ConfigureAwait(false);
                                        if (recursiveResults != null && recursiveResults.Values.Count > 0)
                                        {
                                            relatedObjects.AddRange(recursiveResults);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return relatedObjects.Count() > 0 ? relatedObjects : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Cannot parse entity at one of the path provided: {string.Join(", ", relatedEntityPaths)}",  ex, this);
                throw ex;
            }
        }
        #endregion
    }
}