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
using Rock.Constants;
using Rock.Web.UI;

namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Invoice Type Detail")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("Displays the details of an Invoice Type.")]

    
    public partial class InvoiceTypeDetail : Rock.Web.UI.RockBlock
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);

           
        }

        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            ShowDetail();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                var campuses = CampusCache.All();
                /*
                cpCampus.Campuses = campuses;
                cpCampus.Visible = campuses.Any();
                */
                ShowDetail();
            }
        }

        private InvoiceType _invoiceType = null;

        private void ShowDetail()
        {
            pnlDetails.Visible = true;

            int? invoiceTypeId = PageParameter("InvoiceTypeId").AsIntegerOrNull();

            InvoiceType invoiceType = null;

            if (invoiceTypeId.HasValue)
            {
                invoiceType = _invoiceType ?? new InvoiceTypeService(new RockContext()).Get(invoiceTypeId.Value);
            }

            if (invoiceType != null)
            {
                RockPage.PageTitle = invoiceType.Name;
                lActionTitle.Text = ActionTitle.Edit(invoiceType.Name).FormatAsHtmlTitle();
            }
            else
            {
                invoiceType = new InvoiceType { Id = 0 };
                RockPage.PageTitle = ActionTitle.Add(InvoiceType.FriendlyTypeName);
                lActionTitle.Text = ActionTitle.Add(InvoiceType.FriendlyTypeName).FormatAsHtmlTitle();
            }


            hfInvoiceTypeId.Value = invoiceType.Id.ToString();
            tbName.Text = invoiceType.Name;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if (!IsUserAuthorized(Rock.Security.Authorization.EDIT))
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed(InvoiceType.FriendlyTypeName);
            }

            if (readOnly)
            {
                lActionTitle.Text = ActionTitle.View(InvoiceType.FriendlyTypeName);
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;



            btnSave.Visible = !readOnly;

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            InvoiceType invoiceType;
            var dataContext = new RockContext();
            var service = new InvoiceTypeService(dataContext);

            int invoiceTypeId = int.Parse(hfInvoiceTypeId.Value);

            if (invoiceTypeId == 0)
            {
                invoiceType = new InvoiceType();
                service.Add(invoiceType);
            }
            else
            {
                invoiceType = service.Get(invoiceTypeId);
            }

            invoiceType.Name = tbName.Text;
    

            if (!invoiceType.IsValid || !Page.IsValid)
            {
                // Controls will render the error messages
                return;
            }

            dataContext.SaveChanges();

            NavigateToParentPage();

        }

        public override List<BreadCrumb> GetBreadCrumbs(Rock.Web.PageReference pageReference)
        {
            var breadCrumbs = new List<BreadCrumb>();

            string crumbName = ActionTitle.Add(InvoiceType.FriendlyTypeName);

            int? invoiceTypeId = PageParameter("invoiceTypeId").AsIntegerOrNull();
            if (invoiceTypeId.HasValue)
            {
                _invoiceType = new InvoiceTypeService(new RockContext()).Get(invoiceTypeId.Value);
                if (_invoiceType != null)
                {
                    crumbName = _invoiceType.Name;
                }
            }

            breadCrumbs.Add(new BreadCrumb(crumbName, pageReference));

            return breadCrumbs;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            NavigateToParentPage();
        }

    }
}