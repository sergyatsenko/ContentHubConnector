﻿using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines
{
    [PipelineDisplayName("IImportCategoryFromContentHubPipeline")]
    public interface IImportCategoryFromContentHubPipeline : IPipeline<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
    }
}