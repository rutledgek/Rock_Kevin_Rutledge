using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Security;
using Rock.Web.Cache;
using online.kevinrutledge.InvoiceSystem.Model;
using Rock;
using Rock.Model;

namespace online.kevinrutledge.InvoiceSystem.Model
{
   public partial class ScheduledInvoiceAssignment
    {
        #region Properties


        [NotMapped]
        [LavaVisible]
        public virtual decimal AssignedCost
        {
            get
            {
                // Ensure both Invoice and InvoiceTotal are valid
                if (ScheduledInvoice != null && ScheduledInvoice.ScheduledInvoiceTotal > 0 && AssignedPercent > 0)
                {
                    return ScheduledInvoice.ScheduledInvoiceTotal * (AssignedPercent / 100M);
                }

                // Default to 0 if values are invalid
                return 0.0M;
            }
        }



        #endregion
    }
}
