using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        static string _connectionString; // = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=ManageSendListenAccessKey;SharedAccessKey=XWWm4N78ZewXzLJHkA0C1wxwqaFBABQFrKth8/U+vNQ=";

        //static string _subscriptionName = "quadfectaproductsync";
        static string _subscriptionName;// = "quadfectaproductsync";
        //static string _topicName = "products_content";
        static string _topicName;// = "xccontenthubdemoqueue";
        //static string _productEntityDefinition = "M.PCM.Product";
        //static int _maxMessagesCount;// = 100;
        private const string ENV_NAME = "HabitatAuthoring";
        private IServiceProvider _serviceProvider;
        private CommerceEnvironment _globalEnvironment;

        /// <summary>
        /// Public constructor with DI
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="globalEnvironment"></param>
        /// <param name="getEnvironmentCommand"></param>
        public CommandsController(IServiceProvider serviceProvider, 
            CommerceEnvironment globalEnvironment, 
            GetEnvironmentCommand getEnvironmentCommand,
            IConfiguration configuration) : base(serviceProvider, globalEnvironment)
        {
            _getEnvironmentCommand = getEnvironmentCommand;
            _serviceProvider = serviceProvider;
            _globalEnvironment = globalEnvironment;

            _connectionString = configuration.GetConnectionString("AppSettings:ServiceBusConnectionString");
            _topicName = configuration.GetValue<string>("AppSettings:ServiceBusTopicName");
            _subscriptionName = configuration.GetValue<string>("AppSettings:ServiceBusSubscriptionName");
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