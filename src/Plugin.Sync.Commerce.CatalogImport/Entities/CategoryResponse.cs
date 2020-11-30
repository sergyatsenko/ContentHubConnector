using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Sync.Commerce.CatalogImport.Entities
{
    public class ImportCategoryResponse
    {
        public Category Category { get; set; }
        public int StatusCode { get; set; }
        public bool IsNew { get; internal set; }
        public string ErrorMessage { get; set; }
    }
}