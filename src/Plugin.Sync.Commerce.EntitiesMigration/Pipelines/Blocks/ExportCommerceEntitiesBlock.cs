using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Plugin.Sync.Commerce.EntitiesMigration.Services;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Blocks
{
    /// <summary>
    /// Import Composer Templates Block
    /// </summary>
    [PipelineDisplayName("ExportCommerceEntitiesBlock")]
    public class ExportCommerceEntitiesBlock : PipelineBlock<ExportEntitiesArgument, EntityCollectionModel, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Commerce Commander
        /// </summary>
        private readonly CommerceCommander _commerceCommander;

        /// <summary>
        /// Commerce Entity service
        /// </summary>
        private readonly ICommerceEntityService _entityService;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="commerceCommander">commerceCommander</param>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public ExportCommerceEntitiesBlock(
            CommerceCommander commerceCommander,
            ICommerceEntityService entityService)
        {
            _commerceCommander = commerceCommander;
            _entityService = entityService;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<EntityCollectionModel> Run(ExportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            switch (arg.EntityType.ToLower())
            {
                case "catalog":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<Catalog>(context.CommerceContext));
                case "category":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<Category>(context.CommerceContext));
                case "sellableitem":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<SellableItem>(context.CommerceContext));
                case "pricebook":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<PriceBook>(context.CommerceContext));
                case "pricecard":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<PriceCard>(context.CommerceContext));
                case "promotionbook":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<PromotionBook>(context.CommerceContext));
                case "promotion":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<Promotion>(context.CommerceContext));
                case "composertemplate":
                    return await Task.FromResult(_entityService.ExportCommerceEntities<ComposerTemplate>(context.CommerceContext));
                default:
                    return null;
            }
        }
    }
}