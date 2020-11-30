using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Models
{
    public class EntityCollectionModel
    {
        public Type EntityType { get; set; }
        public IList<CommerceEntity> Entities { get; set; }
    }
}
