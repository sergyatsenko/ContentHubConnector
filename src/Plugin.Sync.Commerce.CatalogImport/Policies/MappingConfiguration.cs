using System;
using System.Collections.Generic;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class MappingConfiguration
    {
        public string EntityType { get; set; }
        public bool AllowSycToRoot { get; set; }
        public bool ClearMissingFieldValues { get; set; }
        public string EntityIdPath { get; set; }
        public string EntityNamePath { get; set; }
        public string ParentEntityIdPath { get; set; }
        public string ParentEntityNamePath { get; set; }
        //public string ListPricePath { get; set; }
        public string SourceName { get; set; }
        public string CatalogName { get; set; }
        public Dictionary<string, string> FieldPaths { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<string>> RelatedEntityPaths { get; set; } = new Dictionary<string, List<string>> (StringComparer.OrdinalIgnoreCase);
    }
}
