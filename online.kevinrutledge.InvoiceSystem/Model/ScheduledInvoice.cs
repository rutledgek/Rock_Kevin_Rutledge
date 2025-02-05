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
using System.ComponentModel;
using System.Reflection;
using Rock.Attribute;


namespace online.kevinrutledge.InvoiceSystem.Model
{
    [Table("_online_kevinrutledge_InvoiceSystem_ScheduledInvoice")]
    // That line goes right above the class definition...
    [DataContract]


    public partial class ScheduledInvoice : Model<ScheduledInvoice>, IRockEntity
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
        /// Gets or sets whether the Scheduled Invoice is Active.
        /// </summary>
        [Required]
        [DataMember(IsRequired = true )]
        public bool IsActive { get; set; } = true;

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
        /// Gets or sets a Schedule Id for the Scheduled Invoice.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int? ScheduleId {  get; set; }

        /// <summary>
        /// Gets or sets the number of days before the due date the invoice should be sent.
        /// </summary>
        [DataMember]
        public int SendInvoiceDaysBefore { get; set; }

        /// <summary>
        /// Gets or sets the number of days after the due date the invoice should be considered late.
        /// </summary>
        ///
        [DataMember]
        public int DaysUntilLate { get; set; }


        #region Virtual Properties
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InvoiceType"/> that this Group is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.InvoiceType"/> that this Group is a member of.
        /// </value>
        [DataMember]
        public virtual InvoiceType InvoiceType { get; set; }



        /// <summary>
        /// Gets or sets the collection of Invoice Assignments associated with this Invoice.
        /// </summary>
        [DataMember]
        public virtual ICollection<ScheduledInvoiceAssignment> ScheduledInvoiceAssignments { get; set; } = new Collection<ScheduledInvoiceAssignment>();

        [DataMember]
        public virtual ICollection<ScheduledInvoiceItem> ScheduledInvoiceItems { get; set; } = new Collection<ScheduledInvoiceItem>();

        [DataMember]
        public virtual Schedule Schedule { get; set; }

        #endregion

    }

    public partial class ScheduledInvoiceConfiguration : EntityTypeConfiguration<ScheduledInvoice>
    {
        public ScheduledInvoiceConfiguration()
        {
      
            this.HasRequired(p => p.InvoiceType).WithMany().HasForeignKey(p => p.InvoiceTypeId).WillCascadeOnDelete(false);

            // Configure the one-to-many relationship with InvoiceAssignment
            this.HasMany(p => p.ScheduledInvoiceAssignments)
                .WithRequired(p => p.ScheduledInvoice)
                .HasForeignKey(p => p.ScheduledInvoiceId)
                .WillCascadeOnDelete(false);


            // Congigure the One to Many Relationship with Invoice Items
            this.HasMany(p => p.ScheduledInvoiceItems)
                .WithRequired(p => p.ScheduledInvoice)
                .HasForeignKey(p => p.ScheduledInvoiceId)
                .WillCascadeOnDelete(true);

            // Configure the Many to One Relationship with Schedule
            this.HasRequired(s => s.Schedule)
                .WithMany()
                .HasForeignKey(s => s.ScheduleId)
                .WillCascadeOnDelete(false);

            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("ScheduledInvoice");
        }
    }


    
}