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
}