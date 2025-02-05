using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;




using Rock.Web.Cache;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Security;
using Rock.Transactions;
using Rock;
using Rock.Lava;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    [Table("_online_kevinrutledge_InvoiceSystem_ScheduledInvoiceItem")]
    // That line goes right above the class definition...
    [DataContract]
    public class ScheduledInvoiceItem : Model<ScheduledInvoiceItem>, IRockEntity
    {

        #region Entity Properties
        // InvoiceId is a foreign key to the Invoice table
        [Required]
        [DataMember(IsRequired = true)]
        public int ScheduledInvoiceId { get; set; }


        // Description of the item
        [DataMember]
        public string Description { get; set; }


        // Quantity of the item
        [DataMember]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public int? Quantity { get; set; }


        // Price of the item
        [DataMember]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be non-negative.")]
        public decimal? UnitPrice { get; set; }

        // Discount Percentage of the item
        [DataMember]
        [Range(0, 100, ErrorMessage = "Discount Percentage must be between 0 and 100.")]
        public decimal? DiscountPercent { get; set; }

        // Discount Amount of the item
        [DataMember]
        public decimal? DiscountAmount { get; set; }

        // Tax Percentage of the item
        [DataMember]
        [Range(0, 100, ErrorMessage = "Tax Rate must be between 0 and 100.")]
        public decimal? TaxRate { get; set; }

        // Entity Type of the item
        [DataMember]
        public int? EntityTypeId { get; set; }

        // Entity Id of the item
        [DataMember]
        public int? EntityId { get; set; }


        #endregion


        #region Virtual Properties

        // Invoice is the parent of InvoiceItem
        [LavaVisible]
        public virtual ScheduledInvoice ScheduledInvoice { get; set; }


        [LavaVisible]
        public virtual EntityType EntityType { get; set; }


        #endregion

    }




    public partial class ScheduledInvoiceItemConfiguration : EntityTypeConfiguration<ScheduledInvoiceItem>
    {
        public ScheduledInvoiceItemConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on Invoice, 
            // no specific foreign key configuration is set here.

            //Create Relationship to Invoice
            this.HasRequired(i => i.ScheduledInvoice).WithMany().HasForeignKey(i => i.ScheduledInvoiceId).WillCascadeOnDelete(true);
            this.HasOptional(i => i.EntityType).WithMany().HasForeignKey(i => i.EntityTypeId).WillCascadeOnDelete(false);


            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("ScheduledInvoiceItem");
        }
    }


}
