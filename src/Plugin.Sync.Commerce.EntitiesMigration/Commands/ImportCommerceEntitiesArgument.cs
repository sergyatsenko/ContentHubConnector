using System;
using System.Threading.Tasks;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;

namespace Plugin.Sync.Commerce.EntitiesMigration.Commands
{
    /// <summary>
    /// Import Commerce Entities from a given collection into Commerce DB
    /// </summary>
    public class ImportCommerceEntitiesCommand : CommerceCommand
    {
        /// <summary>
        /// Import Commerce Entities Template Pipeline
        /// </summary>
        private readonly IImportEntitiesPipeline _pipeline;
        public int MyProperty { get; set; }

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="pipeline">Import Composer Template Pipeline</param>
        /// <param name="serviceProvider">Service Provider</param>
        public ImportCommerceEntitiesCommand(IImportEntitiesPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="commerceContext">commerceContext</param>
        /// <returns>true if the process was successful</returns>
        public async Task<CommerceCommand> Process(CommerceContext commerceContext, ImportEntitiesArgument args)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {                
                var result = await this._pipeline.Run(args, new CommercePipelineExecutionContextOptions(commerceContext));
                return this;
            }
        }
    }
}
