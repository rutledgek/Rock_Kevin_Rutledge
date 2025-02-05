using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Security;

using online.kevinrutledge.InvoiceSystem.Model;
using online.kevinrutledge.InvoiceSystem.Cache;


namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Scheduled Invoice List")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("List of all Scheduled Invoices.")]

    [LinkedPage("Detail Page","",true, online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceDetailPage)]
    [CustomCheckboxListField("Invoice Types",
        Description = "The invoice types that will be included on the page. Leave blank to include all. Only active invoice types are shown.",
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [_online_kevinrutledge_InvoiceSystem_InvoiceType] where [IsActive] = 1",
        Key = AttributeKeys.InvoiceTypes,
        Order = 0)]
    public partial class ScheduledInvoiceList : Rock.Web.UI.RockBlock
    {
        #region Constants
        private static class AttributeKeys
        {
            public const string InvoiceTypes = "InvoiceTypes";


        }

        private static class PageParameter
        {
            public const string InvoiceTypeId = "InvoiceTypeId";
        }

        #endregion

        #region Properties and Fields

        private List<Guid> _allowedInvoiceTypes;
        #endregion

        #region Base Control Methods


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            bool canAddEditDelete = IsUserAuthorized(Rock.Security.Authorization.EDIT);

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gScheduledInvoices.Actions.ShowAdd = canAddEditDelete;
            gScheduledInvoices.IsDeleteEnabled = canAddEditDelete;
            gScheduledInvoices.Actions.AddClick += gScheduledInvoices_Add;

            gScheduledInvoices.RowItemText = "ScheduledInvoice";
            gScheduledInvoices.DataKeyNames = new string[] { "id" };

            
            BindFilter();
        }

        private void BindFilter()
        {
            LoadInvoiceTpyeFilterOptions();


            bool showInactiveTypes = gfSettings.GetFilterPreference("Show Inactive Invoice Types").AsBoolean();

            

            var campuses = CampusCache.All();
            
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                BindGrid();
            }
        }

       protected void LoadInvoiceTpyeFilterOptions()
        {
            _allowedInvoiceTypes = GetAttributeValue(AttributeKeys.InvoiceTypes).SplitDelimitedValues().AsGuidList();

            var rockContext = new RockContext();

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


            cblInvoiceTypes.DataSource = invoiceTypeOptions;

            if (invoiceTypeOptions.Count == 1)
            {
                // Only one item exists, disable the dropdown and select the single item

                cblInvoiceTypes.DataBind();
                cblInvoiceTypes.SelectedValue = invoiceTypeOptions[0].Value.ToString();
                cblInvoiceTypes.Enabled = false; // Disable the dropdown
                hfInvoiceTypeIds.Value = invoiceTypeOptions[0].Value.ToString();
            }
            else
            {
                cblInvoiceTypes.DataBind();
                cblInvoiceTypes.Enabled = true; // Enable the dropdown
            }

        }

        private void BindGrid()
        {
            var service = new ScheduledInvoiceService( new RockContext() );
            SortProperty sortProperty = gScheduledInvoices.SortProperty;

            var query = service.Queryable();

           

            // Sort Results (For adding filters and sorts later)
            if ( sortProperty != null )
            {
                gScheduledInvoices.DataSource = query.Sort(sortProperty).ToList();
            }
            else
            {
                gScheduledInvoices.DataSource = query.OrderBy(a => a.Name).ToList();

            }

            gScheduledInvoices.DataBind();
        }

        protected void gfSettings_ApplyFilterClick(object sender, EventArgs e)
        {
            var gfSettingsShowInactive = gfSettings.GetFilterPreference("Show Inactive Invoice Types");
            var gfSettingsLimitToCategory = gfSettings.GetFilterPreference("Limit To Category");

            
            
            

            BindGrid();
        }

        protected void gScheduledInvoices_Add(object sender, EventArgs e)
        {
            NavigateToDetailPage(0);
        }

        protected void gScheduledInvoices_Edit(object sender, RowEventArgs e)
        {
            NavigateToDetailPage(e.RowKeyId);
        }

        private void NavigateToDetailPage(int invoiceId)
        {
            var queryParams = new Dictionary<string, string>();




            queryParams.Add("ScheduledInvoiceId", invoiceId.ToString());


            NavigateToLinkedPage("DetailPage", queryParams);
        }



      


        protected void gInvoiceTypes_GridRebind(object sender, EventArgs e)
        {
            BindGrid();
        }

        #endregion
    }
}