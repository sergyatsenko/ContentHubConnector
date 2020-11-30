using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
//using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Services.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Sitecore.Diagnostics;

namespace Plugin.Sync.Commerce.CatalogImport.Controllers
{
    /// <summary>
    /// Catalog Entities Import Controller
    /// </summary>
    public class CommandsController : CommerceController
    {
        private readonly GetEnvironmentCommand _getEnvironmentCommand;

        //TODO: move below consts into CH connection policy
        //static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=products_content;SharedAccessKey=yK0BD0C+ijNNWfl3cUJdmOJgC4MT/QSZZFqAAir7MZQ=";
        static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=ManageSendListenAccessKey;SharedAccessKey=XWWm4N78ZewXzLJHkA0C1wxwqaFBABQFrKth8/U+vNQ=";

        //static string _subscriptionName = "quadfectaproductsync";
        static string _subscriptionName = "quadfectaproductsync";
        //static string _topicName = "products_content";
        static string _topicName = "xccontenthubdemoqueue";
        static string _productEntityDefinition = "M.PCM.Product";
        static int _maxMessagesCount = 100;
        private const string ENV_NAME = "HabitatAuthoring";
        private IServiceProvider _serviceProvider;
        private CommerceEnvironment _globalEnvironment;

        /// <summary>
        /// Public constructor with DI
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="globalEnvironment"></param>
        /// <param name="getEnvironmentCommand"></param>
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment, GetEnvironmentCommand getEnvironmentCommand) : base(serviceProvider, globalEnvironment)
        {
            _getEnvironmentCommand = getEnvironmentCommand;
            _serviceProvider = serviceProvider;
            _globalEnvironment = globalEnvironment;

        }

