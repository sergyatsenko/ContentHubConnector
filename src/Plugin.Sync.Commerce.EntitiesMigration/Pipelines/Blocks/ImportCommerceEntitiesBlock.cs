using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Plugin.Sync.Commerce.EntitiesMigration.Services;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Blocks
{
    [PipelineDisplayName("ImportCommerceEntitiesBlock")]
    public class ImportCommerceEntitiesBlock : PipelineBlock<ImportEntitiesArgument, bool, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Commerce Entity service
        /// </summary>
        private readonly ICommerceEntityService _commerceEntityService;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="commerceCommander">commerceCommander</param>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public ImportCommerceEntitiesBlock(ICommerceEntityService commerceEntityService)
        {
            _commerceEntityService = commerceEntityService;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<bool> Run(ImportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            await _commerceEntityService.ImportCommerceEntities(arg.EntityModel, context);

            return true;
        }

    }
}
