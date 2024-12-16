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
        public int? InvoiceStatusId { get; set; }

        /// <summary>
        /// Gets the text representation of the invoice status if the status is Draft, Scheduled, or Canceled.
        /// </summary>
        public InvoiceStatus InvoiceStatus
        {
            get
            {
                if (!InvoiceStatusId.HasValue)
                {
                    // Default to Draft if InvoiceStatusId is null
                    return InvoiceStatus.Draft;
                }

                // Example: Logic for Sent status (commented out for now)
                /*
                if (InvoiceStatusId == (int)InvoiceStatus.Sent)
                {
                    if (SentDate.HasValue && SentDate.Value <= DateTime.Now)
                    {
                        return InvoiceStatus.Sent;
                    }
                    else
                    {
                        // Fallback to Draft if conditions for Sent are not met
                        return InvoiceStatus.Draft;
                    }
                }
                */

                // Example: Logic for Late status (commented out for now)
                /*
                if (InvoiceStatusId == (int)InvoiceStatus.Late)
                {
                    if (DueDate.HasValue && DateTime.Now > DueDate.Value && !IsPaid)
                    {
                        return InvoiceStatus.Late;
                    }
                    else
                    {
                        // If not late, fallback to Scheduled or another status
                        return InvoiceStatus.Scheduled;
                    }
                }
                */

                // Example: Logic for Paid status (commented out for now)
                /*
                if (InvoiceStatusId == (int)InvoiceStatus.Paid)
                {
                    if (IsPaid)
                    {
                        return InvoiceStatus.Paid;
                    }
                    else
                    {
                        // Fallback to Draft if not paid
                        return InvoiceStatus.Draft;
                    }
                }
                */

                // Default behavior for other statuses
                if (Enum.IsDefined(typeof(InvoiceStatus), InvoiceStatusId.Value))
                {
                    return (InvoiceStatus)InvoiceStatusId.Value;
                }

                // Fallback for unexpected values
                return InvoiceStatus.Draft;
            }
        }

        public virtual Rock.Web.UI.Controls.LabelType InvoiceStatusLabelType
        {
            get
            {
                // Map InvoiceStatus to Rock.Web.UI.Controls.LabelType
                switch (InvoiceStatus)
                {
                    case InvoiceStatus.Draft:
                        return Rock.Web.UI.Controls.LabelType.Warning;
                    case InvoiceStatus.Scheduled:
                        return Rock.Web.UI.Controls.LabelType.Info;
                    case InvoiceStatus.Sent:
                        return Rock.Web.UI.Controls.LabelType.Info;
                    case InvoiceStatus.Paid:
                        return Rock.Web.UI.Controls.LabelType.Success;
                    case InvoiceStatus.Late:
                        return Rock.Web.UI.Controls.LabelType.Danger;
                    case InvoiceStatus.Canceled:
                        return Rock.Web.UI.Controls.LabelType.Info;
                    default:
                        return Rock.Web.UI.Controls.LabelType.Default;
                }
            }
        }


        /// <summary>
        /// Gets or sets the due date for the invoice.
        /// </summary>
        [DataMember]
        public DateTime? DueDate { get; set; }

        ///// <summary>
        ///// Gets or sets the number of days after the due date when this invoice is considered late.
        ///// </summary>
        //[DataMember]
        //public int? LateDays { get; set; }

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



        /// <summary>
        /// Gets or sets the collection of Invoice Assignments associated with this Invoice.
        /// </summary>
        [DataMember]
        public virtual ICollection<InvoiceAssignment> InvoiceAssignments { get; set; } = new Collection<InvoiceAssignment>();

        [DataMember]
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new Collection<InvoiceItem>();

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
            // Configure the one-to-many relationship with InvoiceAssignment
            this.HasMany(p => p.InvoiceAssignments)
                .WithRequired(p => p.Invoice)
                .HasForeignKey(p => p.InvoiceId)
                .WillCascadeOnDelete(false);


            // Congigure the One to Many Relationship with Invoice Items
            this.HasMany(p => p.InvoiceItems)
                .WithRequired(p => p.Invoice)
                .HasForeignKey(p => p.InvoiceId)
                .WillCascadeOnDelete(true);

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

    public static class InvoiceStatusHelper
    {
        public static string GetStatusText(InvoiceStatus status)
        {
            switch (status)
            {
                case InvoiceStatus.Draft:
                    return "Draft";
                case InvoiceStatus.Scheduled:
                    return "Scheduled";
                case InvoiceStatus.Sent:
                    return "Sent";
                case InvoiceStatus.Paid:
                    return "Paid";
                case InvoiceStatus.Late:
                    return "Late";
                case InvoiceStatus.Canceled:
                    return "Canceled";
                default:
                    return "Draft";
            }
        }

        public static Rock.Web.UI.Controls.LabelType GetLabelType(InvoiceStatus status)
        {
            switch (status)
            {
                case InvoiceStatus.Draft:
                    return Rock.Web.UI.Controls.LabelType.Warning;
                case InvoiceStatus.Scheduled:
                    return Rock.Web.UI.Controls.LabelType.Info;
                case InvoiceStatus.Sent:
                    return Rock.Web.UI.Controls.LabelType.Info;
                case InvoiceStatus.Paid:
                    return Rock.Web.UI.Controls.LabelType.Success;
                case InvoiceStatus.Late:
                    return Rock.Web.UI.Controls.LabelType.Danger;
                case InvoiceStatus.Canceled:
                    return Rock.Web.UI.Controls.LabelType.Info;
                default:
                    return Rock.Web.UI.Controls.LabelType.Warning;
            }
        }
    }

    #endregion
}