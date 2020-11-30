using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Plugin.Sync.Commerce.EntitiesMigration.Commands;
using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData;

namespace Plugin.Sync.Commerce.EntitiesMigration.Controllers
{
    /// <summary>
    /// Commands Controller
    /// </summary>
    public class CommandsController : CommerceController
    {
        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="serviceProvider">serviceProvider</param>
        /// <param name="globalEnvironment">globalEnvironment</param>
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment)
            : base(serviceProvider, globalEnvironment)
        {
        }

        [HttpPost]
        [Route("ExportEntities()")]
        public async Task<IActionResult> ExportEntities([FromBody]ODataActionParameters request)
        {
            Condition.Requires<ODataActionParameters>(request, nameof(request)).IsNotNull<ODataActionParameters>();
            if (!request.ContainsKey("entityType") || request["entityType"] == null)
                return (IActionResult)new BadRequestObjectResult((object)request);
            string entityType = request["entityType"].ToString();

            try
            {
                InitializeEnvironment();

                var exportEntitiesArgument = new ExportEntitiesArgument
                {
                    EntityType = entityType
                };
                var command = this.Command<ExportCommerceEntitiesCommand>();
                var result = await command.Process(this.CurrentContext, exportEntitiesArgument);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                return new JsonResult(result, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }


        [HttpPost]
        [Route("ImportEntities()")]
        public async Task<IActionResult> ImportEntities([FromBody] ODataActionParameters request)
        {
            if (!this.ModelState.IsValid || request == null)
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            if (!request.ContainsKey("importFile") || request["importFile"] == null)
                return (IActionResult)new BadRequestObjectResult((object)request);

            IFormFile formFile = (IFormFile)request["importFile"];
            MemoryStream memoryStream = new MemoryStream();
            formFile.CopyTo((Stream)memoryStream);
            FormFile file = new FormFile((Stream)memoryStream, 0L, formFile.Length, formFile.Name, formFile.FileName);

            try
            {
                InitializeEnvironment();

                var sb = new StringBuilder();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        sb.AppendLine(reader.ReadLine());
                }
                var jsonString = sb.ToString();
                var entities = JsonConvert.DeserializeObject<EntityCollectionModel>(jsonString, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All} );
                var importEntitiesArgument = new ImportEntitiesArgument(entities);

                var command = this.Command<ImportCommerceEntitiesCommand>();
                var result = await command.Process(this.CurrentContext, importEntitiesArgument);

                return (IActionResult)new ObjectResult((object)this.ExecuteLongRunningCommand(() => command.Process(this.CurrentContext, importEntitiesArgument)));
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        private bool InitializeEnvironment()
        {
            var commerceEnvironment = this.CurrentContext.Environment;
            var pipelineContextOptions = this.CurrentContext.PipelineContextOptions;
            pipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
            this.CurrentContext.PipelineContextOptions.CommerceContext.Environment = commerceEnvironment;

            return true;
        }
    }
}