        /// <summary>
        /// Import Category data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("ImportCategory()")]
        public async Task<IActionResult> ImportCategory([FromBody] JObject request)
        {
            await InitializeEnvironment().ConfigureAwait(false);
            try
            {
                var entityType = "M.PCM.ProductFamily";
                var command = Command<ImportCategoryCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<CategoryMappingPolicy>();
                var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
                if (mappingConfiguration != null)
                {
                    var argument = new ImportCatalogEntityArgument(request, mappingConfiguration, typeof(Category));
                    var result = await command.Process(CurrentContext, argument);
                    return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing Category data");
                }

                return new NotFoundObjectResult("No suitable mapping configuration found");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        /// <summary>
        /// Sync incoming data into Commerce SellableItem
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("ImportSellableItem()")]
        public async Task<IActionResult> ImportSellableItem([FromBody] JObject request)
        {
            await InitializeEnvironment().ConfigureAwait(false);
            try
            {
                var command = Command<ImportSellableItemCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();
                var entityType = "M.PCM.Product";

                var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
                if (mappingConfiguration != null)
                {
                    var argument = new ImportCatalogEntityArgument(request, mappingConfiguration, typeof(SellableItem));
                    var result = await command.Process(CurrentContext, argument);
                    return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing Category data");
                }

                return new NotFoundObjectResult("No suitable mapping configuration found");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        /// <summary>
        /// Import Content Hub entity into Commerce Sellable Item
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ImportSellableItemFromContentHub()")]
        public async Task<IActionResult> ImportSellableItemFromContentHub([FromBody] JObject request)
        {
            await InitializeEnvironment().ConfigureAwait(false);
            try
            {
                if (!request.ContainsKey("EntityId") || request["EntityId"] == null)
                    return (IActionResult)new BadRequestObjectResult((object)request);
                string entityId = request["EntityId"].ToString();
                string entityType = null;
                if (Request?.Headers != null && Request.Headers.ContainsKey("EntityType"))
                {
                    entityType = Request.Headers["EntityType"];
                }
                if (string.IsNullOrEmpty(entityType))
                {
                    throw new ArgumentNullException($"Error syncing Entity with EntityId={entityId}. entityType header value must be set.");
                }

                var command = Command<ImportSellableItemFromContentHubCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();

                var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
                if (mappingConfiguration != null)
                {
                    var argument = new ImportCatalogEntityArgument(mappingConfiguration, typeof(SellableItem))
                    {
                        ContentHubEntityId = entityId,
                        SourceEntityType = entityType
                    };
                    var result = await command.Process(CurrentContext, argument);

                    return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
                }

                return new NotFoundObjectResult("No suitable mapping configuration found");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        [HttpPost]
        [Route("ImportCategoryFromContentHub()")]
        public async Task<IActionResult> ImportCategoryFromContentHub([FromBody] JObject request)
        {
            await InitializeEnvironment().ConfigureAwait(false);
            try
            {
                if (!request.ContainsKey("EntityId") || request["EntityId"] == null)
                    return (IActionResult)new BadRequestObjectResult((object)request);
                string entityId = request["EntityId"].ToString();
                string entityType = null;
                if (Request?.Headers != null && Request.Headers.ContainsKey("EntityType"))
                {
                    entityType = Request.Headers["EntityType"];
                }
                if (string.IsNullOrEmpty(entityType))
                {
                    throw new ArgumentNullException($"Error syncing Entity with EntityId={entityId}. entityType header value must be set.");
                }
                string instanceName = null;
                if (Request?.Headers != null && Request.Headers.ContainsKey("InstanceName"))
                {
                    instanceName = Request.Headers["InstanceName"];
                }
                if (string.IsNullOrEmpty(instanceName))
                {
                    throw new ArgumentNullException($"Error syncing Entity with EntityId={entityId}. InstanceName header value must be set.");
                }

                var command = Command<ImportCategoryFromContentHubCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<CategoryMappingPolicy>();

                var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase)
                                                                                                && c.SourceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase));
                if (mappingConfiguration != null)
                {
                    var argument = new ImportCatalogEntityArgument(mappingConfiguration, typeof(SellableItem))
                    {
                        ContentHubEntityId = entityId,
                        SourceEntityType = entityType
                    };
                    var result = await command.Process(CurrentContext, argument);

                    return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
                }

                return new NotFoundObjectResult("No suitable mapping configuration found");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        //[HttpPost]
        //[Route("ImportCategoryFromContentHub()")]
        //public async Task<IActionResult> ImportCategoryFromContentHub([FromBody] JObject request)
        //{
        //    await InitializeEnvironment().ConfigureAwait(false);
        //    try
        //    {
        //        if (!request.ContainsKey("EntityId") || request["EntityId"] == null)
        //            return (IActionResult)new BadRequestObjectResult((object)request);
        //        string entityId = request["EntityId"].ToString();

        //        var command = Command<ImportCategoryFromContentHubCommand>();
        //        var mappingPolicy = CurrentContext.GetPolicy<CategoryMappingPolicy>();
        //        var argument = new ImportCatalogEntityArgument(mappingPolicy, typeof(SellableItem))
        //        {
        //            ContentHubEntityId = entityId
        //        };
        //        var result = await command.Process(CurrentContext, argument);

        //        return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ObjectResult(ex);
        //    }
        //}

        [HttpPost]
        [Route("ImportSellableItemsFromContentHub()")]
        public async Task<IActionResult> ImportSellableItemsFromContentHub([FromBody] JObject request)
        {
            await InitializeEnvironment().ConfigureAwait(false);

            try
            {
                if (!request.ContainsKey("EntityIds") || request["EntityIds"] == null)
                    return (IActionResult)new BadRequestObjectResult((object)request);
                string entityIds = request["EntityIds"].ToString();

                string entityType = null;
                if (Request?.Headers != null && Request.Headers.ContainsKey("EntityType"))
                {
                    entityType = Request.Headers["EntityType"];
                }
                if (string.IsNullOrEmpty(entityType))
                {
                    throw new ArgumentNullException($"Error syncing Entities with EntityId={entityIds}. EntityType header value must be set.");
                }

                string instanceName = null;
                if (Request?.Headers != null && Request.Headers.ContainsKey("InstanceName"))
                {
                    instanceName = Request.Headers["InstanceName"];
                }
                if (string.IsNullOrEmpty(instanceName))
                {
                    throw new ArgumentNullException($"Error syncing Entities with EntityId={entityIds}. InstanceName header value must be set.");
                }

                var command = Command<ImportSellableItemFromContentHubCommand>();
                var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();
                var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase)
                                                                                                && c.SourceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase));
                if (mappingConfiguration == null)
                {
                    return new NotFoundObjectResult("No suitable mapping configuration found");
                }

                var entityIdList = entityIds.Split(',');

                var results = new List<ImportCatalogEntityArgument>();
                foreach (var entityId in entityIdList)
                {
                    var argument = new ImportCatalogEntityArgument(mappingConfiguration, typeof(SellableItem))
                    {
                        ContentHubEntityId = entityId,
                        SourceEntityType = entityType,
                        
                    };

                    var result = await command.Process(CurrentContext, argument).ConfigureAwait(false);
                    results.Add(result);
                }

                return results != null ? new ObjectResult(results) : new NotFoundObjectResult("Error importing SellableItems data");
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        /// <summary>
        /// Process messages (import CH entities in CE) 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessAzureQueue()")]
        public async Task<IActionResult> ProcessAzureQueue([FromBody] JObject request)
        {
            await InitializeEnvironment().ConfigureAwait(false);
            try
            {
                var subClient = SubscriptionClient.CreateFromConnectionString(_connectionString, _topicName, _subscriptionName);
                //subClient.OnMessage(m =>
                //{
                //    Log.Information($"Processing Azure Queue message: {m.GetBody<string>()}");
                //});
                var messages = subClient.ReceiveBatch(_maxMessagesCount);
                if (messages != null && messages.Count() > 0)
                {
                    var command = Command<ImportSellableItemFromContentHubCommand>();
                    var mappingPolicy = CurrentContext.GetPolicy<SellableItemMappingPolicy>();
                    var entityType = "M.PCM.Product";

                    foreach (var message in messages)
                    {
                        //Check entity type and match it to policy settings
                        if (message != null && message.Properties.ContainsKey("target_id") && message.Properties.ContainsKey("target_definition"))
                        {
                            var targetId = (string)message.Properties["target_id"];
                            var targetDefinition = (string)message.Properties["SaveEntityMessage"];
                            var MessageType = (string)message.Properties["target_definition"];
                            var mappingConfiguration = mappingPolicy?.MappingConfigurations?.FirstOrDefault(c => c.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));

                            if (mappingConfiguration != null)
                            {
                                if (!string.IsNullOrEmpty(targetId) && !string.IsNullOrEmpty(targetDefinition) && targetDefinition.Equals(_productEntityDefinition, StringComparison.OrdinalIgnoreCase))
                                {
                                    var argument = new ImportCatalogEntityArgument(mappingConfiguration, typeof(SellableItem))
                                    {
                                        ContentHubEntityId = targetId,
                                        SourceEntityType = entityType
                                    };
                                    var result = await command.Process(CurrentContext, argument).ConfigureAwait(false);

                                    //TODO: complete on success, define failure(s) handling
                                    message.Complete();
                                } 
                            }
                        }
                    }
                }

                //TODO: return meaningful message(s)
                return new ObjectResult(true);
                //return result != null ? new ObjectResult(result) : new NotFoundObjectResult("Error importing SellableItem data");
            }
            catch (Exception ex)
            {
                Log.Error("Error processing Azure queue message(s)", ex, this);
                return new ObjectResult(ex);
            }
        }

        private CommerceContext GetCommerceContext()
        {
            var logger = (Microsoft.Extensions.Logging.ILogger)_serviceProvider.GetService<ILogger<CommerceController>>();
            var _nodeContext = _serviceProvider.GetService<NodeContext>();
            ITrackActivityPipeline service = _serviceProvider.GetService<ITrackActivityPipeline>();
            CommerceContext commerceContext = new CommerceContext(logger, new Microsoft.ApplicationInsights.TelemetryClient(), _serviceProvider.GetService<IGetLocalizableMessagePipeline>());
            commerceContext.GlobalEnvironment = _globalEnvironment;
            commerceContext.Environment = _globalEnvironment;
            commerceContext.ConnectionId = Guid.NewGuid().ToString("N", (IFormatProvider)CultureInfo.InvariantCulture);
            commerceContext.CorrelationId = Guid.NewGuid().ToString("N", (IFormatProvider)CultureInfo.InvariantCulture);
            commerceContext.TrackActivityPipeline = service;
            //NodeContext nodeContext = _nodeContext;
            commerceContext.PipelineTraceLoggingEnabled = _nodeContext != null && _nodeContext.PipelineTraceLoggingEnabled;

            commerceContext.Headers = new HeaderDictionary();
            commerceContext.Headers.Add("Roles", @"sitecore\Commerce Administrator|sitecore\Customer Service Representative Administrator|sitecore\Customer Service Representative|sitecore\Commerce Business User|sitecore\Pricer Manager|sitecore\Pricer|sitecore\Promotioner Manager|sitecore\Promotioner|sitecore\Merchandiser|sitecore\Relationship Administrator");

            return commerceContext;
            //this._baseContext = commerceContext;
        }

        /// <summary>
        /// Set default environment
        /// </summary>
        /// <returns></returns>
        private async Task InitializeEnvironment()
        {
            //var commerceEnvironment = this.CurrentContext.Environment;
            var commerceEnvironment = await _getEnvironmentCommand.Process(this.CurrentContext, ENV_NAME) ??
                                      this.CurrentContext.Environment;
            //await _getEnvironmentCommand.Process(this.CurrentContext, ENV_NAME) ??
            //this.CurrentContext. = "CommerceEngineDefaultStorefront";

            this.CurrentContext.Environment = commerceEnvironment;
            this.CurrentContext.PipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
        }
    }
}