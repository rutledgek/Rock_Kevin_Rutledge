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

namespace online.kevinrutledge.InvoiceSystem.Model
{
    [Table("_online_kevinrutledge_InvoiceSystem_InvoiceType")]
    // That line goes right above the class definition...
    [DataContract]
    public class InvoiceType : Model<InvoiceType>, IRockEntity
    {
        /// <summary>
        /// Gets or sets the name of the Invoice Type.
        /// </summary>
        [DataMember]
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the Invoice Type.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether this Invoice Type is active.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

                 
        /// <summary>
        /// Gets or sets the term used to describe invoices of this type.
        /// </summary>
        [DataMember]
        [MaxLength(100)]
        public string InvoiceTerm { get; set; }

        /// <summary>
        /// Gets or sets the term used to describe items on the invoice.
        /// </summary>
        [DataMember]
        [MaxLength(100)]
        public string InvoiceItemTerm { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for an icon representing this invoice type.
        /// </summary>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Default communication template for invoices of this type.
        /// </summary>
        [DataMember]
        public string DefaultCommunicationTemplate { get; set; }
               

        /// <summary>
        /// Communication template for late invoices.
        /// </summary>
        [DataMember]
        public string LateInvoiceCommunicationTemplate { get; set; }

        /// <summary>
        /// Default tax percentage for invoices of this type. This value is used for all invoices of this type unless changed on the invoice item.
        /// </summary>
        [DataMember]
        public decimal DefaultTaxPercent { get; set; }

        /// <summary>
        /// Specifies the number of days after the due date that an invoice is considered late. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
        public int? DefaultDaysUntilLate { get; set; }

        /// <summary>
        /// Default late fee amount for invoices of this type. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
            public decimal DefaultLateFeeAmount { get; set; }

        /// <summary>
        /// Default late fee percentage for invoices of this type. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
            public decimal DefaultLateFeePercentage { get; set; }


        [DataMember]
        public int? CategoryId { get; set; }

        [DataMember(IsRequired = false)]
        public int? DefaultFinancialAccountId { get; set; }

        #region Virtual Properties
        public virtual Category Category { get; set; }
        #endregion

        public virtual FinancialAccount FinancialAccount { get; set; }
    }

    public partial class InvoiceTypeConfiguration : EntityTypeConfiguration<InvoiceType>
    {
        public InvoiceTypeConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on InvoiceType, 
            // no specific foreign key configuration is set here.

            this.HasOptional(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.FinancialAccount).WithMany().HasForeignKey(p => p.DefaultFinancialAccountId).WillCascadeOnDelete(false);

            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("InvoiceType");
        }
    }
}