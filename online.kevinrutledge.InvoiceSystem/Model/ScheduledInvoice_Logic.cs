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
    public partial class ScheduledInvoice
    {



        /* Calculate the Invoice Total */

        [NotMapped]
        [LavaVisible]
        public virtual decimal ScheduledInvoiceItemTotal
        {
            get
            {
                if (ScheduledInvoiceItems == null || !ScheduledInvoiceItems.Any())
                {
                    return 0.0M;
                }


                decimal totalCost = 0.0M;

                foreach (var item in ScheduledInvoiceItems)
                {
                    var totalPrice = item.Quantity * item.UnitPrice;
                    totalCost += (decimal)totalPrice;
                }

                return totalCost;
            }
        }


        /* ScheduledInvoice Discount Total */

        [NotMapped]
        [LavaVisible]
        public virtual decimal ScheduledInvoiceDiscountTotal
        {
            get
            {
                if (ScheduledInvoiceItems.Count() == 0 || !ScheduledInvoiceItems.Any())
                {
                    return 0.0M;
                }


                decimal discountTotal = 0.0M;

                foreach (var item in ScheduledInvoiceItems)
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

        /* ScheduledInvoice Tax Total */
        [NotMapped]
        [LavaVisible]
        public virtual decimal ScheduledInvoiceTaxTotal
        {
            get
            {
                if (ScheduledInvoiceItems.Count() == 0 || !ScheduledInvoiceItems.Any())
                {
                    return 0.0M;
                }

                decimal TotalTax = 0.0M;
                foreach (var item in ScheduledInvoiceItems)
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


       

       

        [NotMapped]
        [LavaVisible]
        public virtual decimal ScheduledInvoiceTotal
        {
            get
            {
                // Base total
                decimal ScheduledInvoiceTotal = ScheduledInvoiceItemTotal - ScheduledInvoiceDiscountTotal + ScheduledInvoiceTaxTotal;

               return ScheduledInvoiceTotal;
            }
        }

    }
}

