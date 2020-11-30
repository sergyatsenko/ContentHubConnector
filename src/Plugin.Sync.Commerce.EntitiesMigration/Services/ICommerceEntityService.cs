using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Services
{
    /// <summary>
    /// Composer Template Service
    /// </summary>
    public interface ICommerceEntityService
    {
        /// <summary>
        /// Gets all Composer Templates
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>List of all composer templates</returns>
        EntityCollectionModel ExportCommerceEntities<T>(CommerceContext context) where T: CommerceEntity;
        Task ImportCommerceEntities(EntityCollectionModel entityModel, CommercePipelineExecutionContext context);
    }
}
