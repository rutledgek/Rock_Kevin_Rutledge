using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Rock;
using Rock.Model;
using System.Collections.Generic;
using Rock.Lava;
using Rock.Web.UI.Controls;
using online.kevinrutledge.InvoiceSystem;
using System.Diagnostics;
using Rock.Jobs;


namespace online.kevinrutledge.InvoiceSystem.Model
{
    /// <summary>
    /// DefinedType Logic
    /// </summary>
    public partial class Invoice
    {

        /* Get Transactions */
        /// <summary>
        /// Gets the <see cref="Rock.Model.FinancialTransactionDetail">payments</see>.
        /// </summary>
        /// <value>
        /// The payments.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual IQueryable<FinancialTransactionDetail> Payments
        {
            get
            {
                return this.GetPayments();
            }
        }

        /* Calculate Payment Total */
        [NotMapped]
        [LavaVisible]
        public virtual decimal TotalPaid
        {
            get
            {
                return this.GetTotalPaid();
            }
        }



        /* Calculate the Invoice Total */

        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceItemTotal
        {
            get
            {
                if (InvoiceItems == null || !InvoiceItems.Any())
                {
                    return 0.0M;
                }


                decimal totalCost = 0.0M;

                foreach (var item in InvoiceItems)
                {
                    var totalPrice = item.Quantity * item.UnitPrice;
                    totalCost += (decimal)totalPrice;
                }

                return totalCost;
            }
        }


        /* Invoice Discount Total */

        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceDiscountTotal
        {
            get
            {
                if (InvoiceItems.Count() == 0 || !InvoiceItems.Any())
                {
                    return 0.0M;
                }


                decimal discountTotal = 0.0M;

                foreach (var item in InvoiceItems)
                {
                    var totalPrice = item.Quantity * item.UnitPrice;

                    // Calculate discounts
                    var discountAmountValue = item.DiscountAmount ?? 0;
                    var discountPercentValue = totalPrice * (item.DiscountPercent ?? 0) / 100;

                    // Ensure both values are decimals
                    var totalDiscount = Math.Max((decimal)(discountAmountValue * item.Quantity), (decimal)discountPercentValue);
                    discountTotal += totalDiscount;


                }

                return discountTotal;
            }
        }

        /* Invoice Tax Total */
        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceTaxTotal
        {
            get
            {
                if (InvoiceItems.Count() == 0 || !InvoiceItems.Any())
                {
                    return 0.0M;
                }

                decimal TotalTax = 0.0M;
                foreach (var item in InvoiceItems)
                {
                    var totalPrice = item.Quantity * item.UnitPrice;

                    // Calculate discounts
                    var discountAmountValue = item.DiscountAmount ?? 0;
                    var discountPercentValue = totalPrice * (item.DiscountPercent ?? 0) / 100;

                    // Ensure both values are decimals
                    var totalDiscount = Math.Max((decimal)(discountAmountValue * item.Quantity), (decimal)discountPercentValue);
                    var priceAfterDiscount = totalPrice - totalDiscount;

                    // Calculate tax
                    var taxAmount = priceAfterDiscount * (item.TaxRate ?? 0) / 100;

                    TotalTax += (decimal)taxAmount;
                }



                return TotalTax;
            }
        }


        /* Calculated Late Fee */
        [NotMapped]
        [LavaVisible]
        public virtual decimal CalculatedLateFee
        {
            get
            {
                // Use the database LateFee value if it exists
                if (this.LateFee.HasValue)
                {
                    return this.LateFee.Value;
                }

                // Fallback to calculated late fee
                decimal lateFeeAmount = this.InvoiceType?.DefaultLateFeeAmount ?? 0M;
                decimal lateFeePercent = ((this.InvoiceType?.DefaultLateFeePercent ?? 0M)/ 100) * (this.InvoiceItemTotal - this.InvoiceDiscountTotal);

                // Return the greater of the flat fee or percentage-based fee
                return Math.Max(lateFeeAmount, lateFeePercent);
            }
        }



        /* Total Before Late Fees */
        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceTotalBeforeLateFee
        {
            get
            {
                return InvoiceItemTotal - InvoiceDiscountTotal + InvoiceTaxTotal;
            }
        }


        /* Calculate Is Paid */
        [NotMapped]
        [LavaVisible]
        public virtual bool IsPaid
        {
            get
            {
                // Directly calculate OutstandingBalance instead of calling the property
                decimal outstandingBalance = InvoiceTotalBeforeLateFee - TotalPaid;
                return outstandingBalance <= 0;
            }
        }

        /* Calculate Is Late */
        [NotMapped]
        [LavaVisible]
        public virtual bool IsLate
        {
            get
            {
                if (DueDate.HasValue && DueDate < RockDateTime.Now && !IsPaid)
                {
                    return true;
                }
                return false;
            }
        }

        [NotMapped]
        [LavaVisible]
        public bool ShouldIncludeLateFee
        {
            get
            {
                // Check if the invoice status is Late or Paid Late
                if (InvoiceStatus == InvoiceStatus.Late || InvoiceStatus == InvoiceStatus.PaidLate || InvoiceStatus == InvoiceStatus.Sent && IsLate)
                {
                    return true;
                }

                return false;
            }
        }

       

        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceTotal
        {
            get
            {
                // Base total
                decimal invoiceTotal = InvoiceItemTotal - InvoiceDiscountTotal + InvoiceTaxTotal;

                // Include Late Fee only if conditions are met
                if (ShouldIncludeLateFee)
                {
                    invoiceTotal += CalculatedLateFee;
                }

                return invoiceTotal;
            }
        }

        
        [NotMapped]
        [LavaVisible]
        public virtual decimal OutstandingBalance
        {
            get
            {
                // Directly calculate to avoid circular calls
                return InvoiceItemTotal - InvoiceDiscountTotal + InvoiceTaxTotal + CalculatedLateFee - TotalPaid;
            }
        }

        [NotMapped]
        [LavaVisible]
        // Virtual property to expose the CSS color
        public string InvoiceStatusCssColor => InvoiceStatus.GetCssColor();

        [NotMapped]
        [LavaVisible]
        // Virtual property to expose the Rock LabelType
        public Rock.Web.UI.Controls.LabelType InvoiceStatusLabelType => InvoiceStatus.GetLabelType();

    }
}

