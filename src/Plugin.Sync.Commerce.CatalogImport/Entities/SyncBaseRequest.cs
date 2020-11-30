namespace Plugin.Sync.Commerce.CatalogImport.Entities
{
    public class SyncBaseRequest
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
        //TODO: move vehicle into SellableItem maybe?
        public string VehicleId { get; set; }
    }
}