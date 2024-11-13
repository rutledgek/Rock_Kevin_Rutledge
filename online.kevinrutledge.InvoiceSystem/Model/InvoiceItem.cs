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
    [Table("_online_kevinrutledge_InvoiceSystem_InvoiceItem")]
    // That line goes right above the class definition...
    [DataContract]
    public class InvoiceItem : Model<InvoiceItem>, IRockEntity
    {
       

    }

    public partial class InvoiceItemConfiguration : EntityTypeConfiguration<InvoiceItem>
    {
        public InvoiceItemConfiguration()
        {
            // Specify relationships and cascade delete settings if any
            // Since this example has no direct foreign keys on Invoice, 
            // no specific foreign key configuration is set here.
            

            // Set entity set name for consistency and easy querying
            this.HasEntitySetName("InvoiceItem");
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