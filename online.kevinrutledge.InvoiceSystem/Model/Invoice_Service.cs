
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Linq;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class InvoiceService : Service<Invoice>
    {

        public IQueryable<FinancialTransactionDetail> GetPayments(Invoice invoice)
        {
            int invoiceassignmentEntityTypeId = EntityTypeCache.Get(typeof(online.kevinrutledge.InvoiceSystem.Model.InvoiceAssignment)).Id;

            return invoice.InvoiceAssignments
                   .AsQueryable()
                   .SelectMany(a => new FinancialTransactionDetailService((RockContext)this.Context)
                       .Queryable("Transaction")
                       .Where(t => t.EntityTypeId == invoiceassignmentEntityTypeId && t.EntityId == a.Id));
        }


        public decimal GetTotalPayments(Invoice invoice)
        {
            return GetPayments(invoice)
                .Select(p => p.Amount)
                .DefaultIfEmpty(0) // Provide a default value of 0 if there are no payments
                .Sum(); // Sum the amounts
        }

        public InvoiceService(RockContext context) : base(context) { }

        public bool CanDelete(Invoice item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}