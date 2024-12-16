using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;
using online.kevinrutledge.InvoiceSystem;
using online.kevinrutledge.InvoiceSystem.Cache;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    /// <summary>
    /// DefinedType Logic
    /// </summary>
    public partial class InvoiceType
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return InvoiceTypeCache.Get(this.Id);
        }

        #region ICacheable

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache(EntityState entityState, Rock.Data.DbContext dbContext)
        {
            // Update or remove the cache entry for the InvoiceType
            InvoiceTypeCache.UpdateCachedEntity(this.Id, entityState);
        }

        #endregion
    }

    #endregion
}

