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

            bool canAddEditDelete = IsUserAuthorized(Rock.Security.Authorization.EDIT);

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gInvoiceTypes.Actions.ShowAdd = canAddEditDelete;
            gInvoiceTypes.IsDeleteEnabled = canAddEditDelete;
            gInvoiceTypes.Actions.AddClick += gInvoiceTypes_Add;

            gInvoiceTypes.RowItemText = "Invoice Type";
            gInvoiceTypes.DataKeyNames = new string[] { "id" };

            var securityField = gInvoiceTypes.Columns.OfType<SecurityField>().FirstOrDefault();
            if( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get(typeof(InvoiceType)).Id;
            }



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
            var service = new InvoiceTypeService( new RockContext() );
            SortProperty sortProperty = gInvoiceTypes.SortProperty;

            var query = service.Queryable();

            bool showInactiveTypes = gfSettings.GetFilterPreference("Show Inactive Invoice Types").AsBoolean();
            int? limitCategory = gfSettings.GetFilterPreference("Limit To Category").AsIntegerOrNull();


            if (!showInactiveTypes)
            {
                query = query.Where(a => a.IsActive == true);
            }

            if(limitCategory.HasValue)
            {
                query = query.Where(a => a.CategoryId == limitCategory);
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
            var gfSettingsLimitToCategory = gfSettings.GetFilterPreference("Limit To Category");

            gfSettings.SetFilterPreference("Campus", cpCampus.SelectedCampusId != null ? cpCampus.SelectedCampusId.Value.ToString() : string.Empty);
            gfSettings.SetFilterPreference("Show Inactive Invoice Types", cbShowInActive.Checked.ToString());
            gfSettings.SetFilterPreference("Limit To Category", cpCategory.SelectedValueAsId() != null ? cpCategory.SelectedValueAsId().ToString() : string.Empty);

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
            var rockContext = new RockContext();
            var invoiceTypeService = new InvoiceTypeService( rockContext );
            var invoiceType = invoiceTypeService.Get((int)e.RowKeyValue);
            if (invoiceType != null)
            {

                int invoiceTypeId = invoiceType.Id;
                if( !invoiceType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson))
                {
                    mdGridWarning.Show("Sorry, you are not authorized to delete this Invoice Type.", ModalAlertType.Alert);
                    return;
                }
                string errorMessage;
                if (!invoiceTypeService.CanDelete(invoiceType, out errorMessage))
                {
                    mdGridWarning.Show(errorMessage, ModalAlertType.Information);
                    return;
                }


                invoiceTypeService.Delete(invoiceType);
                rockContext.SaveChanges();
                InvoiceTypeCache.Remove(invoiceTypeId);
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