

using Rock.Data;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class InvoiceTypeService : Service<InvoiceType>
    {
        public InvoiceTypeService(RockContext context) : base(context) { }

        public bool CanDelete(InvoiceType item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}