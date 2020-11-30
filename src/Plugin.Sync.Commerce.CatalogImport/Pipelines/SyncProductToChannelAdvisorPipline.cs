using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.Ryder.Commerce.CatalogSync.Entities;
using Plugin.Ryder.Commerce.CatalogSync.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Ryder.Commerce.CatalogSync.Pipelines
{
    public class SyncProductToChannelAdvisorPipline : CommercePipeline<SyncProductToChannelAdvisorArgument, SyncProductToChannelAdvisorArgument>, ISyncProductToChannelAdvisorPipline
    {
        
        public SyncProductToChannelAdvisorPipline(IPipelineConfiguration<ISyncProductToChannelAdvisorPipline> configuration, ILoggerFactory loggerFactory) : 
            base(configuration, loggerFactory)
        {
        }
    }
}
  