using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Linq;
using System.Data.Entity;
using System.Web.UI.WebControls;

using Rock;
using Attribute = Rock.Model.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;











using online.kevinrutledge.InvoiceSystem.Model;
using Rock.Attribute;
using Newtonsoft.Json;
using online.kevinrutledge.InvoiceSystem.Cache;


namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Invoice Type Detail")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("Displays the details of an Invoice Type.")]
    [ContextAware(typeof(InvoiceType))]

    #region Block Attributes

    [IntegerField("Default Number of Days Until Late",
        Key = AttributeKey.DefaultDaysLate,
        Description = "The default number of days that an invoice is considered late after the due date.  This will set the slider of all invoice types to this value unless changed.",
        IsRequired = true,
        Order = 1,
        DefaultIntegerValue = 5
        )]
    #endregion

    public partial class InvoiceTypeDetail : RockBlock
    {
        private static class AttributeKey
        {
            public const string DefaultDaysLate = "DefaultDaysLate";

        }

        private List<Rock.Model.Attribute> InvoiceAttributesState { get; set; }

        private List<string> InvoiceFormAttributesState { get; set; }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            InvoiceFormAttributesState = ViewState["InvoiceformAttributesState"] as List<string> ?? new List<string>();
            
            string json = ViewState["InvoiceAttributesState"] as string;
            if (string.IsNullOrWhiteSpace(json))
            {
                InvoiceAttributesState = new List<Rock.Model.Attribute>();
            }
            else
            {
                InvoiceAttributesState = JsonConvert.DeserializeObject<List<Rock.Model.Attribute>>(json);
            }
        }



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

        // Load Values into the drop downs on the form.
        private void LoadDropDowns(RockContext rockContext)
        {
            // Clear the System Communications List
            ddlInvoiceSystemCommunication.Items.Clear();
            ddlLateNoticeSystemCommunication.Items.Clear();


            // Get System Communications with the right category.
            var communicationService = new SystemCommunicationService(rockContext);
            var invoicesystemCommunicationsCategoryId = CategoryCache.GetId(online.kevinrutledge.InvoiceSystem.SystemGuids.Categories.InvoiceSystemCommumincations.AsGuid());

            var invoiceCommunications = communicationService.Queryable()
                .AsNoTracking()
                .Where(c => c.CategoryId == invoicesystemCommunicationsCategoryId)
                .OrderBy(t => t.Title)
                .Select(a => new
                {
                    a.Id,
                    a.Title
                });


            foreach (var invoiceCommunication in invoiceCommunications)
            {
                ddlInvoiceSystemCommunication.Items.Add(
                    new ListItem(invoiceCommunication.Title, invoiceCommunication.Id.ToString())
                );
                ddlLateNoticeSystemCommunication.Items.Add(
                   new ListItem(invoiceCommunication.Title, invoiceCommunication.Id.ToString())
               );
            }

        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {

                ShowDetail();
            }
        }

        private InvoiceType _invoiceType = null;

        private void ShowDetail()
        {
            var rockContext = new RockContext();

            LoadDropDowns(rockContext);

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
            tbDescription.Text = invoiceType.Description;
            tbCssIcon.Text = invoiceType.IconCssClass;
            cbIsActive.Checked = invoiceType.IsActive;
            pgPaymentPage.SetValue( invoiceType.PaymentPageId );

            tbInvoiceTerm.Text = invoiceType.InvoiceTerm;
            tbInvoiceItemTerm.Text = invoiceType.InvoiceItemTerm;

            catpInvoiceTypeCategory.SetValue(invoiceType.CategoryId);
            
            // Populate the UI controls with the data from invoiceType
            tbName.Text = invoiceType.Name;
            tbDescription.Text = invoiceType.Description;
            cbIsActive.Checked = invoiceType.IsActive;
            tbCssIcon.Text = invoiceType.IconCssClass;
            tbInvoiceTerm.Text = invoiceType.InvoiceTerm;
            tbInvoiceItemTerm.Text = invoiceType.InvoiceItemTerm;
            rsDaysUntilLate.SelectedValue = invoiceType.DefaultDaysUntilLate;

            acctpDefaultFinancialAccount.SetValue(invoiceType.DefaultFinancialAccountId);
            numbTaxRate.Text = (invoiceType.DefaultTaxRate * 100).ToString("0.##");
            numbLateFeeAmount.Text = invoiceType.DefaultLateFeeAmount.ToString("0.##");
            numbLateFeePercent.Text = (invoiceType.DefaultLateFeePercent * 100).ToString("0.##");


            if (invoiceType.InvoiceFromPersonAliasId != null)
            {
                ppInvoiceFromPerson.SetValue(invoiceType.InvoiceFromPersonAlias.Person);
            }



            tbInvoiceFromName.Text = invoiceType.InvoiceFromName;
            tbInvoiceFromEmail.Text = invoiceType.InvoiceFromEmail;
            tbInvoiceSubject.Text = invoiceType.InvoiceSubject;

            if (invoiceType.LateNoticeFromPersonAliasId != null )
            {
                ppLateNoticeFromPerson.SetValue(invoiceType.LateNoticeFromPersonAlias.Person);
            }



            tbLateNoticeFromName.Text = invoiceType.LateNoticeFromName;
            tbLateNoticeFromEmail.Text = invoiceType.LateNoticeFromEmail;
            tbLateNoticeSubject.Text = invoiceType.LateNoticeSubject;

            ddlInvoiceSystemCommunication.SetValue(invoiceType.InvoiceSystemCommunicationId);
            ddlLateNoticeSystemCommunication.SetValue(invoiceType.LateNoticeSystemCommunicationId);

            tbInvoiceTemplate.Text = invoiceType.InvoiceCommunicationTemplate;
            tbLateNoticeTemplate.Text = invoiceType.LateNoticeCommunicationTemplate;


            // Get Default Number of Days Late from Block Settings
            var defaultDaysLateInt = GetAttributeValue(AttributeKey.DefaultDaysLate).AsInteger();

            // Use InvoiceType.DefaultDaysUntilLate if not null, otherwise fallback to defaultDaysLateInt
            rsDaysUntilLate.SelectedValue = invoiceType.DefaultDaysUntilLate ?? defaultDaysLateInt;




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
            var rockContext = new RockContext();
            var invoiceTypeService = new InvoiceTypeService(rockContext);

            int invoiceTypeId = int.Parse(hfInvoiceTypeId.Value);

            if (invoiceTypeId == 0)
            {
                invoiceType = new InvoiceType();
                invoiceTypeService.Add(invoiceType);
                invoiceType.CreatedByPersonAliasId = CurrentPersonAliasId;
                invoiceType.CreatedDateTime = RockDateTime.Now;
            }
            else
            {
                invoiceType = invoiceTypeService.Get(invoiceTypeId);
                invoiceType.ModifiedByPersonAliasId = CurrentPersonAliasId;
                invoiceType.ModifiedDateTime = RockDateTime.Now;
            }

            if( invoiceType != null )
            { 
            invoiceType.Name = tbName.Text;
            invoiceType.Description = tbDescription.Text;
            invoiceType.IsActive = cbIsActive.Checked;
            invoiceType.IconCssClass = tbCssIcon.Text;
            invoiceType.InvoiceTerm = tbInvoiceTerm.Text;
            invoiceType.InvoiceItemTerm = tbInvoiceItemTerm.Text;
            invoiceType.CategoryId = catpInvoiceTypeCategory.SelectedValueAsId();
            invoiceType.DefaultDaysUntilLate = rsDaysUntilLate.SelectedValue;



            invoiceType.DefaultFinancialAccountId = acctpDefaultFinancialAccount.SelectedValueAsInt();
            invoiceType.DefaultTaxRate = numbTaxRate.Text.AsDecimal() * 0.01m;
            invoiceType.DefaultLateFeeAmount = numbLateFeeAmount.Text.AsDecimal();
            invoiceType.DefaultLateFeePercent = numbLateFeePercent.Text.AsDecimal() * 0.01m;
            invoiceType.PaymentPageId = pgPaymentPage.SelectedValueAsInt();


            invoiceType.InvoiceFromPersonAliasId = ppInvoiceFromPerson.PersonAliasId ?? null;

            invoiceType.InvoiceFromName = tbInvoiceFromName.Text;
            invoiceType.InvoiceFromEmail = tbInvoiceFromEmail.Text;
            invoiceType.InvoiceSubject = tbInvoiceSubject.Text;

            invoiceType.LateNoticeFromPersonAliasId = ppLateNoticeFromPerson.PersonAliasId ?? null;
            invoiceType.LateNoticeFromName = tbLateNoticeFromName.Text;
            invoiceType.LateNoticeFromEmail = tbLateNoticeFromEmail.Text;
            invoiceType.LateNoticeCommunicationTemplate = tbLateNoticeTemplate.Text;
            invoiceType.LateNoticeSubject = tbLateNoticeSubject.Text;

            invoiceType.InvoiceSystemCommunicationId = ddlInvoiceSystemCommunication.SelectedValueAsId();
            invoiceType.LateNoticeSystemCommunicationId = ddlLateNoticeSystemCommunication.SelectedValueAsId();

            invoiceType.InvoiceCommunicationTemplate = tbInvoiceTemplate.Text;




            if (!invoiceType.IsValid || !Page.IsValid )
            {
                // Controls will render the error messages
                return;
            }

                rockContext.WrapTransaction(() =>
                {
                    rockContext.SaveChanges();

                    //get it back to make sure we have a good ID for it for the Attributes
                    invoiceType = invoiceTypeService.Get(invoiceType.Guid);

                    //int entityTypeId = EntityTypeCache.Get(typeof(InvoiceType)).Id;
                    //SaveAttributes(invoiceType.Id, entityTypeId, InvoiceAttributesState, rockContext); 
                } );

                InvoiceTypeCache.Clear();
                EntityTypeAttributesCache.Clear();
            

            NavigateToParentPage();
            }
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