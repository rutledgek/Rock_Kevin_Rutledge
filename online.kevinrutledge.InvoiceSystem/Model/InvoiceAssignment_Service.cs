

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Linq;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public class InvoiceAssignmentService : Service<InvoiceAssignment>
    {
        /// <summary>
        /// Gets the payments for this association.
        /// </summary>
        /// <param name="context"></param>
        public IQueryable<FinancialTransactionDetail> GetPayments(int InvoiceAssignmentId)
        {
            int invoiceassignmentEntityTypeId = EntityTypeCache.Get(typeof(online.kevinrutledge.InvoiceSystem.Model.InvoiceAssignment)).Id;
            return new FinancialTransactionDetailService((RockContext)this.Context)
                .Queryable("Transaction")
                .Where(
                    t =>
                        t.EntityTypeId == invoiceassignmentEntityTypeId &&
                        t.EntityId == InvoiceAssignmentId);
        }

        public decimal GetTotalPayments(int InvoiceAssignmentId )
        {
            return GetPayments(InvoiceAssignmentId)
                .Select(p => p.Amount).DefaultIfEmpty()
                .Sum();
        }


        public InvoiceAssignmentService(RockContext context) : base(context) { }

        public bool CanDelete(InvoiceAssignment item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}