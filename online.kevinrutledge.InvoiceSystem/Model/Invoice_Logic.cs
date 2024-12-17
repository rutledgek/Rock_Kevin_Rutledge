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


namespace online.kevinrutledge.InvoiceSystem.Model
{
    /// <summary>
    /// DefinedType Logic
    /// </summary>
    public partial class Invoice
    {

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

        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceDiscountTotal
        {
            get
            {
                if (InvoiceItems == null || !InvoiceItems.Any())
                {
                    return 0.0M;
                }


                decimal runningTotal = 0.0M;

                foreach (var item in InvoiceItems)
                {
                    var totalPrice = item.Quantity * item.UnitPrice;

                    // Calculate discounts
                    var discountAmountValue = item.DiscountAmount ?? 0;
                    var discountPercentValue = totalPrice * (item.DiscountPercent ?? 0) / 100;

                    // Ensure both values are decimals
                    var totalDiscount = Math.Max((decimal)(discountAmountValue * item.Quantity), (decimal)discountPercentValue);
                    runningTotal += totalDiscount;


                }

                return runningTotal;
            }
        }

        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceTotalTax
        {
            get
            {
                if (InvoiceItems == null || !InvoiceItems.Any())
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

        [NotMapped]
        [LavaVisible]
        public virtual decimal InvoiceTotal
        {
            get
            {
                return InvoiceItemTotal - InvoiceDiscountTotal + InvoiceTotalTax;
            }
        }

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
        [NotMapped]
        [LavaVisible]
        public virtual decimal TotalPaid
        {
            get
            {
                return this.GetTotalPaid();
            }
        }
        [NotMapped]
        [LavaVisible]

        public virtual decimal OutstandingBalance
        {
            get
            {
                return InvoiceTotal - TotalPaid;
            }
        }

        [NotMapped]
        [LavaVisible]
        public virtual bool IsPaid
        {
            get
            {
                bool isPaid = false;

                if( OutstandingBalance <= 0 )
                {
                    isPaid = true;
                }

                return isPaid;
            }
        }

        [NotMapped]
        [LavaVisible]
        public virtual bool IsLate {

            get {

                bool isLate = false;

                if( OutstandingBalance > 0 && DueDate < RockDateTime.Now)
                {
                    isLate = true;

                }
                return isLate;
            }

            }


        /// <summary>
        /// Gets the text representation of the invoice status if the status is Draft, Scheduled, or Canceled.
        /// </summary>
        [NotMapped]
        [LavaVisible]
        public InvoiceStatus InvoiceStatus
        {
            get
            {
                if (!InvoiceStatusId.HasValue)
                {
                    // Default to Draft if InvoiceStatusId is null
                    return InvoiceStatus.Draft;
                }


                // Either the Invoice status is scheduled or the last sent date is in the future. Set it to scheduled
                if(InvoiceStatusId == (int)InvoiceStatus.Scheduled && (LastSentDate > RockDateTime.Now || LastSentDate == null))
                {
                    return InvoiceStatus.Scheduled;
                }

                //Check to see if the status is scheduled and the email was sent.
                if(InvoiceStatusId == (int)InvoiceStatus.Scheduled && LastSentDate < RockDateTime.Now)
                {
                        //If so and it hasn't been paid and it isn't late return sent.
                        if(!IsPaid && !IsLate)
                        {
                            return InvoiceStatus.Sent;
                        }

                        //If so and it hasn't been paid but it is late, return late.
                        if(!IsPaid && IsLate)
                        {
                            return InvoiceStatus.Late;
                        }          

                        //If so and it is paid, return paid.
                        if (IsPaid)
                        {
                            return InvoiceStatus.Paid;
                        }
                }

                // Example: Logic for Late status (commented out for now)
                /*
                if (InvoiceStatusId == (int)InvoiceStatus.Late)
                {
                    if (DueDate.HasValue && DateTime.Now > DueDate.Value && !IsPaid)
                    {
                        return InvoiceStatus.Late;
                    }
                    else
                    {
                        // If not late, fallback to Scheduled or another status
                        return InvoiceStatus.Scheduled;
                    }
                }
                */

                // Example: Logic for Paid status (commented out for now)
                /*
                if (InvoiceStatusId == (int)InvoiceStatus.Paid)
                {
                    if (IsPaid)
                    {
                        return InvoiceStatus.Paid;
                    }
                    else
                    {
                        // Fallback to Draft if not paid
                        return InvoiceStatus.Draft;
                    }
                }
                */

                // Default behavior for other statuses
                if (Enum.IsDefined(typeof(InvoiceStatus), InvoiceStatusId.Value))
                {
                    return (InvoiceStatus)InvoiceStatusId.Value;
                }

                // Fallback for unexpected values
                return InvoiceStatus.Draft;
            }
        }

        [NotMapped]
        public virtual LabelType InvoiceStatusLabelType
        {
            get
            {
                // Map InvoiceStatus to Rock.Web.UI.Controls.LabelType
                switch (InvoiceStatus)
                {
                    case InvoiceStatus.Draft:
                        return Rock.Web.UI.Controls.LabelType.Warning;
                    case InvoiceStatus.Scheduled:
                        return Rock.Web.UI.Controls.LabelType.Info;
                    case InvoiceStatus.Sent:
                        return Rock.Web.UI.Controls.LabelType.Info;
                    case InvoiceStatus.Paid:
                        return Rock.Web.UI.Controls.LabelType.Success;
                    case InvoiceStatus.PaidLate:
                        return Rock.Web.UI.Controls.LabelType.Success;
                    case InvoiceStatus.Late:
                        return Rock.Web.UI.Controls.LabelType.Danger;
                    case InvoiceStatus.Canceled:
                        return Rock.Web.UI.Controls.LabelType.Info;
                    default:
                        return Rock.Web.UI.Controls.LabelType.Default;
                }
            }
        }

        [NotMapped]
        public virtual string InvoiceStatusColor
        {
            get
            {
                // Map InvoiceStatus to Rock.Web.UI.Controls.LabelType
                switch (InvoiceStatus)
                {
                    case InvoiceStatus.Draft:
                        return "#ffd866";
                    case InvoiceStatus.Scheduled:
                        return "#084298";
                    case InvoiceStatus.Sent:
                        return "#084298";
                    case InvoiceStatus.Paid:
                        return "#0f5132";
                    case InvoiceStatus.PaidLate:
                        return "#0f5132";
                    case InvoiceStatus.Late:
                        return "#842029";
                    case InvoiceStatus.Canceled:
                        return "#084298";
                    default:
                        return "#bcbebf";
                }
            }
        }


    }
}
