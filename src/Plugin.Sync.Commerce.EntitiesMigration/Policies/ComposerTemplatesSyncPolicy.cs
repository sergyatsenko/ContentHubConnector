namespace Plugin.Sync.Commerce.EntitiesMigration.Policies
{
    using Sitecore.Commerce.Core;

    /// <summary>
    /// Composer Templates Sync Policy
    /// </summary>
    public class ComposerTemplatesSyncPolicy : Policy
    {
        /// <summary>
        /// c'tor
        /// </summary>
        public ComposerTemplatesSyncPolicy()
        {
            PathToJson = "C:\\Code\\ComposerExport.json";
        }

        /// <summary>
        /// Determines the path to the models json
        /// </summary>
        public string PathToJson { get; set; }
    }
}
