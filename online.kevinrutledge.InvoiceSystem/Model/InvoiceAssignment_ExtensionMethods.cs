using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using online.kevinrutledge.InvoiceSystem;
using Rock.Data;
using Rock;
using Rock.Model;


namespace online.kevinrutledge.InvoiceSystem.Model
{
    public static partial class InvoiceAssignmentExtensionMethods
    {
        ///<summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="invoiceassignment">The Invoice Assignment.</param>
        /// <param name="rockContext">The rock context.</param>
        public static IQueryable<FinancialTransactionDetail> GetPayments(this InvoiceAssignment invoiceAssignment, RockContext rockContext = null)
        {
            rockContext = rockContext ?? new RockContext();
            return new InvoiceAssignmentService(rockContext).GetPayments(invoiceAssignment != null ? invoiceAssignment.Id : 0);

        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <param name="invoiceassignment">The Invoice Assignment.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static decimal GetTotalPaid(this InvoiceAssignment invoiceAssignment, RockContext rockContext = null)
        {
            rockContext = rockContext ?? new RockContext();
            return new InvoiceAssignmentService(rockContext).GetTotalPayments(invoiceAssignment != null ? invoiceAssignment.Id : 0);
        }
    }
}
