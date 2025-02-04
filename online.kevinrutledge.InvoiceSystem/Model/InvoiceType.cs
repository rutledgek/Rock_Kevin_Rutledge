using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    [Table("_online_kevinrutledge_InvoiceSystem_InvoiceType")]
    // That line goes right above the class definition...
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid(online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Type)]
    public partial class InvoiceType : Model<InvoiceType>, IRockEntity, ICacheable
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
        /// Gets or sets the Default Financial Account Id.
        /// </summary>
        [DataMember(IsRequired = false)]
        public int? DefaultFinancialAccountId { get; set; }

        /// <summary>
        /// Default tax percentage for invoices of this type. This value is used for all invoices of this type unless changed on the invoice item.
        /// </summary>
        [DataMember]
        public decimal DefaultTaxRate { get; set; }

        /// <summary>
        /// Specifies the number of days after the due date that an invoice is considered late. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
        public int? DefaultDaysUntilLate { get; set; }

        /// <summary>
        /// Default late fee amount for invoices of this type. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
        public decimal? DefaultLateFeeAmount { get; set; }

        /// <summary>
        /// Default late fee percentage for invoices of this type. This value is used for all invoices of this type unless changed on the invoice.
        /// </summary>
        [DataMember]
        public decimal? DefaultLateFeePercent { get; set; }

        /// <summary>
        /// Gets or sets the Invoice Type Category Id.
        /// </summary>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        ///Gets or sets the Invoice Fom Name
        /// </summary>
        [DataMember]
        public string InvoiceFromName { get; set; }




        [DataMember]
        public int? InvoiceFromPersonAliasId { get; set; }

        /// <summary>
        ///Gets or Sets the Invoice From Email Address.
        /// </summary>
        [DataMember]
        public string InvoiceFromEmail { get; set; }



        /// <summary>
        ///Gets or Sets the Invoice Subject.
        /// </summary>
        [DataMember]
        public string InvoiceSubject { get; set; }

        /// <summary>
        /// Default communication template for invoices of this type.
        /// </summary>
        [DataMember]
        public string InvoiceCommunicationTemplate { get; set; }


        /// <summary>
        ///Gets or sets the system communication to use for sending the invoices.
        /// </summary>
        public int? InvoiceSystemCommunicationId { get; set; }



        [DataMember]
        public int? LateNoticeFromPersonAliasId { get; set; }

        /// <summary>
        ///Gets or sets the Invoice Fom Name
        /// </summary>
        [DataMember]
        public string LateNoticeFromName { get; set; }

        /// <summary>
        ///Gets or Sets the Invoice From Email Address.
        /// </summary>
        [DataMember]
        public string LateNoticeFromEmail { get; set; }

        /// <summary>
        ///Gets or Sets the Invoice Subject.
        /// </summary>
        [DataMember]
        public string LateNoticeSubject { get; set; }

        /// <summary>
        /// Default communication template for invoices of this type.
        /// </summary>
        [DataMember]
        public string LateNoticeCommunicationTemplate { get; set; }


        /// <summary>
        ///Gets or sets the system communication to use for sending the invoices.
        /// </summary>
        public int? LateNoticeSystemCommunicationId { get; set; }

        [DataMember]
        public int? PaymentPageId { get; set; }

        private Dictionary<string, string> _supportedActions;

        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace("ManageInvoices", "The roles and/or users that have the ability to manage invoices of this type. ");
                return supportedActions;
            }
        }
  #region Virtual Properties
        public virtual Category Category { get; set; }


        public virtual FinancialAccount FinancialAccount { get; set; }

        public virtual SystemCommunication InvoiceSystemCommunication { get; set; }

        public virtual SystemCommunication LateNoticeSystemCommunication { get; set; }


        public virtual PersonAlias InvoiceFromPersonAlias { get; set; }

        public virtual PersonAlias LateNoticeFromPersonAlias { get; set; }

        public virtual Page PaymentPage {  get; set; }

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
            this.HasOptional(p => p.FinancialAccount).WithMany().HasForeignKey(p => p.DefaultFinancialAccountId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.InvoiceSystemCommunication).WithMany().HasForeignKey(p => p.InvoiceSystemCommunicationId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.LateNoticeSystemCommunication).WithMany().HasForeignKey(p => p.LateNoticeSystemCommunicationId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.PaymentPage).WithMany().HasForeignKey(p => p.PaymentPageId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.InvoiceFromPersonAlias).WithMany().HasForeignKey(p => p.InvoiceFromPersonAliasId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.LateNoticeFromPersonAlias).WithMany().HasForeignKey(p => p.LateNoticeFromPersonAliasId).WillCascadeOnDelete(false);


            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("InvoiceType");
        }
    }
}