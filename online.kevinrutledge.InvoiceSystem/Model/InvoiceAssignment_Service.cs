

using Rock.Data;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class InvoiceAssignmentService : Service<InvoiceAssignment>
    {
        public InvoiceAssignmentService(RockContext context) : base(context) { }

        public bool CanDelete(InvoiceAssignment item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}