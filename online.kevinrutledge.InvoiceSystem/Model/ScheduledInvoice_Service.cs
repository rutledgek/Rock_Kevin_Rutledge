
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Linq;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class ScheduledInvoiceService : Service<ScheduledInvoice>
    {

       

        public ScheduledInvoiceService(RockContext context) : base(context) { }

        public bool CanDelete(Invoice item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}