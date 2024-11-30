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

using online.kevinrutledge.InvoiceSystem.Model;


namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Invoice Type List")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("List of all Invoice Types.")]

    [LinkedPage("Detail Page","",true, online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeDetailPage)]
    public partial class InvoiceTypeList : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            bool canEdit = IsUserAuthorized(Rock.Security.Authorization.EDIT);

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gInvoiceTypes.Actions.ShowAdd = canEdit;
            gInvoiceTypes.IsDeleteEnabled = canEdit;
            gInvoiceTypes.Actions.AddClick += gInvoiceTypes_Add;

            gInvoiceTypes.RowItemText = "Invoice Type";
            gInvoiceTypes.DataKeyNames = new string[] { "id" };

            SecurityField securityField = gInvoiceTypes.Columns.OfType<SecurityField>().First();
            securityField.EntityTypeId = EntityTypeCache.Get(typeof(InvoiceType)).Id;


            BindFilter();
        }

        private void BindFilter()
        {

            bool showInactiveTypes = gfSettings.GetFilterPreference("Show Inactive Invoice Types").AsBoolean();

            cbShowInActive.Checked = showInactiveTypes;

            var campuses = CampusCache.All();
            cpCampus.Campuses = campuses;
            cpCampus.Visible = campuses.Any();

            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                BindGrid();
            }
        }



        private void BindGrid()
        {
            var service = new InvoiceTypeService(new RockContext());
            SortProperty sortProperty = gInvoiceTypes.SortProperty;

            var query = service.Queryable();

            bool showInactiveTypes = gfSettings.GetFilterPreference("Show Inactive Invoice Types").AsBoolean();

            if (!showInactiveTypes)
            {
                query = query.Where(a => a.IsActive == true);
            }


            // Sort Results (For adding filters and sorts later)
            if ( sortProperty != null )
            {
                gInvoiceTypes.DataSource = query.Sort(sortProperty).ToList();
            }
            else
            {
                gInvoiceTypes.DataSource = query.OrderBy(a => a.Name).ToList();

            }

            gInvoiceTypes.DataBind();
        }

        protected void gfSettings_ApplyFilterClick(object sender, EventArgs e)
        {
            var gfSettingsShowInactive = gfSettings.GetFilterPreference("Show Inactive Invoice Types");

            gfSettings.SetFilterPreference("Campus", cpCampus.SelectedCampusId != null ? cpCampus.SelectedCampusId.Value.ToString() : string.Empty);
            gfSettings.SetFilterPreference("Show Inactive Invoice Types", cbShowInActive.Checked.ToString());

            BindGrid();
        }

        protected void gInvoiceTypes_Add(object sender, EventArgs e)
        {
            NavigateToDetailPage(0);
        }

        protected void gInvoiceTypes_Edit(object sender, RowEventArgs e)
        {
            NavigateToDetailPage(e.RowKeyId);
        }

        private void NavigateToDetailPage(int invoiceTypeId)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("InvoiceTypeId", invoiceTypeId.ToString());

            NavigateToLinkedPage("DetailPage", queryParams);
        }



        protected void gInvoiceTypes_Delete(object sender, RowEventArgs e)
        {
            var dataContext = new RockContext();
            var service = new InvoiceTypeService(dataContext);
            var invoiceType = service.Get((int)e.RowKeyValue);
            if (invoiceType != null)
            {
                string errorMessage;
                if (!service.CanDelete(invoiceType, out errorMessage))
                {
                    mdGridWarning.Show(errorMessage, ModalAlertType.Information);
                    return;
                }

                service.Delete(invoiceType);
                dataContext.SaveChanges();
            }

            BindGrid();
        }


        protected void gInvoiceTypes_GridRebind(object sender, EventArgs e)
        {
            BindGrid();
        }

        #endregion
    }
}