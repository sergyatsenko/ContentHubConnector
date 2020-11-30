namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments
{
    /// <summary>
    /// Import Types
    /// </summary>
    public enum ImportType
    {
        /// <summary>
        /// MErge of input template with an existing one
        /// </summary>
        Merge,

        /// <summary>
        /// Overrides existing template with input one
        /// </summary>
        Override,

        /// <summary>
        /// Skips input template if it is already existing
        /// </summary>
        Skip
    }
}
