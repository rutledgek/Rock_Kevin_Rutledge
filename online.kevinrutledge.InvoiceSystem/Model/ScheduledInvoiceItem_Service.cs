

using Rock.Data;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class ScheduledInvoiceItemService : Service<ScheduledInvoiceItem>
    {
        public ScheduledInvoiceItemService(RockContext context) : base(context) { }

        public bool CanDelete(InvoiceItem item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}