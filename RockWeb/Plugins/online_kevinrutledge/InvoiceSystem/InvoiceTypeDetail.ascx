<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceTypeDetail.ascx.cs"
    Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceTypeDetail" %>

    <asp:UpdatePanel ID="upnlContent" runat="server">
        <ContentTemplate>


            <asp:Panel ID="pnlDetails" runat="server" Visible="false">
                <asp:HiddenField ID="hfInvoiceTypeId" runat="server" />

                <div class="banner">
                    <h1>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </h1>
                </div>

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following"
                    CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />



                <!-- General Information -->
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#generalInfo">General Information</a>
                        </h4>
                    </div>
                    <div id="generalInfo" class="panel-collapse collapse in">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-md-12">
                                    <div class="form-group">

                                        <div class="row">
                                            <div class="col-md-7">
                                                <Rock:DataTextBox ID="tbName" runat="server"
                                                    SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                    PropertyName="Name" Help="The name of the invoice type." />
                                            </div>
                                            <div class="col-md-3">
                                                <Rock:DataTextBox ID="tbCssIcon" runat="server"
                                                    SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                    PropertyName="IconCssClass"
                                                    Help="The Css Icon to show with invoices of this type." />
                                            </div>

                                            <div class="col-md-2">
                                                <Rock:RockCheckBox ID="cbIsActive" runat="server"
                                                    CssClass="js-isactivegroup" Label="Active"
                                                    Help="Is this invoice type active?" />
                                            </div>
                                        </div>



                                        <Rock:CategoryPicker ID="catpInvoiceTypeCategory" runat="server"
                                            Label="Category" Help="Choose a Category for this Invoice Type"
                                            EntityTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType" />


                                        <Rock:DataTextBox ID="tbDescription" runat="server"
                                            SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                            PropertyName="Description" TextMode="MultiLine" Rows="4"
                                            Help="Enter the description of the invoice type." />

                                    </div>
                                </div>

                            </div>
                        </div>

                    </div>
                </div>


                <!-- Invoice Settings -->
                <div class="panel panel-default ">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#invoiceSettings">Invoice Settings</a>
                        </h4>
                    </div>
                    <div id="invoiceSettings" class="panel-collapse collapse">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbInvoiceTerm" runat="server"
                                        SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                        PropertyName="InvoiceTerm" placeholder="Enter Invoice Item Term"
                                        Help="This is the term for invoices of this type. Examples include: Bill, Order, etc." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbInvoiceItemTerm" runat="server"
                                        SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                        PropertyName="InvoiceItemTerm" placeholder="Enter Invoice Item Term"
                                        Help="This ist he term to use for the invoice items in the invoice. " />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RangeSlider ID="rsDaysUntilLate" runat="server"
                                        Label="Default Days Until Late" MaxValue="31" MinValue="0" StepValue="1"
                                        SelectedValue="0"
                                        Help="This the number of days after the invoice due date that invoices of this type should be considred late." />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>


                <!-- Financial Settings -->
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#financialSettings">Financial Settings</a>
                        </h4>
                    </div>
                    <div id="financialSettings" class="panel-collapse collapse">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-md-3">
                                    <Rock:AccountPicker ID="acctpDefaultFinancialAccount" runat="server"
                                        Label="Default Financial Account"
                                        Help="Select a Financial Account to use if the financial account is not set on an invoice item." />
                                    <Rock:NumberBox ID="numbTaxRate" runat="server" Label="Default Tax Rate"
                                        Help="Enter a default tax rate to use for invoice items unless otherwise entered on the item. This value will be divided by 100 in calculations."
                                        Placeholder="0.00" NumberType="Double" />

                                </div>
                                <div class="col-md-9">
                                    <div class="panel panel-success">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">To set a default late fee for invoices of this type
                                                enter a Late Fee Amount for a
                                                flat fee or a Late Fee Percent for a fee based on the total amount.</h4>
                                        </div>
                                        <div class="panel-body">
                                            <div class="col-md-6">
                                                <Rock:CurrencyBox ID="numbLateFeeAmount" runat="server"
                                                    Label="Late Fee Amount"
                                                    Help="The amount that should be added to the invoice once it is considered late."
                                                    Placeholder="0.00" />
                                            </div>
                                            <div class="col-md-6">
                                                <Rock:NumberBox ID="numbLateFeePercent" runat="server"
                                                    Label="Late Fee Percent"
                                                    Help="The percent of the item total that should be added to the invoice item when it is considered late.  The value will be divided by 100 in calculations."
                                                    Placeholder="0.00" NumberType="Double" />
                                            </div>
                                        </div>
                                    </div>

                                </div>
                            </div>
                            

                                <Rock:PagePicker ID="pgPaymentPage" runat="server" Label="Payment Page" Help="Please select a pagement page for invoices of this type to be paid online. It should be set up to receive the entiy information, account information, and amounts." />
                            
                            
                        </div>
                    </div>

                </div>



                <!-- Invoice Settings -->
                <div class="panel panel-default ">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#invoiceCommunications">Invoice Communications</a>
                        </h4>
                    </div>
                    <div id="invoiceCommunications" class="panel-collapse collapse">
                        <div class="panel-body">

                            <div class="row">
                                <div class="col-md-6">
                                    <div class=" panel panel-success">
                                        <div class="panel-heading">

                                            <h3 class="panel-title">Invoice Communications</h3>

                                        </div>
                                        <div class="panel-body">
                                            <div class="row">
                                                <div class="col-md-6">

                                                    <Rock:PersonPicker ID="ppInvoiceFromPerson" runat="server"
                                                        Label="Send Invoices From"
                                                        Help="Select the person that the invoices should be sent from. This person should have a safe domain email address." />


                                                </div>
                                                <div class="col-md-6">
                                                    <Rock:DataTextBox ID="tbInvoiceFromName" runat="server"
                                                        Label="Invoice From Name"
                                                        SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                        PropertyName="InvoiceFromName" />
                                                    <Rock:DataTextBox ID="tbInvoiceFromEmail" runat="server"
                                                        Label="Invoice From Email"
                                                        SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                        PropertyName="InvoiceFromEmail" />




                                                </div>
                                            </div>

                                            <hr />


                                            <p>Select a system communication to use or enter your own communication
                                                template.</p>
                                            <Rock:RockDropDownList ID="ddlInvoiceSystemCommunication" runat="server"
                                                Label="Invoice System Communication"
                                                Help="The System Communication that should be used when sending invoices." />
                                            <hr />
                                            <Rock:DataTextBox ID="tbInvoiceSubject" runat="server"
                                                SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                PropertyName="InvoiceSubject" />


                                            <Rock:DataTextBox ID="tbInvoiceTemplate" runat="server"
                                                SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                PropertyName="InvoiceCommunicationTemplate"
                                                Help="The communication template to use when sending invoices of this type. {{ Lava }} Enabled"
                                                TextMode="MultiLine" Rows="15" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class=" panel panel-danger">
                                        <div class="panel-heading">

                                            <h3 class="panel-title">Late Notice Communications</h3>

                                        </div>
                                        <div class="panel-body">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <Rock:PersonPicker ID="ppLateNoticeFromPerson" runat="server"
                                                        Label="Send Invoices From"
                                                        Help="Select the person that the invoices should be sent from. This person should have a safe domain email address." />


                                                </div>
                                                <div class="col-md-6">

                                                    <Rock:DataTextBox ID="tbLateNoticeFromName" runat="server"
                                                        Label="Invoice From Name"
                                                        SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                        PropertyName="LateNoticeFromName" />
                                                    <Rock:DataTextBox ID="tbLateNoticeFromEmail" runat="server"
                                                        Label="Invoice From Email"
                                                        SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                        PropertyName="LateNoticeFromEmail" />




                                                </div>
                                            </div>

                                            <hr />
                                            <p>Select a system communication to use or enter your own communication
                                                template.</p>
                                            <Rock:RockDropDownList ID="ddlLateNoticeSystemCommunication" runat="server"
                                                Label="Late Notice System Communication"
                                                Help="The System Communication that should be used when sending late notices." />

                                            <hr />
                                            <Rock:DataTextBox ID="tbLateNoticeSubject" runat="server"
                                                SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                PropertyName="LateNoticeSubject" />
                                            <Rock:DataTextBox ID="tbLateNoticeTemplate" runat="server"
                                                SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem"
                                                PropertyName="LateNoticeCommunicationTemplate"
                                                Help="The communication template to use when sending late notices to invoices of this type. {{ Lava }} Enabled"
                                                TextMode="MultiLine" Rows="15" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>


                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary"
                        OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link"
                        CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>