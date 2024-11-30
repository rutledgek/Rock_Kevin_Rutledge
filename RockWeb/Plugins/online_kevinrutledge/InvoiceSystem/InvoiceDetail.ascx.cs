using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Linq;
using System.Data.Entity;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Security;

using online.kevinrutledge.InvoiceSystem.Model;
using Rock.Web.UI.Controls;
using dotless.Core.Parser;
using Rock.Attribute;
using PayPal.Payments.DataObjects;
using Invoice = online.kevinrutledge.InvoiceSystem.Model.Invoice;
using cache = online.kevinrutledge.InvoiceSystem.Cache;

namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Invoice Detail")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("Displays the details of an Invoice.")]
    [ContextAware(typeof(Invoice))]

    [CustomCheckboxListField("Invoice Types",
        Description = "The invoice types that will be included on the page. Leave blank to include all. Only active invoice types are shown.",
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [_online_kevinrutledge_InvoiceSystem_InvoiceType] where [IsActive] = 1",
        Key = AttributeKeys.InvoiceTypes,
        Order = 0)]

    public partial class InvoiceDetail : Rock.Web.UI.RockBlock
    {
        #region Attribute and Pageparameter Keys
        private static class AttributeKeys
        {
            public const string InvoiceTypes = "InvoiceTypes";


        }

        private static class PageParameter
        {
            public const string InvoiceTypeId = "InvoiceTypeId";
            public const string InvoiceId = "InvoiceId";
        }

        #endregion


        #region Properties and Fields

        // Dictionary to store the state of assignments (Guid as key)

        public Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)> InvoiceAssignmentState
        {
            get
            {
                // Use the correct key for InvoiceAssignmentState
                return (Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>)(ViewState["InvoiceAssignmentState"]
                       ?? new Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>());
            }
            set
            {
                ViewState["InvoiceAssignmentState"] = value;
            }
        }

        public Dictionary<Guid, (string Description, int Quantity, decimal UnitPrice, decimal? TaxRate, decimal? DiscountAmount, decimal? DiscountPercent)> InvoiceItemState
        {
            get
            {
                return (Dictionary<Guid, (string, int, decimal, decimal?, decimal?, decimal?)>)(ViewState["InvoiceItemState"] ?? new Dictionary<Guid, (string, int, decimal, decimal?, decimal?, decimal?)>());
            }
            set
            {
                ViewState["InvoiceItemState"] = value;
            }
        }






        private Invoice _invoice = null; // Cached invoice instance
        private InvoiceType _invoiceType = null; // Cached Invoice Type
        private List<Guid> _allowedInvoiceTypes;

        #endregion

        #region Initialization and ViewState Handling

        protected override object SaveViewState()
        {
            ViewState["InvoiceAssignmentState"] = InvoiceAssignmentState;
            ViewState["InvoiceItemState"] = InvoiceItemState; // Save both states
            return base.SaveViewState();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            // Restore both states from ViewState
            InvoiceAssignmentState = ViewState["InvoiceAssignmentState"] as Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>
                                     ?? new Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>();

            InvoiceItemState = ViewState["InvoiceItemState"] as Dictionary<Guid, (string Description, int Quantity, decimal UnitPrice, decimal? TaxRate, decimal? DiscountAmount, decimal? DiscountPercent)>
                               ?? new Dictionary<Guid, (string, int, decimal, decimal?, decimal?, decimal?)>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);



            // Configure the Assignments grid
            gAssignments.DataKeyNames = new[] { "Guid" };
            gAssignments.Actions.ShowAdd = true;
            gAssignments.Actions.AddClick += gAssignments_AddClick;
            gAssignments.GridRebind += gAssignments_GridRebind;

            // Configure Items Grid
            gInvoiceItems.DataKeyNames = new[] { "Guid" };
            gInvoiceItems.Actions.ShowAdd = true;
            gInvoiceItems.Actions.AddClick += gInvoiceItems_AddClick;
            gInvoiceItems.GridRebind += gInvoiceItems_GridRebind;


            // Register BlockUpdated event
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);

            gAssignments.Actions.ShowAdd = false;
            gInvoiceItems.Actions.ShowAdd = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                var rockContext = new RockContext();
                bool authorized = true;

                // Retrieve Invoice ID from Page Parameters
                int? invoiceId = PageParameter(PageParameter.InvoiceId).AsIntegerOrNull();
                int? invoiceTypeId = PageParameter(PageParameter.InvoiceTypeId).AsIntegerOrNull();

                if (invoiceTypeId.HasValue)
                { 
                var invoiceType = cache.InvoiceTypeCache.Get(invoiceTypeId.Value);
                }


                ShowDetail();
            }

            if (!string.IsNullOrEmpty(hfActiveDialog.Value))
            {
                ShowDialog();
            }
        }

        #endregion

        #region Page Methods

        /// <summary>
        /// Bind the reminder types dropdown list.
        /// </summary>
        /// <param name="invoiceTypes">The reminder types.</param>
        private void BindInvoiceTypes()
            {
            _allowedInvoiceTypes = GetAttributeValue(AttributeKeys.InvoiceTypes).SplitDelimitedValues().AsGuidList();

            var rockContext = new RockContext();
            var invoiceTypeOptions = new InvoiceTypeService( rockContext ).Queryable()
                .Where(t => t.IsActive) // Only active invoice types
                .ToList()
                .Where(t => t.IsAuthorized(Authorization.VIEW, CurrentPerson)) // Check VIEW authorization
                .Where(t => !_allowedInvoiceTypes.Any() || _allowedInvoiceTypes.Contains(t.Guid)) // Filter by allowed GUIDs or include all if empty
                .Select(t => new { Value = t.Guid, Text = t.Name }) // Project to Guid (Value) and Name (Text)
                .ToList();

            if (invoiceTypeOptions.Count == 1)
            {
                // Only one item exists, disable the dropdown and select the single item
                ddlInvoiceType.DataSource = invoiceTypeOptions;
                ddlInvoiceType.DataBind();

                ddlInvoiceType.SelectedValue = invoiceTypeOptions[0].Value.ToString();
                ddlInvoiceType.Enabled = false; // Disable the dropdown
            }
            else
            {
                // Multiple items exist, bind normally
                ddlInvoiceType.DataSource = invoiceTypeOptions;
                ddlInvoiceType.DataBind();
                ddlInvoiceType.Enabled = true; // Enable the dropdown
            }

        }


        protected void ShowDetail()
        {
            var rockContext = new RockContext();
            // Retrieve Invoice ID from Page Parameters
            int? invoiceId = PageParameter(PageParameter.InvoiceId).AsIntegerOrNull();
            int? invoiceTypeId = PageParameter(PageParameter.InvoiceTypeId).AsIntegerOrNull();

            // Fetch Invoice
            Invoice invoice = invoiceId.HasValue
                ? _invoice ?? new InvoiceService(rockContext).Get(invoiceId.Value)
                : null;

            // Fetch Invoice Type
            InvoiceType invoiceType = invoiceTypeId.HasValue ? _invoiceType ?? new InvoiceTypeService(rockContext).Get(invoiceTypeId.Value) : null;

            BindInvoiceTypes();

            var invoiceTypeCount = _allowedInvoiceTypes.Count();

            
            
           
            upnlContent.Visible = true;

            

            // Set Page Title and other UI elements
            if (invoice != null)
            {
                string pageTitle = $"Invoice #{invoice.Id}: {invoice.Name}";
                ltlInvoiceNumberAndName.Text = pageTitle;
                RockPage.PageTitle = pageTitle;
            }
            else
            {
                string newInvoiceTitle = "Create New Invoice";
                RockPage.PageTitle = newInvoiceTitle;
                ltlInvoiceNumberAndName.Text = newInvoiceTitle;
            }

            // Load Invoice Data into Controls
            hfInvoiceId.Value = invoice?.Id.ToString() ?? "0";
            tbName.Text = invoice?.Name;
            tbSummary.Text = invoice?.Summary;
            dpDueDate.SelectedDate = invoice?.DueDate;
            dpLateDate.SelectedDate = invoice?.LateDate;
            numbLateDays.Text = invoice?.LateDays.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            bool CanEdit = (invoiceType != null && invoiceType.IsAuthorized("ManageInvoices", CurrentPerson))
               || IsUserAuthorized(Authorization.EDIT);

            
            if (!CanEdit)
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed(Invoice.FriendlyTypeName);


            }

            if (readOnly)
            {
                ltlInvoiceNumberAndName.Text = ActionTitle.View(Invoice.FriendlyTypeName);

            }
            tbName.ReadOnly = readOnly;
            tbSummary.ReadOnly = readOnly;
            dpDueDate.ReadOnly = readOnly;
            dpLateDate.ReadOnly = readOnly;
            numbLateDays.ReadOnly = readOnly;
            gAssignments.Actions.ShowAdd = !readOnly;
            gInvoiceItems.Actions.ShowAdd = !readOnly;




            // Populate Assignment State
            InvoiceAssignmentState.Clear();
            if (invoice?.InvoiceAssignments != null)
            {
                foreach (var assignment in invoice.InvoiceAssignments)
                {
                    var assignmentKey = assignment.Guid != Guid.Empty ? assignment.Guid : Guid.NewGuid();
                    InvoiceAssignmentState[assignmentKey] = (
                        AuthorizedPersonAliasId: assignment.AuthorizedPersonAliasId,
                        AssignedPercent: assignment.AssignedPercent
                    );
                }
            }

            // Populate Items State
            InvoiceItemState.Clear();
            if ( invoice?.InvoiceItems != null)
            {
                foreach( var item in invoice.InvoiceItems)
                {
                    var itemKey = item.Guid != Guid.Empty ? item.Guid : Guid.NewGuid();
                    InvoiceItemState[itemKey] = (
                         //
                        item.Description,
                        Quantity: item.Quantity.ToIntSafe(),
                        UnitPrice: item.UnitPrice.ToIntSafe(),
                        item.TaxRate,
                        item.DiscountAmount,
                        item.DiscountPercent
                        );

                }

            }    


            // Bind Assignment Grid
            BindAssignmentGrid();
            BindInvoiceItemsGrid();
        }

        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            // Logic to handle updates to the block
        }

        #endregion

        #region Assignment Grid Methods and Events

        protected void gAssignments_AddClick(object sender, EventArgs e)
        {
            hfAssignmentGuid.Value = string.Empty; // Clear the GUID for new assignments
           
            ClearDialogFields();
            numbAssignedPercent.Text = CalculateRemainingAssignedPercent().ToString();
            ShowDialog(Dialogs.InvoiceAssignment);
        }

        protected void gAssignments_RowSelected(object sender, RowEventArgs e)
        {
            Guid assignmentKey = (Guid)e.RowKeyValue;

            if (InvoiceAssignmentState.ContainsKey(assignmentKey))
            {
                var selectedAssignment = InvoiceAssignmentState[assignmentKey];

                // Prefill dialog fields
                ppAssignment.SetValue(new PersonAliasService(new RockContext()).Get(selectedAssignment.AuthorizedPersonAliasId)?.Person);
                numbAssignedPercent.Text = selectedAssignment.AssignedPercent.ToString();

                // Store the GUID in a hidden field for editing
                hfAssignmentGuid.Value = assignmentKey.ToString();

                // Show dialog
                ShowDialog(Dialogs.InvoiceAssignment);
            }
        }

        protected void gAssignment_Delete(object sender, RowEventArgs e)
        {
            if (e.RowKeyValue is Guid assignmentKey && InvoiceAssignmentState.ContainsKey(assignmentKey))
            {
                InvoiceAssignmentState.Remove(assignmentKey);
                BindAssignmentGrid();
            }
        }

        protected void gAssignments_GridRebind(object sender, EventArgs e)
        {
            BindAssignmentGrid();
        }


        protected void btnSaveAssignment_Click(object sender, EventArgs e)
        {
            var personAliasId = ppAssignment.PersonAliasId;
            var percentAssigned = numbAssignedPercent.Text.AsDecimal();

            if (personAliasId.HasValue && percentAssigned > 0)
            {
                Guid assignmentKey;

                if (!string.IsNullOrWhiteSpace(hfAssignmentGuid.Value) && Guid.TryParse(hfAssignmentGuid.Value, out assignmentKey))
                {
                    // Edit existing entry
                    InvoiceAssignmentState[assignmentKey] = (personAliasId.Value, percentAssigned);
                }
                else
                {
                    // Add new entry
                    assignmentKey = Guid.NewGuid();
                    InvoiceAssignmentState[assignmentKey] = (personAliasId.Value, percentAssigned);
                }


            }

            HideDialog();

            BindAssignmentGrid();
        }

        protected void BindAssignmentGrid()
        {
            using (var rockContext = new RockContext())
            {
                var personAliasService = new PersonAliasService(rockContext);
                var authorizedPersonAliasIds = InvoiceAssignmentState.Values.Select(v => v.AuthorizedPersonAliasId).ToList();

                // Fetch Person Names for the Assignments
                var personAliases = personAliasService.Queryable()
                    .Where(pa => authorizedPersonAliasIds.Contains(pa.Id))
                    .ToDictionary(pa => pa.Id, pa => pa.Person.FullName);

                gAssignments.DataSource = InvoiceAssignmentState.Select(kvp => new
                {
                    Guid = kvp.Key,
                    PersonAliasName = personAliases.ContainsKey(kvp.Value.AuthorizedPersonAliasId)
                        ? personAliases[kvp.Value.AuthorizedPersonAliasId]
                        : "Unknown",
                    AssignedPercent = kvp.Value.AssignedPercent
                }).ToList();

                gAssignments.DataBind();

                hlblCurrentAssignedTotalGridView.Text = $"{CalculateRemainingAssignedPercent():0.##}% Not Assigned";
            }
        }







        #endregion


        #region InvoiceItems Grid Methods and Events

        protected void gInvoiceItems_AddClick(object sender, EventArgs e)
        {
            ClearDialogFields();
            hfInvoiceItemGuid.Value = string.Empty; // Clear the GUID for new Invoice Items
            numbQuantity.Text = "1";
            ShowDialog(Dialogs.InvoiceItem);
        }

        protected void gInvoiceItems_RowSelected(object sender, RowEventArgs e)
        {
            Guid invoiceItemKey = (Guid)e.RowKeyValue;

            if (InvoiceItemState.ContainsKey(invoiceItemKey))
            {
                var selectedItem = InvoiceItemState[invoiceItemKey];

                // Prefill dialog fields with the selected item's values
                tbItemDescription.Text = selectedItem.Description;
                numbUnitPrice.Text = selectedItem.UnitPrice.ToString("F2"); // Format as decimal with 2 places
                numbQuantity.Text = selectedItem.Quantity.ToString();
                numbTaxPercent.Text = selectedItem.TaxRate?.ToString("F2") ?? string.Empty; // Handle nullable decimal
                numbDiscountAmount.Text = selectedItem.DiscountAmount?.ToString("F2") ?? string.Empty; // Handle nullable decimal
                numbDiscountPercent.Text = selectedItem.DiscountPercent?.ToString("F2") ?? string.Empty; // Optional discount percentage

                // Store the GUID in a hidden field for editing
                hfInvoiceItemGuid.Value = invoiceItemKey.ToString();

             
            }
            // Show dialog
            ShowDialog(Dialogs.InvoiceItem);
        }

        protected void gInvoiceItem_Delete(object sender, RowEventArgs e)
        {
            if (e.RowKeyValue is Guid invoiceItemKey && InvoiceItemState.ContainsKey(invoiceItemKey))
            {
                InvoiceItemState.Remove(invoiceItemKey);
                BindInvoiceItemsGrid();
            }
        }

        protected void gInvoiceItems_GridRebind(object sender, EventArgs e)
        {
            BindInvoiceItemsGrid();
        }

        protected void btnSaveInvoiceItem_Click(object sender, EventArgs e)
        {
            // Extract input values
            var description = tbItemDescription.Text;
            var unitPrice = numbUnitPrice.Text.AsDecimal();
            var quantity = numbQuantity.Text.AsInteger();
            var discountAmount = numbDiscountAmount.Text.AsDecimalOrNull();
            var discountPercent = numbDiscountPercent.Text.AsDecimalOrNull();
            var guidValue = hfInvoiceItemGuid.Value;

            // Check form tax rate or fallback to global tax rate, then default to 0 if neither exists
            var taxRate = numbTaxPercent.Text.AsDecimalOrNull()
                         // ?? GlobalTaxRate
                          ?? 0m;

            // Use ViewState-backed InvoiceItemState
            var invoiceItemState = InvoiceItemState;

            if (!string.IsNullOrWhiteSpace(guidValue) && Guid.TryParse(guidValue, out var itemGuid))
            {
                // Update existing item
                if (invoiceItemState.ContainsKey(itemGuid))
                {
                    invoiceItemState[itemGuid] = (
                        Description: description,
                        Quantity: quantity,
                        UnitPrice: unitPrice,
                        TaxRate: taxRate,
                        DiscountAmount: discountAmount,
                        DiscountPercent: discountPercent
                    );
                }
            }
            else
            {
                // Add new item
                var newItemGuid = Guid.NewGuid();
                invoiceItemState[newItemGuid] = (
                    Description: description,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    TaxRate: taxRate,
                    DiscountAmount: discountAmount,
                    DiscountPercent: discountPercent
                );
                hfInvoiceItemGuid.Value = newItemGuid.ToString();
            }

            // Persist state
            InvoiceItemState = invoiceItemState;

            // Hide dialog and refresh UI
            HideDialog();
            BindInvoiceItemsGrid();
        }


        protected void BindInvoiceItemsGrid()
        {
            var dataSource = new List<dynamic>();

            decimal subtotal = 0;
            decimal discountTotal = 0;
            decimal preTaxTotal = 0;
            decimal taxTotal = 0;
            decimal finalTotal = 0;

            foreach (var kvp in InvoiceItemState)
            {
                var totalPrice = kvp.Value.Quantity * kvp.Value.UnitPrice;

                // Calculate discounts
                var discountAmountValue = kvp.Value.DiscountAmount ?? 0;
                var discountPercentValue = totalPrice * (kvp.Value.DiscountPercent ?? 0) / 100;
                var totalDiscount = Math.Max(discountAmountValue, discountPercentValue);
                var priceAfterDiscount = totalPrice - totalDiscount;

                // Calculate tax
                var taxAmount = priceAfterDiscount * (kvp.Value.TaxRate ?? 0) / 100;

                // Aggregate values
                subtotal += totalPrice;
                discountTotal += totalDiscount;
                preTaxTotal += priceAfterDiscount;
                taxTotal += taxAmount;
                finalTotal += priceAfterDiscount + taxAmount;

                // Add row data
                dataSource.Add(new
                {
                    Guid = kvp.Key,
                    Quantity = kvp.Value.Quantity,
                    UnitPrice = kvp.Value.UnitPrice,
                    Description = kvp.Value.Description,
                    DiscountPercent = kvp.Value.DiscountPercent,
                    DiscountAmount = kvp.Value.DiscountAmount,
                    TotalPrice = totalPrice,
                    TotalDiscount = totalDiscount,
                    TotalAfterDiscount = priceAfterDiscount,
                    TaxAmount = taxAmount,
                    TotalAfterTax = priceAfterDiscount + taxAmount
                });
            }

            // Set the literals
            litInvoiceItemCount.Text = InvoiceItemState.Count.ToString();
            litInvoiceSubtotal.Text = subtotal.ToString("C"); // Format as currency
            litDiscountTotal.Text = discountTotal.ToString("C");
            litInvoicePreTaxTotal.Text = preTaxTotal.ToString("C");
            litTaxTotal.Text = taxTotal.ToString("C");
            litInvoiceFinalTotal.Text = finalTotal.ToString("C");

            // Bind data to grid
            gInvoiceItems.DataSource = dataSource;
            gInvoiceItems.DataBind();
        }


        #endregion




        #region Global Modal Controls


        protected void ClearActiveDialog()
        {

            HideDialog();
            BindAssignmentGrid();
        }

        protected void HideDialog()
        {

            if (Enum.TryParse(hfActiveDialog.Value, out Dialogs dialog))
            {
                if (dialog == Dialogs.InvoiceAssignment)
                {
                    dlgAssignment.Hide();
                    ClearDialogFields();
                }
           
                if (dialog == Dialogs.InvoiceItem)
                {
                    dlgInvoiceItem.Hide();
                    
                }
            }

            hfActiveDialog.Value = string.Empty;
        }



        protected void ShowDialog()
        {
            if (Enum.TryParse(hfActiveDialog.Value, out Dialogs dialog))
            {
                ShowDialog(dialog);
            }
        }

        protected decimal CalculateRemainingAssignedPercent()
        {
            var remainingPercent = 100 - InvoiceAssignmentState.Sum(a => a.Value.AssignedPercent);
            return remainingPercent;
        }

        protected void ShowDialog(Dialogs dialog)
        {
            hfActiveDialog.Value = dialog.ToString();

            if (dialog == Dialogs.InvoiceAssignment)
            {
                var remainingPercent = CalculateRemainingAssignedPercent();
                hlblCurrentAssignedTotal.LabelType = remainingPercent == 0
                    ? Rock.Web.UI.Controls.LabelType.Success
                    : Rock.Web.UI.Controls.LabelType.Warning;
                hlblCurrentAssignedTotal.Text = $"{remainingPercent:0.##}% Not Assigned";
                dlgAssignment.Show();
            }
            if( dialog == Dialogs.InvoiceItem)
            {
                dlgInvoiceItem.Show();

            }
        }

        protected void ClearDialogFields()
        {
            // Clear fields in dlgAssignment
            ppAssignment.SetValue(null); // Clear PersonPicker
            hlblCurrentAssignedTotal.Text = string.Empty; // Clear HighlightLabel
            numbAssignedPercent.Text = string.Empty; // Clear NumberBox

            // Clear fields in dlgInvoiceItem
            tbItemDescription.Text = string.Empty; // Clear DataTextBox
            numbQuantity.Text = string.Empty; // Clear Quantity NumberBox
            numbUnitPrice.Text = string.Empty; // Clear Unit Price NumberBox
            numbTaxPercent.Text = string.Empty; // Clear Tax Percent NumberBox
            numbDiscountAmount.Text = string.Empty; // Clear Discount Amount NumberBox
            numbDiscountPercent.Text = string.Empty; // Clear Discount Percentage NumberBox

            // Clear Validation Summary Controls
            vsAssignment.Controls.Clear(); 
            vsInvoiceItem.Controls.Clear();
        }
        #endregion


        public override List<BreadCrumb> GetBreadCrumbs(Rock.Web.PageReference pageReference)
        {
            var breadCrumbs = new List<BreadCrumb>();

            string crumbName = ActionTitle.Add(Invoice.FriendlyTypeName);

            int? invoiceId = PageParameter(PageParameter.InvoiceId).AsIntegerOrNull();

            if (invoiceId.HasValue)
            {
                _invoice = new InvoiceService(new RockContext()).Get(invoiceId.Value);
                if (_invoice != null)
                {
                    crumbName = $"Invoice #{_invoice.Id}: {_invoice.Name}";
                }
            }

            breadCrumbs.Add(new BreadCrumb(crumbName, pageReference));

            return breadCrumbs;
        }




        #region Enums

        protected enum Dialogs
        {
            InvoiceAssignment
                , InvoiceItem
        }

        #endregion
    }
}
