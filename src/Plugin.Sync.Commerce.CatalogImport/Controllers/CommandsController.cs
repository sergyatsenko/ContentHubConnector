using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Commands;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Controllers
{
    /// <summary>
    /// SXC Controller to expose Content Hub import pipelines as REST API in SXC Engine
    /// </summary>
    public class CommandsController : CommerceController
    {
        private readonly GetEnvironmentCommand _getEnvironmentCommand;

        static string _connectionString;
        static string _subscriptionName;
        static string _topicName;
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
        /// Import mapped Content Hub entity into SellableItem in SXC.
        /// </summary>
        /// <param name="request">EntityId of Content Hub Entity to be imported into SXC SellableItem.</param>
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

        /// <summary>
        /// Import mapped Content Hub entity into Category in SXC.
        /// </summary>
        /// <param name="request">EntityId of Content Hub Entity to be imported into SXC Category.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Set default environment
        /// </summary>
        /// <returns></returns>
        private async Task InitializeEnvironment()
        {
            var commerceEnvironment = await _getEnvironmentCommand.Process(this.CurrentContext, ENV_NAME) ??
                                      this.CurrentContext.Environment;

            this.CurrentContext.Environment = commerceEnvironment;
            this.CurrentContext.PipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
        }
    }
}