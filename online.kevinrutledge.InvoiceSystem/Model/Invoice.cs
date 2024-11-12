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

       // Create a collection of Invoices


        #region Virtual Properties
        public virtual Category Category { get; set; }
        #endregion
    
    }

    public partial class InvoicConfiguration : EntityTypeConfiguration<Invoice>
    {
        public InvoicConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on Invoice, 
            // no specific foreign key configuration is set here.


            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("Invoice");
        }
    }
}