

using Rock.Data;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class InvoiceItemService : Service<InvoiceItem>
    {
        public InvoiceItemService(RockContext context) : base(context) { }

        public bool CanDelete(InvoiceItem item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}