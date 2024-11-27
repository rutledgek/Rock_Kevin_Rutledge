
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
        public Dictionary<int, (int AuthorizedPersonAliasId, decimal AssignedPercent)> InvoiceAssignmentState { get; set; } = new Dictionary<int, (int AuthorizedPersonAliasId, decimal AssignedPercent)>();

        private static class AttributeKey
        {
            public const string DefaultDaysLate = "DefaultDaysLate";
        }

        protected override object SaveViewState()
        {
            ViewState["AssignmentState"] = InvoiceAssignmentState;
            return base.SaveViewState();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            InvoiceAssignmentState = ViewState["AssignmentState"] as Dictionary<int, (int AuthorizedPersonAliasId, decimal AssignedPercent)>
                ?? new Dictionary<int, (int AuthorizedPersonAliasId, decimal AssignedPercent)>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);

            gAssignments.DataKeyNames = new string[] { "Id" };
            gAssignments.Actions.ShowAdd = true;
            gAssignments.Actions.AddClick += gAssignments_AddClick;
            gAssignments.GridRebind += gAssignments_GridRebind; // Attach the event handler here
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                // Load initial state if necessary
                BindAssignmentGrid();
            }

            if (!string.IsNullOrEmpty(hfActiveDialog.Value))
            {
                ShowDialog();
            }
        }

        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            // Update logic after block changes
        }

        /// <summary>
        /// Handles the AddClick event for the Assignments grid.
        /// </summary>
        private void gAssignments_AddClick(object sender, EventArgs e)
        {
            ClearDialogFields();
            ShowDialog(Dialogs.InvoiceAssignment);
        }

        /// <summary>
        /// Handles the Delete event for the Assignments grid.
        /// </summary>
        protected void gAssignment_Delete(object sender, RowEventArgs e)
        {
            var assignmentId = e.RowKeyId; // Use the dictionary key as Id
            if (InvoiceAssignmentState.ContainsKey(assignmentId))
            {
                InvoiceAssignmentState.Remove(assignmentId);
                BindAssignmentGrid();
            }
        }

        /// <summary>
        /// Handles the Save event from the modal dialog.
        /// </summary>
        protected void btnSaveAssignment_Click(object sender, EventArgs e)
        {
            var personAliasId = ppAssignment.PersonAliasId;
            var percentAssigned = numbAssignedPercent.Text.AsDecimal();

            if (personAliasId.HasValue && percentAssigned > 0)
            {
                // Generate a unique key (e.g., personAliasId or another identifier)
                var assignmentKey = personAliasId.Value;

                // Add or update the dictionary entry
                if (!InvoiceAssignmentState.ContainsKey(assignmentKey))
                {
                    InvoiceAssignmentState[assignmentKey] = (AuthorizedPersonAliasId: personAliasId.Value, AssignedPercent: percentAssigned);
                }
                else
                {
                    InvoiceAssignmentState[assignmentKey] = (AuthorizedPersonAliasId: personAliasId.Value, AssignedPercent: percentAssigned);
                }

                BindAssignmentGrid();
            }

            HideDialog();
        }

        /// <summary>
        /// Binds the Assignments grid.
        /// </summary>
        private void BindAssignmentGrid()
        {
            gAssignments.DataSource = InvoiceAssignmentState.Select(kvp => new
            {
                Id = kvp.Key, // Use the dictionary key as Id
                PersonAliasName = new PersonAliasService(new RockContext()).Get(kvp.Value.AuthorizedPersonAliasId)?.Person.FullName ?? "Unknown",
                AssignedPercent = kvp.Value.AssignedPercent
            }).ToList();

            var remainingPercent = 100 - InvoiceAssignmentState.Sum(a => a.Value.AssignedPercent);
            if (remainingPercent == 0)
            {
                hlblCurrentAssignedTotalGridView.LabelType = Rock.Web.UI.Controls.LabelType.Success;
                hlblCurrentAssignedTotalGridView.Text = $"{remainingPercent:0.##}% <strong>Not Assigned</strong> ";
            }
            else if(remainingPercent < 0)
            {
                hlblCurrentAssignedTotalGridView.Text = $" <strong>Over Assigned By: </strong> {remainingPercent:0.##}%";

            } else { 
            hlblCurrentAssignedTotalGridView.Text = $"{remainingPercent:0.##}% <strong>Not Assigned</strong> ";
            }
            gAssignments.DataBind();
        }

        /// <summary>
        /// Clears the dialog fields.
        /// </summary>
        private void ClearDialogFields()
        {
            ppAssignment.SetValue(null);
            numbAssignedPercent.Text = string.Empty;
        }

        #region Dialog Handling

        /// <summary>
        /// Shows the dialog specified in hfActiveDialog.
        /// </summary>
        private void ShowDialog()
        {
            if (Enum.TryParse(hfActiveDialog.Value, out Dialogs dialog))
            {
                ShowDialog(dialog);
            }
        }

        /// <summary>
        /// Shows the specified dialog.
        /// </summary>
        private void ShowDialog(Dialogs dialog)
        {
            hfActiveDialog.Value = dialog.ToString();

            switch (dialog)
            {
                case Dialogs.InvoiceAssignment:
                    var remainingPercent = 100 - InvoiceAssignmentState.Sum(a => a.Value.AssignedPercent);
                    if (remainingPercent == 0)
                    {
                        hlblCurrentAssignedTotal.LabelType = Rock.Web.UI.Controls.LabelType.Success;
                    }
                        hlblCurrentAssignedTotal.Text = $"{remainingPercent:0.##}% <strong>Not Assigned</strong> ";
                    dlgAssignment.Show();
                    break;
            }
        }

        private void gAssignments_GridRebind(object sender, EventArgs e)
        {
            BindAssignmentGrid();
        }

        /// <summary>
        /// Hides the active dialog.
        /// </summary>
        private void HideDialog()
        {
            if (Enum.TryParse(hfActiveDialog.Value, out Dialogs dialog))
            {
                switch (dialog)
                {
                    case Dialogs.InvoiceAssignment:
                        dlgAssignment.Hide();
                        break;
                }
            }

            hfActiveDialog.Value = string.Empty;
        }

        private enum Dialogs
        {
            InvoiceAssignment
        }

        #endregion
    }
}
 