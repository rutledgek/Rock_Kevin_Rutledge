

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Linq;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class ScheduledInvoiceAssignmentService : Service<ScheduledInvoiceAssignment>
    {
       
        public ScheduledInvoiceAssignmentService(RockContext context) : base(context) { }

        public bool CanDelete(InvoiceAssignment item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}