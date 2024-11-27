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


namespace RockWeb.Plugins.online_kevinrutledge.InvoiceSystem
{
    [DisplayName("Invoice Detail")]
    [Category("online_kevinrutledge > Invoice System")]
    [Description("Displays the details of an Invoice.")]
    [ContextAware(typeof(Invoice))]

    #region Block Attributes

    #endregion
    public partial class InvoiceDetail : Rock.Web.UI.RockBlock
    {

        private static class AttributeKey
        {
            public const string DefaultDaysLate = "DefaultDaysLate";

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);


        }

        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            //ShowDetail();
        }
    }

}
