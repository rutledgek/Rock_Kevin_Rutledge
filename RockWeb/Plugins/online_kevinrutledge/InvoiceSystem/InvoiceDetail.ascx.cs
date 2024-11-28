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

using online.kevinrutledge.InvoiceSystem.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Invoice Detail")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("Displays the details of an Invoice.")]
    [ContextAware(typeof(Invoice))]
    public partial class InvoiceDetail : Rock.Web.UI.RockBlock
    {
        #region Properties and Fields

        // Dictionary to store the state of assignments (Guid as key)
        public Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)> InvoiceAssignmentState { get; set; }
            = new Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>();

        public Dictionary<Guid, (string Description, int Quantity, decimal UnitPrice)> InvoiceItemState { get; set; }
            = new Dictionary<Guid, (string Description, int Quantity, decimal UnitPrice)>();





        private Invoice _invoice = null; // Cached invoice instance

        #endregion

        #region Initialization and ViewState Handling

        protected override object SaveViewState()
        {
            ViewState["AssignmentState"] = InvoiceAssignmentState;
            return base.SaveViewState();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            InvoiceAssignmentState = ViewState["AssignmentState"] as Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>
                ?? new Dictionary<Guid, (int AuthorizedPersonAliasId, decimal AssignedPercent)>();
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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                ShowDetail();
            }

            if (!string.IsNullOrEmpty(hfActiveDialog.Value))
            {
                ShowDialog();
            }
        }

        #endregion

        #region Page Methods

        protected void ShowDetail()
        {
            var rockContext = new RockContext();
            upnlContent.Visible = true;

            // Retrieve Invoice ID from Page Parameters
            int? invoiceId = PageParameter("InvoiceId").AsIntegerOrNull();

            // Fetch Invoice
            Invoice invoice = invoiceId.HasValue
                ? _invoice ?? new InvoiceService(rockContext).Get(invoiceId.Value)
                : null;

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
                        Description: item.Description,
                        Quantity: item.Quantity.ToIntSafe(),
                        UnitPrice: item.UnitPrice.ToIntSafe()
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
            ClearAssignmentDialogFields();
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
            }
        }




        protected void ClearAssignmentDialogFields()
        {
            hfActiveDialog.Value = string.Empty;
            ppAssignment.SetValue(null);
            numbAssignedPercent.Text = string.Empty;
        }


        #endregion


        #region InvoiceItems Grid Methods and Events

        protected void gInvoiceItems_AddClick(object sender, EventArgs e)
        {
            hfInvoiceItemGuid.Value = string.Empty; // Clear the GUID for new Invoice Items
            ClearInvoiceItemDialogFields();
            ShowDialog(Dialogs.InvoiceItem);
        }

        protected void gInvoiceItems_RowSelected(object sender, RowEventArgs e)
        {
            Guid invoiceItemKey = (Guid)e.RowKeyValue;

            if (InvoiceItemState.ContainsKey(invoiceItemKey))
            {
                var selectedItem = InvoiceItemState[invoiceItemKey];

                // Prefill dialog fields
                //tbItemName.Text = selectedItem.ItemName;
                //nbQuantity.Text = selectedItem.Quantity.ToString();
                //nbUnitPrice.Text = selectedItem.UnitPrice.ToString("F2");

                // Store the GUID in a hidden field for editing
                hfInvoiceItemGuid.Value = invoiceItemKey.ToString();

                // Show dialog
                ShowDialog(Dialogs.InvoiceItem);
            }
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
            //var itemName = tbItemName.Text;
            //var quantity = nbQuantity.Text.AsInteger();
            //var unitPrice = nbUnitPrice.Text.AsDecimal();

            //if (!string.IsNullOrWhiteSpace(itemName) && quantity > 0 && unitPrice > 0)
            //{
            //    Guid invoiceItemKey;

            //    if (!string.IsNullOrWhiteSpace(hfInvoiceItemGuid.Value) && Guid.TryParse(hfInvoiceItemGuid.Value, out invoiceItemKey))
            //    {
            //        // Edit existing entry
            //        InvoiceItemState[invoiceItemKey] = new InvoiceItem
            //        {
            //            ItemName = itemName,
            //            Quantity = quantity,
            //            UnitPrice = unitPrice
            //        };
            //    }
            //    else
            //    {
            //        // Add new entry
            //        invoiceItemKey = Guid.NewGuid();
            //        InvoiceItemState[invoiceItemKey] = new InvoiceItem
            //        {
            //            ItemName = itemName,
            //            Quantity = quantity,
            //            UnitPrice = unitPrice
            //        };
            //    }
            //}

            HideDialog();
            BindInvoiceItemsGrid();
        }

        protected void BindInvoiceItemsGrid()
        {
            gInvoiceItems.DataSource = InvoiceItemState.Select(kvp => new
            {
                Guid = kvp.Key,
                ItemName = kvp.Value.Description,
                Quantity = kvp.Value.Quantity,
                UnitPrice = kvp.Value.UnitPrice,
                TotalPrice = kvp.Value.Quantity * kvp.Value.UnitPrice
            }).ToList();

            gInvoiceItems.DataBind();
        }

        protected void ClearInvoiceItemDialogFields()
        {
            //tbItemName.Text = string.Empty;
            //nbQuantity.Text = string.Empty;
            //nbUnitPrice.Text = string.Empty;
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
                    ClearAssignmentDialogFields();
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

        protected void ShowDialog(Dialogs dialog)
        {
            hfActiveDialog.Value = dialog.ToString();

            if (dialog == Dialogs.InvoiceAssignment)
            {
                var remainingPercent = 100 - InvoiceAssignmentState.Sum(a => a.Value.AssignedPercent);
                hlblCurrentAssignedTotal.LabelType = remainingPercent == 0
                    ? Rock.Web.UI.Controls.LabelType.Success
                    : Rock.Web.UI.Controls.LabelType.Warning;
                hlblCurrentAssignedTotal.Text = $"{remainingPercent:0.##}% Not Assigned";
                dlgAssignment.Show();
            }
        }


        #endregion

        #region Enums

        protected enum Dialogs
        {
            InvoiceAssignment
                , InvoiceItem
        }

        #endregion
    }
}
