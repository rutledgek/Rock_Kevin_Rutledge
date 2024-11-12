

using Rock.Data;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class InvoiceService : Service<Invoice>
    {
        public InvoiceService(RockContext context) : base(context) { }

        public bool CanDelete(Invoice item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}