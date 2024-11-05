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
            /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that this Group is associated with.
            /// </summary>
            /// <value>
            /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that the Invoice Type is associated with. If the invoice type is not 
            /// associated with a campus, this value is null.
            /// </value>
            [HideFromReporting]
            [DataMember]
            [FieldType(Rock.SystemGuid.FieldType.CAMPUS)]
            public int? CampusId { get; set; }



            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Campus"/> that this Group is associated with.
            /// </summary>
            /// <value>
            /// The <see cref="Rock.Model.Campus"/> that this Group is associated with.
            /// </value>
            [DataMember]
            public virtual Rock.Model.Campus Campus { get; set; }



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
            /// Gets or sets the prefix for financial batches related to this invoice type.
            /// </summary>
            [DataMember]
            [MaxLength(100)]
            public string FinancialBatchPrefix { get; set; }

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
            /// Specifies the number of days after the due date that an invoice is considered late. This value is used for all invoices of this type unless changed on the invoice.
            /// </summary>
            [DataMember]
            public int? GlobalDaysUntilLate { get; set; }

        /// <summary>
        /// Default late fee amount for invoices of this type. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
            public decimal GlobalLateFeeAmount { get; set; }

        /// <summary>
        /// Default late fee percentage for invoices of this type. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
            public decimal GlobalLateFeePercentage { get; set; }


        [DataMember]
        public int? CategoryId { get; set; }


        #region Virtual Properties
        public virtual Category Category { get; set; }
        #endregion
    
}

    public partial class InvoiceTypeConfiguration : EntityTypeConfiguration<InvoiceType>
    {
        public InvoiceTypeConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on InvoiceType, 
            // no specific foreign key configuration is set here.

            this.HasOptional(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.Campus).WithMany().HasForeignKey(p => p.CampusId).WillCascadeOnDelete(false);

            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("InvoiceType");
        }
    }
}