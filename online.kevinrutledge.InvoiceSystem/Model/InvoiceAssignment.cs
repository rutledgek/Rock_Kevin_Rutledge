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
using Rock.Lava;
using Rock;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    [Table("_online_kevinrutledge_InvoiceSystem_InvoiceAssignment")]
    // That line goes right above the class definition...
    [DataContract]
    public partial class InvoiceAssignment : Model<InvoiceAssignment>, IRockEntity
    {
        #region Database Properties

        [DataMember]
        public int InvoiceId { get; set; }

        [DataMember]
        public int AuthorizedPersonAliasId { get; set; }

        [DataMember]
        public decimal AssignedPercent { get; set; }

        [DataMember]
        public DateTime? LastSentDate { get; set; }
        #endregion

        #region Virtual Properties

        public virtual Invoice Invoice { get; set; }

        public virtual PersonAlias AuthorizedPersonAlias { get; set; }

    }
        #endregion
        public partial class InvoiceAssignmentConfiguration : EntityTypeConfiguration<InvoiceAssignment>
    {
        public InvoiceAssignmentConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on Invoice, 
            // no specific foreign key configuration is set here.

            // this.HasOptional(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).WillCascadeOnDelete(false);
            this.HasRequired(p => p.Invoice).WithMany().HasForeignKey(p => p.InvoiceId).WillCascadeOnDelete(false);
            this.HasRequired(p => p.AuthorizedPersonAlias).WithMany().HasForeignKey(p => p.AuthorizedPersonAliasId).WillCascadeOnDelete(false);


            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("InvoiceAssignment");
        }
    }
}
