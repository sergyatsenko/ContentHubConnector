using Plugin.Ryder.Commerce.CatalogSync.Entities;
using Plugin.Ryder.Commerce.CatalogSync.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Ryder.Commerce.CatalogSync.Pipelines
{
    [PipelineDisplayName("SyncProductToChannelAdvisorPipline")]
    public interface ISyncProductToChannelAdvisorPipline : IPipeline<SyncProductToChannelAdvisorArgument, SyncProductToChannelAdvisorArgument, CommercePipelineExecutionContext>
    {
        
    }
}