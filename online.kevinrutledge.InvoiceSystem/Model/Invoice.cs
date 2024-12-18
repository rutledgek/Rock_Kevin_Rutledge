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
    [Table("_online_kevinrutledge_InvoiceSystem_Invoice")]
    // That line goes right above the class definition...
    [DataContract]


    public partial class Invoice : Model<Invoice>, IRockEntity
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

        [DataMember]
        public virtual InvoiceStatus InvoiceStatus {get; set;}

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


        [DataMember]
        public DateTime? LastSentDate { get; set; }



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
        [CSSColor("#ffd866"), LabelType(Rock.Web.UI.Controls.LabelType.Default)]
        Draft = 0,

        [CSSColor("#084298"), LabelType(Rock.Web.UI.Controls.LabelType.Info)]
        Scheduled = 1,

        [CSSColor("#084298"), LabelType(Rock.Web.UI.Controls.LabelType.Info)]
        Sent = 2,

        [CSSColor("#0f5132"), LabelType(Rock.Web.UI.Controls.LabelType.Success)]
        Paid = 3,

        [CSSColor("#0f5132"), LabelType(Rock.Web.UI.Controls.LabelType.Success), Description("Paid Late")]
        PaidLate = 4,

        [CSSColor("#842029"), LabelType(Rock.Web.UI.Controls.LabelType.Danger)]
        Late = 5,

        [CSSColor("#084298"), LabelType(Rock.Web.UI.Controls.LabelType.Info)]
        Canceled = 6
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            // Get the enum field
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            // Check if the Description attribute is applied
            DescriptionAttribute attribute = System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute?.Description ?? value.ToString();
        }

        public static string GetCssColor(this Enum value)
        {
            if (value == null) return "#ffd866"; // Default color

            FieldInfo field = value.GetType().GetField(value.ToString());
            CSSColorAttribute attribute = field?.GetCustomAttribute<CSSColorAttribute>();
            return attribute?.Color ?? "#ffd866"; // Default fallback
        }

        public static Rock.Web.UI.Controls.LabelType GetLabelType(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            LabelTypeAttribute attribute = field?.GetCustomAttribute<LabelTypeAttribute>();
            return attribute?.LabelType ?? Rock.Web.UI.Controls.LabelType.Default; // Default fallback
        }
    }



    #endregion
}