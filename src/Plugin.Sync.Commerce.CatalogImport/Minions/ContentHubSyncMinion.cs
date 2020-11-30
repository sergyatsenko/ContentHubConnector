using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Plugin.Sync.Commerce.CatalogImport.Minions
{
    public class ContentHubSyncMinion : Minion
    {
        //protected IProductImportMinionPipeline MinionPipeline { get; set; }

        public override void Initialize(IServiceProvider serviceProvider, MinionPolicy policy, CommerceContext globalContext)
        {
            base.Initialize(serviceProvider, policy, globalContext);
           //var MinionPipeline = serviceProvider.GetService<IProductImportMinionPipeline>();

        }

        protected override async Task<MinionRunResultsModel> Execute()
        {
            Logger.LogInformation("ContentHubSyncMinion running...");

            //var commerceContext = new CommerceContext(this.Logger, this.MinionContext.TelemetryClient, null);
            //commerceContext.Environment = this.Environment;

            //CommercePipelineExecutionContextOptions executionContextOptions = new CommercePipelineExecutionContextOptions(commerceContext, null, null, null, null, null);

            //MinionRunResultsModel result = await this.MinionPipeline.Run(new ProductImportMinionArgument(100, 0), executionContextOptions);
            var _tcs = new TaskCompletionSource<bool>();
            await _tcs.Task;
            Logger.LogInformation("ContentHubSyncMinion Completed.");

            return new MinionRunResultsModel();
        }

    }
}