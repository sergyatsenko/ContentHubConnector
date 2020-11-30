using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Commands
{
    /// <summary>
    /// Export Commerce Entities into a collection
    /// </summary>
    public class ExportCommerceEntitiesCommand : CommerceCommand
    {
        /// <summary>
        /// Import Commerce Entities Pipeline
        /// </summary>
        private readonly IExportEntitiesPipeline _pipeline;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="pipeline">Import Composer Template Pipeline</param>
        /// <param name="serviceProvider">Service Provider</param>
        public ExportCommerceEntitiesCommand(IExportEntitiesPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="commerceContext">commerceContext</param>
        /// <returns>true if the process was successful</returns>
        public async Task<EntityCollectionModel> Process(CommerceContext commerceContext, ExportEntitiesArgument arg)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                return await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));
            }
        }
    }
}
