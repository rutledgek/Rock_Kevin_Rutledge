#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Linq;
using System.Data.Entity;
using System.Web.UI.WebControls;
using System.Diagnostics;

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
using online.kevinrutledge.InvoiceSystem.Cache;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using Rock.Workflow.Action.Groups;
using CSScriptLibrary;


#endregion


namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    #region Block Configuration
    [DisplayName("Scheduled Invoice Detail")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("Displays the details of a Scheduled Invoice.")]
    [ContextAware(typeof(ScheduledInvoice))]

    [CustomCheckboxListField("Invoice Types",
        Description = "The invoice types that will be included on the page. Leave blank to include all. Only active invoice types are shown.",
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [_online_kevinrutledge_InvoiceSystem_InvoiceType] where [IsActive] = 1",
        Key = AttributeKeys.InvoiceTypes,
        Order = 0)]
    #endregion

    

    public partial class ScheduledInvoiceDetail : Rock.Web.UI.RockBlock
    {
        #region Constants
        private static class AttributeKeys
        {
            public const string InvoiceTypes = "InvoiceTypes";


        }

        private static class PageParameter
        {
            public const string InvoiceTypeId = "InvoiceTypeId";
            public const string ScheduledInvoiceId = "ScheduledInvoiceId";
        }

        #endregion


        #region Properties and Fields

        private ScheduledInvoice _scheduledinvoice = null; // Cached invoice instance
        private InvoiceType _invoiceType = null; // Cached Invoice Type
        private int? _invoiceTypeId = null;
        private int? _scheduledinvoiceId = null;
        private List<Guid> _allowedInvoiceTypes;
        private Schedule _schedule = null;

        #endregion

        #region State Management
        // Create Invoice Assignment State
        public List<ScheduledInvoiceAssignment> ScheduledInvoiceAssignmentState { get; set; }
        public List<ScheduledInvoiceItem> InvoiceItemState { get; set; }


 protected override void LoadViewState(object savedState)
{
    base.LoadViewState(savedState);
    LoadScheduledInvoiceAssignmentState();
    LoadInvoiceItemState();
}

private void LoadScheduledInvoiceAssignmentState()
{
    string jsonAssignment = ViewState["ScheduledInvoiceAssignmentState"] as string;
    ScheduledInvoiceAssignmentState = string.IsNullOrWhiteSpace(jsonAssignment)
        ? new List<ScheduledInvoiceAssignment>()
        : JsonConvert.DeserializeObject<List<ScheduledInvoiceAssignment>>(jsonAssignment);
}

private void LoadInvoiceItemState()
{
    string jsonItems = ViewState["InvoiceItemState"] as string;
    InvoiceItemState = string.IsNullOrWhiteSpace(jsonItems)
        ? new List<ScheduledInvoiceItem>()
        : JsonConvert.DeserializeObject<List<ScheduledInvoiceItem>>(jsonItems);
}

protected override object SaveViewState()
{
    SaveScheduledInvoiceAssignmentState();
    SaveInvoiceItemState();
    return base.SaveViewState();
}

private void SaveScheduledInvoiceAssignmentState()
{
    ViewState["ScheduledInvoiceAssignmentState"] = JsonConvert.SerializeObject(ScheduledInvoiceAssignmentState);
}

private void SaveInvoiceItemState()
{
    ViewState["InvoiceItemState"] = JsonConvert.SerializeObject(InvoiceItemState);
}



        #endregion

        #region Initialization and ViewState Handling


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


            if (ScheduledInvoiceAssignmentState == null)
            {
                ScheduledInvoiceAssignmentState = new List<ScheduledInvoiceAssignment>();
            }

            if (InvoiceItemState == null)
            {
                InvoiceItemState = new List<ScheduledInvoiceItem>();
            }

            // Register BlockUpdated event
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                // Initialize state if not already set
                if (ScheduledInvoiceAssignmentState == null)
                {
                    ScheduledInvoiceAssignmentState = new List<ScheduledInvoiceAssignment>();
                }
                if (InvoiceItemState == null)
                {
                    InvoiceItemState = new List<ScheduledInvoiceItem>();
                }

                LoadInvoiceStatusDropdown();

                ShowDetail();
            }
        }
        #endregion

        #region Data Binding and Display Methods


        /// <summary>
        /// Bind the reminder types dropdown list.
        /// </summary>
        /// <param name="invoiceTypes">The reminder types.</param>
        private void BindInvoiceTypes()
        {
            var rockContext = new RockContext();
            _allowedInvoiceTypes = GetAttributeValue(AttributeKeys.InvoiceTypes).SplitDelimitedValues().AsGuidList();
            InvoiceTypeService invoiceTypeService = new InvoiceTypeService(rockContext);

            

            // Fetches and filters the available invoice types for the dropdown list.
            // 1. Query all active InvoiceTypes from the database using InvoiceTypeService.
            // 2. Filter results in-memory after loading data with .ToList():
            //    a. Restrict to allowed GUIDs (_allowedInvoiceTypes):
            //       - If the list is empty, include all InvoiceTypes.
            //       - Otherwise, only include InvoiceTypes with a GUID present in _allowedInvoiceTypes.
            //    b. Ensure the CurrentPerson has authorization to manage invoices for the type:
            //       - The person must either have "ManageInvoices" permission or Authorization.EDIT permission for the InvoiceType.
            //    c. Include InvoiceTypes without a category (t.Category == null) or those where the associated category is authorized for the CurrentPerson:
            //       - Ensures categories without explicit restrictions are visible.
            //       - Categories with restrictions are included only if authorized.
            // 3. Convert the filtered InvoiceTypes into a dropdown-compatible format with Guid as the value and Name as the text.
            // 4. Return the results as a list for use in the application.

            var invoiceTypeOptions = new InvoiceTypeService(rockContext).Queryable()
               .Where(t => t.IsActive) // Only active invoice types
                .Where(t => !_allowedInvoiceTypes.Any() || _allowedInvoiceTypes.Contains(t.Guid)) // Restrict to allowed GUIDs
                .ToList() // Load remaining data into memory
                .Where(t => t.IsAuthorized("ManageInvoices", CurrentPerson) || t.IsAuthorized(Authorization.EDIT, CurrentPerson)) // Authorization to manage or edit invoices
                .Where(t => t.Category == null || (t.Category != null && t.Category.IsAuthorized(Authorization.VIEW, CurrentPerson))) // Category authorization or no category
                .Select(t => new { Value = t.Id, Text = t.Name }) // Format for dropdown
                .ToList(); // Return as list


            ddlInvoiceType.DataSource = invoiceTypeOptions;

            if (invoiceTypeOptions.Count == 1)
            {
                // Only one item exists, disable the dropdown and select the single item

                ddlInvoiceType.DataBind();
                _invoiceType = invoiceTypeService.Get(invoiceTypeOptions[0].Value);
                ddlInvoiceType.SelectedValue = invoiceTypeOptions[0].Value.ToString();
                ddlInvoiceType.Enabled = false; // Disable the dropdown
                hfInvoiceTypeId.Value = invoiceTypeOptions[0].Value.ToString();
            }
            else
            {
                ddlInvoiceType.DataBind();
                ddlInvoiceType.Enabled = true; // Enable the dropdown
            }

        }




        private void LoadInvoiceStatusDropdown()
        {
            // Define statuses to exclude
            var excludedStatuses = new[] { InvoiceStatus.Sent, InvoiceStatus.Paid, InvoiceStatus.PaidLate, InvoiceStatus.Late };

            // Filter out excluded statuses and prepare for dropdown binding
            var invoiceStatusList = Enum.GetValues(typeof(InvoiceStatus))
                .Cast<InvoiceStatus>()
                .Where(status => !excludedStatuses.Contains(status)) // Exclude specific statuses
                .Select(status => new
                {
                    Value = ((int)status).ToString(), // Enum value as string
                    Text = online.kevinrutledge.InvoiceSystem.Model.EnumExtensions.GetDescription(status)
            
        })
                .ToList();



            rblScheduleSelect.Items.Clear();
            rblScheduleSelect.Items.Add(new ListItem(ScheduleSelectionType.Unique.ConvertToString(), ScheduleSelectionType.Unique.ConvertToInt().ToString()));
            rblScheduleSelect.Items.Add(new ListItem(ScheduleSelectionType.NamedSchedule.ConvertToString(), ScheduleSelectionType.NamedSchedule.ConvertToInt().ToString()));

        }


        // Show Content and Fields
        protected void ShowDetail()
        {


            // Log the hidden field and dropdown value
            Debug.WriteLine($"Show Detail Fired");





            var rockContext = new RockContext();
            // Retrieve Invoice ID from Page Parameters
            int? scheduledinvoiceId = PageParameter(PageParameter.ScheduledInvoiceId).AsIntegerOrNull();
            int? invoiceTypeId = PageParameter(PageParameter.InvoiceTypeId).AsIntegerOrNull();

            ScheduledInvoice scheduledinvoice = null;

            if (_scheduledinvoice == null && scheduledinvoiceId > 0)
            {
                scheduledinvoice = new ScheduledInvoiceService(rockContext).Get(4);


            }
            else
            {
                scheduledinvoice = _scheduledinvoice;
            }


            // Fetch Invoice Type
            InvoiceType invoiceType = invoiceTypeId.HasValue ? _invoiceType ?? new InvoiceTypeService(rockContext).Get(invoiceTypeId.Value) : null;


            if (invoiceTypeId.HasValue)
            {
                hfInvoiceTypeId.Value = InvoiceTypeCache.Get(invoiceTypeId.Value).Id.ToString();
            }
            else
            {
                // Handle the case where invoiceTypeId is null
                hfInvoiceTypeId.Value = string.Empty; // or a default value
            }

            hfInvoiceId.Value = scheduledinvoiceId.ToString();

            BindInvoiceTypes();

            var invoiceTypeCount = _allowedInvoiceTypes.Count();




            upnlContent.Visible = true;

            int? invoiceTypeDaysLate;
            int? sendBefore;

            // Set Page Title and other UI elements
            if (scheduledinvoice != null)
            {
                string pageTitle = $"Invoice #{scheduledinvoice.Id}: {scheduledinvoice.Name}";
                ltlInvoiceNumberAndName.Text = pageTitle;
                RockPage.PageTitle = pageTitle;
                numbLateDays.Text = scheduledinvoice.DaysUntilLate.ToString();
                numbSendInvoiceDays.Text = scheduledinvoice.SendInvoiceDaysBefore.ToString();
                invoiceTypeDaysLate  = scheduledinvoice?.InvoiceType?.DefaultDaysUntilLate;
                sendBefore = scheduledinvoice?.InvoiceType?.DefaultSendInvoiceDaysBeforeDue;

                cbIsActive.Checked = scheduledinvoice.IsActive;



                if (scheduledinvoice.Schedule != null)
                {

                    sbSchedule.iCalendarContent = scheduledinvoice.Schedule.iCalendarContent;
                    if (!string.IsNullOrEmpty(scheduledinvoice.Schedule.Name))
                    {
                        // Named Schedule
                        rblScheduleSelect.SelectedValue = ScheduleSelectionType.NamedSchedule.ConvertToInt().ToString();
                        sbSchedule.iCalendarContent = null;
                        spSchedulePicker.SetValue(scheduledinvoice.Schedule);
                    }
                    else
                    {
                        // Unique Schedule (specific to this Scheduled Invoice)
                        rblScheduleSelect.SelectedValue = ScheduleSelectionType.Unique.ConvertToInt().ToString();
                        sbSchedule.iCalendarContent = scheduledinvoice.Schedule.iCalendarContent;

                        // set the hidden field for the scheduleId of the Unique schedule so we don't overwrite a named schedule
                        hfUniqueScheduleId.Value = scheduledinvoice.ScheduleId.ToString();
                    }

                    

                }
                else
                {
                    sbSchedule.iCalendarContent = null;
                    

                }



                rblScheduleSelect_Refresh();
                InitializeFriendlyScheduleLabel();
            }
            else
            {
                string newInvoiceTitle = "Create New Scheduled Invoice";
                RockPage.PageTitle = newInvoiceTitle;
                ltlInvoiceNumberAndName.Text = newInvoiceTitle;
                invoiceTypeDaysLate = _invoiceType?.DefaultDaysUntilLate;
                sendBefore = _invoiceType?.DefaultSendInvoiceDaysBeforeDue;
            }


             

            // Load Invoice Data into Controls
            hfInvoiceId.Value = scheduledinvoice?.Id.ToString() ?? "0";
            tbName.Text = scheduledinvoice?.Name;
            tbSummary.Text = scheduledinvoice?.Summary;

            if (invoiceTypeDaysLate.HasValue) {
                numbLateDays.Placeholder = invoiceTypeDaysLate.ToString();
            }

            if (sendBefore.HasValue)
            {
                numbSendInvoiceDays.Placeholder = sendBefore.ToString();
            }


            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            bool CanEdit = (invoiceType != null && invoiceType.IsAuthorized("ManageInvoices", CurrentPerson))
               || IsUserAuthorized(Authorization.EDIT);


            if (!CanEdit)
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed(ScheduledInvoice.FriendlyTypeName);

            }

            if (readOnly)
            {
                ltlInvoiceNumberAndName.Text = ActionTitle.View(ScheduledInvoice.FriendlyTypeName);

            }

            tbName.ReadOnly = readOnly;
            tbSummary.ReadOnly = readOnly;
            numbLateDays.ReadOnly = readOnly;
            numbSendInvoiceDays.ReadOnly = readOnly;
            numbLateDays.ReadOnly = readOnly;
            gAssignments.Actions.ShowAdd = !readOnly;
            gInvoiceItems.Actions.ShowAdd = !readOnly;




            if (scheduledinvoice?.ScheduledInvoiceAssignments != null && scheduledinvoice.ScheduledInvoiceAssignments.Any())
            {
                ScheduledInvoiceAssignmentState = scheduledinvoice?.ScheduledInvoiceAssignments?.ToList() ?? new List<ScheduledInvoiceAssignment>();
            }

            if (scheduledinvoice?.ScheduledInvoiceItems != null && scheduledinvoice.ScheduledInvoiceItems.Any())
            {
                InvoiceItemState = scheduledinvoice?.ScheduledInvoiceItems?.ToList() ?? new List<ScheduledInvoiceItem>();
            }

           

            BindAssignmentGrid();
            BindInvoiceItemsGrid();

        }

        #endregion


        #region Event Handlers and Grid Operations

        protected void sbSchedule_SaveSchedule(object sender, EventArgs e)
        {
            
            InitializeFriendlyScheduleLabel();

        }

        /// <summary>
        /// Initializes the friendly schedule label.
        /// </summary>
        private void InitializeFriendlyScheduleLabel()
        {
            lScheduleText.Visible = false;

            if (!string.IsNullOrWhiteSpace(sbSchedule.iCalendarContent))
            {
                var calEvent = InetCalendarHelper.CreateCalendarEvent(sbSchedule.iCalendarContent);
                if (sbSchedule.IsValid && calEvent != null && calEvent.DtStart != null)
                {
                    var tempSchedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
                    lScheduleText.Text = $"<span class='text-sm'>{tempSchedule.ToFriendlyScheduleText(true)}</span>";
                    lScheduleText.Visible = true;
                }
            }
        }


        
        protected void gAssignments_GridRebind(object sender, EventArgs e)
        {
            Debug.WriteLine("Assignment Grid Rebound");

            BindAssignmentGrid();
        }


        protected void gInvoiceItems_GridRebind(object sender, EventArgs e)
        {
            BindInvoiceItemsGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            // Create Rock Context and Initiate Service
            var rockContext = new RockContext();
            ScheduledInvoice invoice;

            ScheduledInvoiceService invoiceService = new ScheduledInvoiceService(rockContext);

            ScheduledInvoiceAssignmentService scheduledInvoiceAssignmentService = new ScheduledInvoiceAssignmentService(rockContext);
            ScheduledInvoiceItemService scheduledInvoiceItemService = new ScheduledInvoiceItemService(rockContext);
            InvoiceTypeService invoiceTypeService = new InvoiceTypeService(rockContext);
            ScheduleService scheduleService = new ScheduleService(rockContext);
            


            int scheduledInvoiceId = int.Parse(hfInvoiceId.Value);
            int invoiceTypeId = ddlInvoiceType.SelectedValueAsInt() ?? 0;

            var scheduleSelectionType = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>();

            InvoiceType invoiceType = invoiceTypeService.Get(invoiceTypeId);



            if (scheduledInvoiceId == 0)
            {
                invoice = new ScheduledInvoice();
                invoiceService.Add(invoice);
                invoice.CreatedByPersonAliasId = CurrentPersonAliasId;
                invoice.CreatedDateTime = DateTime.Now;
            }
            else
            {
                invoice = invoiceService.Get(scheduledInvoiceId);
                invoice.ModifiedByPersonAliasId = CurrentPersonAliasId;
                invoice.CreatedDateTime = DateTime.Now;
            }

            if (invoice != null)
            {




                invoice.Name = tbName.Text;
                invoice.Summary = tbSummary.Text;
                invoice.IsActive = cbIsActive.Checked;
                string scheduleiCalendarContent = sbSchedule.iCalendarContent;
                invoice.InvoiceTypeId = invoiceTypeId;


                if (scheduleSelectionType == ScheduleSelectionType.NamedSchedule)
                {
                    invoice.ScheduleId = (int)spSchedulePicker.SelectedValueAsId();
                }
                else
                {
                    invoice.ScheduleId = hfUniqueScheduleId.ValueAsInt();
                }


                Schedule schedule = scheduleService.Get(invoice.ScheduleId ?? 0);

                if (schedule == null)
                {
                    schedule = new Schedule();

                    // make it an "Unnamed" metrics schedule
                    schedule.Name = string.Empty;
                    

                }
                // if the schedule was a unique schedule (configured in the Metric UI, set the schedule's iCal content to the schedule builder UI's value
                if (scheduleSelectionType == ScheduleSelectionType.Unique)
                {
                    schedule.iCalendarContent = sbSchedule.iCalendarContent;
                }

                if (!schedule.HasSchedule() && scheduleSelectionType == ScheduleSelectionType.Unique)
                {
                    // don't save as a unique schedule if the schedule doesn't do anything
                    schedule = null;
                }

                else
                {
                    if (schedule.Id == 0)
                    {
                        scheduleService.Add(schedule);

                        // save to make sure we have a scheduleId
                        rockContext.SaveChanges();
                    }
                }

                if (schedule != null)
                {
                    invoice.ScheduleId = schedule.Id;
                }
                else
                {
                    invoice.ScheduleId = null;
                }

                int? SendInvoiceDaysBefore = invoiceType.DefaultSendInvoiceDaysBeforeDue;
                int? LateDays = invoiceType.DefaultDaysUntilLate;

                if(!string.IsNullOrWhiteSpace(numbLateDays.Text))
                {
                    LateDays = numbLateDays.Text.AsIntegerOrNull();
                }

                if (!string.IsNullOrWhiteSpace(numbSendInvoiceDays.Text))
                {
                    SendInvoiceDaysBefore = numbSendInvoiceDays.Text.AsIntegerOrNull();
                }

                invoice.DaysUntilLate = (int)LateDays;
                invoice.SendInvoiceDaysBefore = (int)SendInvoiceDaysBefore;

                rockContext.SaveChanges();

                // Get the Invoice Back to make sure we have a good Id for saving the assignments and items.
                invoice = invoiceService.Get(invoice.Guid);

                foreach (var stateItem in InvoiceItemState)
                {
                    var existingItem = invoice.ScheduledInvoiceItems.FirstOrDefault(a => a.Guid == stateItem.Guid);

                    if (existingItem != null)
                    {
                        //Update the existing item's properties
                        existingItem.ModifiedDateTime = RockDateTime.Now;
                        existingItem.ModifiedByPersonAliasId = CurrentPersonAliasId;

                        //Update Properties
                        existingItem.TaxRate = stateItem.TaxRate;
                        existingItem.Quantity = stateItem.Quantity;
                        existingItem.UnitPrice = stateItem.UnitPrice;
                        existingItem.DiscountAmount = stateItem.DiscountAmount;
                        existingItem.DiscountPercent = stateItem.DiscountPercent;
                        existingItem.Description = stateItem.Description;
                    }
                    else
                    {
                        var newItem = new ScheduledInvoiceItem
                        {
                            Guid = stateItem.Guid != Guid.Empty ? stateItem.Guid : Guid.NewGuid(), // Ensure a GUID is assigned
                            ScheduledInvoiceId = invoice.Id,
                            Description = stateItem.Description,
                            TaxRate = stateItem.TaxRate,
                            Quantity = stateItem.Quantity,
                            UnitPrice = stateItem.UnitPrice,
                            DiscountAmount = stateItem.DiscountAmount,
                            DiscountPercent = stateItem.DiscountPercent,
                            CreatedDateTime = RockDateTime.Now,
                            CreatedByPersonAliasId = CurrentPersonAliasId,
                        };

                        invoice.ScheduledInvoiceItems.Add(newItem);
                    }

                }

                // Convert state Guids into a HashSet for fast lookup
                var stateGuids = InvoiceItemState.Select(i => i.Guid).ToHashSet();


                // Find items to remove
                var itemsToRemove = invoice.ScheduledInvoiceItems
                    .Where(item => !stateGuids.Contains(item.Guid))
                    .ToList(); // Create a list to avoid modifying the collection while iterating


                // Remove items
                foreach (var item in itemsToRemove)
                {
                    scheduledInvoiceItemService.Delete(item);
                }


                foreach (var stateItem in ScheduledInvoiceAssignmentState)
                {
                    // Check if an assignment with the same GUID exists in the invoice
                    var existingAssignment = invoice.ScheduledInvoiceAssignments.FirstOrDefault(a => a.Guid == stateItem.Guid);

                    if (existingAssignment != null)
                    {
                        // Update the existing assignment's properties
                        existingAssignment.AuthorizedPersonAliasId = stateItem.AuthorizedPersonAliasId;
                        existingAssignment.AssignedPercent = stateItem.AssignedPercent;
                        existingAssignment.ModifiedDateTime = RockDateTime.Now;
                        existingAssignment.ModifiedByPersonAliasId = CurrentPersonAliasId;
                    }
                    else
                    {
                        // Create a new assignment if none exists
                        var newAssignment = new ScheduledInvoiceAssignment
                        {
                            Guid = stateItem.Guid != Guid.Empty ? stateItem.Guid : Guid.NewGuid(), // Ensure a GUID is assigned
                            AuthorizedPersonAliasId = stateItem.AuthorizedPersonAliasId,
                            AssignedPercent = stateItem.AssignedPercent,
                            ScheduledInvoiceId = invoice.Id
                        };

                        invoice.ScheduledInvoiceAssignments.Add(newAssignment);
                    }
                }

                // Remove assignments not present in InvoiceAssignmentState
                var assignmentStateGuids = ScheduledInvoiceAssignmentState.Select(i => i.Guid).ToHashSet();

                // Check if InvoiceAssignments is a List and remove items accordingly
                if (invoice.ScheduledInvoiceAssignments is List<ScheduledInvoiceAssignment> assignmentList)
                {
                    assignmentList.RemoveAll(item => !assignmentStateGuids.Contains(item.Guid));
                }
                else
                {
                    // Manual removal for other collection types
                    var assignmentsToRemove = invoice.ScheduledInvoiceAssignments
                        .Where(item => !assignmentStateGuids.Contains(item.Guid))
                        .ToList(); // Create a list to avoid modifying the collection while iterating

                    foreach (var assignment in assignmentsToRemove)
                    {
                        scheduledInvoiceAssignmentService.Delete(assignment);
                    }
                }

                rockContext.SaveChanges();
                NavigateToParentPage();

            }



        }

        protected void BindAssignmentGrid()
        {

            using (var rockContext = new RockContext())
            {
                var personAliasService = new PersonAliasService(rockContext);
                var authorizedPersonAliasIds = ScheduledInvoiceAssignmentState.Select(v => v.AuthorizedPersonAliasId).ToList();

                // Fetch Person Names for the Assignments
                var personAliases = personAliasService.Queryable()
                    .Where(pa => authorizedPersonAliasIds.Contains(pa.Id))
                    .ToDictionary(pa => pa.Id, pa => pa.Person.FullName);

                // Bind data to the grid
                gAssignments.DataSource = ScheduledInvoiceAssignmentState.Select(assignment => new
                {
                    Guid = assignment.Guid,
                    PersonAliasName = personAliases.TryGetValue(assignment.AuthorizedPersonAliasId, out var fullName)
                        ? fullName
                        : "Unknown",
                    AssignedPercent = assignment.AssignedPercent,
      
                }).ToList();

                gAssignments.DataBind();
                hlblCurrentAssignedTotalGridView.Text = $"{CalculateRemainingAssignedPercent():0.##}% Not Assigned";
            }
        }


        protected void btnCancel_Click(object sender, EventArgs e)
        {




            NavigateToParentPage();
            // 
        }
        #endregion

        #region Modal Controls
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





        protected void ShowDialog()
        {
            if (Enum.TryParse(hfActiveDialog.Value, out Dialogs dialog))
            {
                ShowDialog(dialog);
            }
        }

        protected decimal CalculateRemainingAssignedPercent()
        {
            var remainingPercent = 100 - ScheduledInvoiceAssignmentState.Sum(a => a.AssignedPercent);
            return remainingPercent;
        }

        protected void ShowDialog(Dialogs dialog)
        {
            hfActiveDialog.Value = dialog.ToString();

            if (dialog == Dialogs.ScheduledInvoiceAssignment)
            {
                var remainingPercent = CalculateRemainingAssignedPercent();
                hlblCurrentAssignedTotal.LabelType = remainingPercent == 0
                    ? Rock.Web.UI.Controls.LabelType.Success
                    : Rock.Web.UI.Controls.LabelType.Warning;
                hlblCurrentAssignedTotal.Text = $"{remainingPercent:0.##}% Not Assigned";
                dlgAssignment.Show();
            }
            if (dialog == Dialogs.ScheduledInvoiceItem)
            {
                dlgInvoiceItem.Show();

            }
        }




        protected void ClearActiveDialog()
        {

            HideDialog();
            ClearDialogFields();
            BindAssignmentGrid();
        }

        protected void HideDialog()
        {

            if (Enum.TryParse(hfActiveDialog.Value, out Dialogs dialog))
            {
                if (dialog == Dialogs.ScheduledInvoiceAssignment)
                {
                    dlgAssignment.Hide();
                    ClearDialogFields();
                }

                if (dialog == Dialogs.ScheduledInvoiceItem)
                {
                    dlgInvoiceItem.Hide();

                }
            }

            hfActiveDialog.Value = string.Empty;
        }



        #endregion

        #region Page Methods


        public override List<BreadCrumb> GetBreadCrumbs(Rock.Web.PageReference pageReference)
        {
            var breadCrumbs = new List<BreadCrumb>();

            string crumbName = ActionTitle.Add(Invoice.FriendlyTypeName);

            int? invoiceId = PageParameter(PageParameter.ScheduledInvoiceId).AsIntegerOrNull();

            if (invoiceId.HasValue)
            {
                _scheduledinvoice = new ScheduledInvoiceService(new RockContext()).Get(invoiceId.Value);
                if (_scheduledinvoice != null)
                {
                    crumbName = $"Invoice #{_scheduledinvoice.Id}: {_scheduledinvoice.Name}";
                }
            }

            breadCrumbs.Add(new BreadCrumb(crumbName, pageReference));

            return breadCrumbs;
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
            
            numbAssignedPercent.Text = CalculateRemainingAssignedPercent().ToString();
            ShowDialog(Dialogs.ScheduledInvoiceAssignment);
        }


        protected void gAssignments_RowSelected(object sender, RowEventArgs e)
        {
            Guid assignmentKey = (Guid)e.RowKeyValue;

            // Find the selected assignment in the state list
            var selectedAssignment = ScheduledInvoiceAssignmentState.FirstOrDefault(a => a.Guid == assignmentKey);

            if (selectedAssignment != null)
            {
                // Prefill dialog fields
                var personAlias = new PersonAliasService(new RockContext()).Get(selectedAssignment.AuthorizedPersonAliasId);
                ppAssignment.SetValue(personAlias?.Person); // Ensure the PersonPicker displays the correct person
                numbAssignedPercent.Text = selectedAssignment.AssignedPercent.ToString();

                // Store the GUID in a hidden field for editing
                hfAssignmentGuid.Value = assignmentKey.ToString();

                // Show dialog
                ShowDialog(Dialogs.ScheduledInvoiceAssignment);
            }
        }


        protected void gAssignment_Delete(object sender, RowEventArgs e)
        {
            if (e.RowKeyValue is Guid assignmentKey)
            {
                // Find the assignment to remove based on the GUID
                var assignmentToRemove = ScheduledInvoiceAssignmentState.FirstOrDefault(a => a.Guid == assignmentKey);

                if (assignmentToRemove != null)
                {
                    // Remove the assignment from the state list
                    ScheduledInvoiceAssignmentState.Remove(assignmentToRemove);

                    // Rebind the grid to reflect the changes
                    BindAssignmentGrid();
                }
            }
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
                    var existingAssignment = ScheduledInvoiceAssignmentState.FirstOrDefault(a => a.Guid == assignmentKey);
                    if (existingAssignment != null)
                    {
                        existingAssignment.AuthorizedPersonAliasId = personAliasId.Value;
                        existingAssignment.AssignedPercent = percentAssigned;
                    }
                }
                else
                {
                    // Add new entry
                    ScheduledInvoiceAssignmentState.Add(new ScheduledInvoiceAssignment
                    {
                        Guid = Guid.NewGuid(),
                        AuthorizedPersonAliasId = personAliasId.Value,
                        AssignedPercent = percentAssigned
                    });
                }
            }

            HideDialog();
            BindAssignmentGrid();
        }








        #endregion


        #region InvoiceItems Grid Methods and Events



        protected void gInvoiceItems_AddClick(object sender, EventArgs e)
        {
            var rockContext = new RockContext();
            InvoiceTypeService invoiceTypeService = new InvoiceTypeService(rockContext);

            // Fetch Invoice Type
            InvoiceType invoiceType = invoiceTypeService.Get((int)ddlInvoiceType.SelectedValueAsInt());

            // Clear dialog fields for adding a new Invoice Item
            hfInvoiceItemGuid.Value = string.Empty; // Clear the GUID for new Invoice Items
            tbItemDescription.Text = string.Empty;
            numbUnitPrice.Text = string.Empty;
            numbQuantity.Text = "1"; // Default quantity
            numbTaxPercent.Placeholder = invoiceType?.DefaultTaxRate.ToString("F2") ?? string.Empty;

            numbDiscountAmount.Text = string.Empty;
            numbDiscountPercent.Text = string.Empty;



            // Show dialog
            ShowDialog(Dialogs.ScheduledInvoiceItem);
        }

        protected void gInvoiceItems_RowSelected(object sender, RowEventArgs e)
        {
            Guid invoiceItemKey = (Guid)e.RowKeyValue;

            // Retrieve the selected item by its GUID
            var selectedItem = InvoiceItemState.FirstOrDefault(item => item.Guid == invoiceItemKey);

            dlgInvoiceItem.SaveButtonText = "Save Changes";
            if (selectedItem != null)
            {
                var rockContext = new RockContext();
                InvoiceTypeService invoiceTypeService = new InvoiceTypeService(rockContext);

                // Fetch Invoice Type
                InvoiceType invoiceType = invoiceTypeService.Get((int)ddlInvoiceType.SelectedValueAsInt());

                decimal taxRate = selectedItem.TaxRate ?? invoiceType?.DefaultTaxRate ?? 0M;
                // Prefill dialog fields with the selected item's values
                tbItemDescription.Text = selectedItem.Description;
                numbUnitPrice.Text = selectedItem.UnitPrice.ToStringOrDefault("F2"); // Format as decimal with 2 places
                numbQuantity.Text = selectedItem.Quantity.ToString();
                numbTaxPercent.Placeholder = invoiceType?.DefaultTaxRate.ToString("F2") ?? string.Empty;
                numbDiscountAmount.Text = selectedItem.DiscountAmount?.ToString("F2") ?? string.Empty; // Handle nullable decimal
                numbDiscountPercent.Text = selectedItem.DiscountPercent?.ToString("F2") ?? string.Empty; // Optional discount percentage

                // Store the GUID in a hidden field for editing
                hfInvoiceItemGuid.Value = invoiceItemKey.ToString();
            }

            // Show dialog
            ShowDialog(Dialogs.ScheduledInvoiceItem);
        }

        protected void gInvoiceItem_Delete(object sender, RowEventArgs e)
        {
            Guid invoiceItemKey = (Guid)e.RowKeyValue;

            // Remove the selected item from the InvoiceItemState list
            var itemToRemove = InvoiceItemState.FirstOrDefault(item => item.Guid == invoiceItemKey);
            if (itemToRemove != null)
            {
                InvoiceItemState.Remove(itemToRemove);
                BindInvoiceItemsGrid(); // Refresh the grid after deletion
            }
        }

        #endregion

        #region Helper Methods

        protected void btnSaveInvoiceItem_Click(object sender, EventArgs e)
        {
            var rockContext = new RockContext();
            InvoiceTypeService invoiceTypeService = new InvoiceTypeService(rockContext);

            var description = tbItemDescription.Text;
            var quantity = numbQuantity.Text.AsInteger();
            var unitPrice = numbUnitPrice.Text.AsDecimal();
            var taxRate = numbTaxPercent.Text.AsDecimalOrNull();
            var discountAmount = numbDiscountAmount.Text.AsDecimalOrNull();
            var discountPercent = numbDiscountPercent.Text.AsDecimalOrNull();

            if (!string.IsNullOrWhiteSpace(description) && quantity > 0 && unitPrice >= 0)
            {
                Guid itemKey;

                if (!string.IsNullOrWhiteSpace(hfInvoiceItemGuid.Value) && Guid.TryParse(hfInvoiceItemGuid.Value, out itemKey))
                {
                    // Edit existing entry
                    var existingItem = InvoiceItemState.FirstOrDefault(i => i.Guid == itemKey);
                    if (existingItem != null)
                    {
                        existingItem.Description = description;
                        existingItem.Quantity = quantity;
                        existingItem.UnitPrice = unitPrice;
                        existingItem.TaxRate = taxRate;
                        existingItem.DiscountAmount = discountAmount;
                        existingItem.DiscountPercent = discountPercent;
                    }
                }
                else
                {
                    // Add new entry
                    InvoiceItemState.Add(new ScheduledInvoiceItem
                    {
                        Guid = Guid.NewGuid(),
                        Description = description,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TaxRate = taxRate,
                        DiscountAmount = discountAmount,
                        DiscountPercent = discountPercent
                    });
                }
            }

            HideDialog();
            BindInvoiceItemsGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblScheduleSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblScheduleSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            spSchedulePicker.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.NamedSchedule;
            sbSchedule.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.Unique;
            lScheduleText.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.Unique;
        }

        protected void rblScheduleSelect_Refresh()
        {
            spSchedulePicker.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.NamedSchedule;
            sbSchedule.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.Unique;
            lScheduleText.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.Unique;
        }

        protected void BindInvoiceItemsGrid()
        {
            var rockContext = new RockContext();
            InvoiceTypeService invvoiceTypeService = new InvoiceTypeService(rockContext);


            var dataSource = new List<dynamic>();

            decimal subtotal = 0;
            decimal discountTotal = 0;
            decimal preTaxTotal = 0;
            decimal taxTotal = 0;  
            decimal finalTotal = 0;

            if (_invoiceType == null)
            {
                _invoiceType = invvoiceTypeService.Get((int)ddlInvoiceType.SelectedValueAsInt());

            }

            // Iterate through the list of InvoiceItems
            foreach (var item in InvoiceItemState)
            {
                var totalPrice = item.Quantity * item.UnitPrice;

                // Calculate discounts
                var discountAmountValue = item.DiscountAmount ?? 0;
                var discountPercentValue = totalPrice * (item.DiscountPercent ?? 0) / 100;

                // Ensure both values are decimals
                var totalDiscount = Math.Max((decimal)(discountAmountValue * item.Quantity), (decimal)discountPercentValue);
                var priceAfterDiscount = totalPrice - totalDiscount;

                // Calculate tax
                decimal taxRate = item.TaxRate ?? (_invoiceType?.DefaultTaxRate ?? 0M);
                decimal taxAmount = (decimal)(priceAfterDiscount * (taxRate / 100));


                // Aggregate values
                subtotal += (decimal)totalPrice;
                discountTotal += totalDiscount;
                preTaxTotal += (decimal)priceAfterDiscount;
                taxTotal += (decimal)taxAmount;
                finalTotal += (decimal)(priceAfterDiscount + taxAmount);

                // Add row data
                dataSource.Add(new
                {
                    Guid = item.Guid,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Description = item.Description,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = item.DiscountAmount,
                    UnitDiscount = Math.Max((decimal)(discountAmountValue), (decimal)discountPercentValue),
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

            //if(_scheduledinvoice?.ShouldIncludeLateFee == true)
            //{
            //    decimal lateFee = _scheduledinvoice?.ShouldIncludeLateFee == true ? _scheduledinvoice.CalculatedLateFee : 0M;
            //    litLateFee.Text = lateFee.ToString("C");
            //    finalTotal += lateFee;


                
            //}
            //else
            //{
            //    divLateFee.Visible = false;
            //}

            litInvoiceFinalTotal.Text = finalTotal.ToString("C");


            // Bind data to grid
            gInvoiceItems.DataSource = dataSource;
            gInvoiceItems.DataBind();
        }





        #endregion

        #region Enums

        /// <summary>
        ///
        /// </summary>
        public enum ScheduleSelectionType
        {
            Unique = 0,
            NamedSchedule = 1
        }

        protected enum Dialogs
        {
            ScheduledInvoiceAssignment
                , ScheduledInvoiceItem
        }

        #endregion
    }
}
