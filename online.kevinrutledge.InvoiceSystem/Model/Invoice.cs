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
    [Table("_online_kevinrutledge_InvoiceSystem_Invoice")]
    // That line goes right above the class definition...
    [DataContract]
    public class Invoice : Model<Invoice>, IRockEntity
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InvoiceType"/> that this Group is a member belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InvoiceType"/> that this group is a member of.
        /// </value>
        [Required]
        [HideFromReporting]
        [DataMember(IsRequired = true)]
        public int InvoiceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name/title of the invoice.
        /// </summary>
        [DataMember]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a summary or description for the invoice.
        /// </summary>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the status of the invoice.
        /// </summary>
        [DataMember]
        public InvoiceStatus InvoiceStatus { get; set; }

        /// <summary>
        /// Gets or sets the due date for the invoice.
        /// </summary>
        [DataMember]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days after the due date when this invoice is considered late.
        /// </summary>
        [DataMember]
        public int? LateDays { get; set; }

        /// <summary>
        /// Gets or sets the late date for this invoice, if applicable.
        /// </summary>
        [DataMember]
        public DateTime? LateDate { get; set; }


        /// <summary>
        /// Gets or sets the collection of invoice items associated with this invoice.
        /// </summary>
        /// [LavaInclude]
        /// public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();



        #region Virtual Properties
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InvoiceType"/> that this Group is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.InvoiceType"/> that this Group is a member of.
        /// </value>
        [DataMember]
        public virtual InvoiceType InvoiceType { get; set; }
        #endregion

    }

    public partial class InvoiceConfiguration : EntityTypeConfiguration<Invoice>
    {
        public InvoiceConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on Invoice, 
            // no specific foreign key configuration is set here.
            this.HasRequired(p => p.InvoiceType).WithMany().HasForeignKey(p => p.InvoiceTypeId).WillCascadeOnDelete(false);

            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("Invoice");
        }
    }


    #region Enumerations

    public enum InvoiceStatus
    {
        Draft = 0,
        Scheduled = 1,
        Sent = 2,
        Paid = 3,
        Late = 4,
        Canceled = 5
    }

    #endregion
}