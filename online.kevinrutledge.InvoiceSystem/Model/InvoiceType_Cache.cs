using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using online.kevinrutledge.InvoiceSystem.Model;
using Rock.Data; // Ensure your project has access to the Rock.Data namespace
using Rock.Web.Cache; // Assuming ModelCache is part of Rock.Web.Cache

namespace online.kevinrutledge.InvoiceSystem.Cache
{
    /// <summary>
    /// A cache class for InvoiceType, containing the properties and logic to manage cached InvoiceType entities.
    /// </summary>
    [Serializable]
    [DataContract]
    public class InvoiceTypeCache : ModelCache<InvoiceTypeCache, InvoiceType>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the InvoiceType.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description of the InvoiceType.
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the default tax rate for the InvoiceType.
        /// </summary>
        [DataMember]
        public decimal? DefaultTaxRate { get; private set; }

        /// <summary>
        /// Gets or sets the category identifier for the InvoiceType.
        /// </summary>
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this InvoiceType is active.
        /// </summary>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets related invoice items associated with this InvoiceType.
        /// </summary>
        [DataMember]
        private List<int> _relatedItemIds;

        #endregion



        #region Public Methods

        /// <summary>
        /// Populates the cache object with values from the InvoiceType database entity.
        /// </summary>
        /// <param name="entity">The InvoiceType entity.</param>
        public override void SetFromEntity(IEntity entity)
        {
            base.SetFromEntity(entity);

            var invoiceType = entity as InvoiceType;
            if (invoiceType == null)
            {
                return;
            }

            Name = invoiceType.Name;
            Description = invoiceType.Description;
            DefaultTaxRate = invoiceType.DefaultTaxRate;
            CategoryId = invoiceType.CategoryId;
            IsActive = invoiceType.IsActive;

            // Reset related items for lazy loading
            _relatedItemIds = null;
        }

        /// <summary>
        /// Retrieves related invoice items.
        /// </summary>
        /// <returns>A list of related invoice item IDs.</returns>
        public List<int> GetRelatedItems()
        {
            if (_relatedItemIds == null)
            {
                using (var rockContext = new RockContext())
                {
                    _relatedItemIds = new InvoiceItemService(rockContext)
                        .Queryable()
                        .Where(i => i.Id == Id)
                        .Select(i => i.Id)
                        .ToList();
                }
            }
            return _relatedItemIds;
        }

        /// <summary>
        /// Returns a string representation of the InvoiceType.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
