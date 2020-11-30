using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCatalogEntityArgument : PipelineArgument
    {
        public string ContentHubEntityId { get; set; }
        public JObject Entity { get; set; }
        public Dictionary<string, List<JObject>> RelatedEntities { get; set; }
        public MappingConfiguration MappingConfiguration { get; set; }
        public Type CommerceEntityType { get; set; }
        public string SourceEntityType { get; set; }

        public ImportCatalogEntityArgument(JObject request, MappingConfiguration mappingConfiguration, Type commerceEntityType)
        {
            Entity = request;
            this.MappingConfiguration = mappingConfiguration;
            CommerceEntityType = commerceEntityType;
        }

        public ImportCatalogEntityArgument(MappingConfiguration mappingConfiguration, Type commerceEntityType)
        {
            this.MappingConfiguration = mappingConfiguration;
            CommerceEntityType = commerceEntityType;
        }

       
    }
}
