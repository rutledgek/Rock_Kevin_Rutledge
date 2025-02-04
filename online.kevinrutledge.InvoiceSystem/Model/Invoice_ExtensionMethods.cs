
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using online.kevinrutledge.InvoiceSystem.Model;

namespace online.kevinrutledge.InvoiceSystem.Model
{
    public static class InvoiceExtensionMethods
    {
        ///<summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="invoiceassignment">The Invoice Assignment.</param>
        /// <param name="rockContext">The rock context.</param>
        public static IQueryable<FinancialTransactionDetail> GetPayments(this Invoice invoice, RockContext rockContext = null)
        {
            rockContext = rockContext ?? new RockContext();
            return new InvoiceAssignmentService(rockContext).GetPayments(invoice != null ? invoice.Id : 0);

        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <param name="invoiceassignment">The Invoice Assignment.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static decimal GetTotalPaid(this Invoice invoice, RockContext rockContext = null)
        {
            rockContext = rockContext ?? new RockContext();
            return new InvoiceAssignmentService(rockContext).GetTotalPayments(invoice != null ? invoice.Id : 0);
        }
    }
}
