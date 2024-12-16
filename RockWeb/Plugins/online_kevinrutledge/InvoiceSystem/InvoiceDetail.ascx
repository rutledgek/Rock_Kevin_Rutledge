<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceDetail.ascx.cs"
    Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceDetail" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <asp:HiddenField ID="hfInvoiceId" runat="server" />
        <asp:HiddenField ID="hfInvoiceTypeGuid" runat="server" />
        <asp:HiddenField ID="hfInvoiceTypeId" runat="server" />
        <asp:HiddenField ID="hfAssignmentGuid" runat="server" />
        <asp:HiddenField ID="hfInvoiceItemGuid" runat="server" />
        <asp:HiddenField ID="hfremainingPercent" runat="server" />

        <div class="banner">
            <h1>
                <asp:Literal ID="lActionTitle" runat="server" />
            </h1>
        </div>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following"
            CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal runat="server" ID="ltlInvoiceNumberAndName" />

                </h1>
                <Rock:HighlightLabel ID="hlblInvoiceStatus" runat="server" LabelType="Warning" Text="Draft" />
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-8">

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockDropDownList ID="ddlInvoiceType" runat="server" Label="Invoice Type" DataTextField="Text" DataValueField="Value" />
                                <Rock:RockDropDownList ID="ddlInvoiceStatus" runat="server" Label="Invoice Status" DataTextField="Text" DataValueField="Value" />
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.Invoice, online.kevinrutledge.InvoiceSystem" PropertyName="Name" />


                            </div>
                        </div>

                        <!-- Invoice Details Section -->
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbSummary" runat="server"
                                    SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.Invoice, online.kevinrutledge.InvoiceSystem"
                                    PropertyName="Summary" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>

                    </div>

                    <div class="col-md-4">


                        <div class="row">
                            <div class="col-md-12">

                                <div class="panel panel-block">
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Payee List</h1>
                                        <Rock:HighlightLabel ID="hlblCurrentAssignedTotalGridView" runat="server"
                                            LabelType="Danger" />
                                    </div>
                                    <div class="panel-body">

                                        <div class="grid grid-panel">
                                            <Rock:Grid ID="gAssignments" runat="server" DisplayType="Light"
                                                RowItemText="Assignment" ShowConfirmDeleteDialog="false"
                                                DataKeyNames="Guid">
                                                <Columns>
                                                    <Rock:RockBoundField DataField="PersonAliasName"
                                                        HeaderText="Person" />
                                                    <Rock:RockBoundField DataField="AssignedPercent"
                                                        HeaderText="Percent Assigned" />
                                                    <Rock:EditField OnClick="gAssignments_RowSelected" />
                                                    <Rock:DeleteField OnClick="gAssignment_Delete" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
                                        <div class="row">
                            <div class="col-md-12">


                                <div class="panel panel-warning">
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Due and Late Payment Date:</h1>
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">

                                            <div class="col-md-12">
                                                <p>the late date for the invoice or the number of days after the due date when it should be considered late. If neither is provided, the default value from the invoice type will be used.</p>
                                            </div>
                                        </div>

                                        <div class="col-lg-12 col-md-12">
                                            <Rock:DatePicker ID="dpDueDate" runat="server" Label="Due Date" Required="true" />
                                            <hr />
                                        </div>

                                        <div class="col-lg-4 col-md-12">
                                            <Rock:DatePicker ID="dpLateDate" runat="server" Label="Late Date"
                                                Help="The Date the invoice will be considered late.  Alternatively, you can select a number of days after the due date." />
                                        </div>
                                        <div class="col-lg-4 col-md-12">
                                            <Rock:NumberBox ID="numbLateDays" runat="server"
                                                Default="5"
                                                Label="Number of Days After Due Date"
                                                Help="This is the number of days after the Due Date the invoice should be considered late.  This will be used to calculate the due date." />

                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                <hr />
                <div class="row">
                    <div class="col-md-8">

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title">Invoice Items</h1>
                            </div>
                            <div class="panel-body">

                                <div class="grid grid-panel">
                                    <Rock:Grid ID="gInvoiceItems" runat="server" DisplayType="Light"
                                        RowItemText="Item" ShowConfirmDeleteDialog="false"
                                        DataKeyNames="Guid">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                            <Rock:RockBoundField DataField="Quantity" HeaderText="Quantity" />
                                            <Rock:CurrencyField DataField="UnitPrice" HeaderText="Unit Price" />
                                            <Rock:CurrencyField DataField="UnitDiscount" HeaderText="Unit Discount" />
                                            <Rock:CurrencyField DataField="TotalPrice" HeaderText="Total Price" DataFormatString="Currency" />
                                            <Rock:CurrencyField DataField="TotalDiscount" HeaderText="Total Discount" DataFormatString="Currency" />
                                            <Rock:CurrencyField DataField="TaxAmount" HeaderText="Tax" DataFormatString="Currency" />
                                            <Rock:CurrencyField DataField="TotalAfterTax" HeaderText="Total After Tax" DataFormatString="Currency" />
                                            <Rock:EditField OnClick="gInvoiceItems_RowSelected" />
                                            <Rock:DeleteField OnClick="gInvoiceItem_Delete" />
                                        </Columns>

                                    </Rock:Grid>
                                </div>
                            </div>

                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="panel panel-info">
                            <div class="panel-heading">
                                <h1 class="panel-title">Invoice Summary</h1>
                            </div>
                            <div class="panel-body">
                                <strong>Invoice Item Count: </strong>
                                <asp:Literal ID="litInvoiceItemCount" runat="server" /><br />
                                <strong>Item Subtotal: </strong>
                                <asp:Literal ID="litInvoiceSubtotal" runat="server" />
                                <br />
                                <strong>Discount Total: </strong>
                                <asp:Literal ID="litDiscountTotal" runat="server" />
                                <br />
                                <strong>Invoice Pre-Tax Total:</strong>
                                <asp:Literal ID="litInvoicePreTaxTotal" runat="server" />
                                <br />
                                <strong>Tax: </strong>
                                <asp:Literal ID="litTaxTotal" runat="server" />
                                <br />
                                <strong>Invoice Total: </strong>
                                <asp:Literal ID="litInvoiceFinalTotal" runat="server" />
                                <br />
                            </div>
                            </>

                        </div>
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary"
                        OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link"
                        CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        <Rock:ModalDialog ID="dlgAssignment" runat="server" ScrollbarEnabled="false" ValidationGroup="Assignment"
                SaveButtonText="Add" OnCancelClick="ClearAllModalFields" OnSaveClick="btnSaveAssignment_Click"
                Title="Enter Person and Percent">
                <Content>
                    <asp:ValidationSummary ID="vsAssignment" runat="server" HeaderText="Please correct the following:"
                        CssClass="alert alert-validation" ValidationGroup="Assignment" />
                    <Rock:PersonPicker ID="ppAssignment" runat="server" Required="true" Label="Person" ValidationGroup="Assignment" />
                    <Rock:HighlightLabel ID="hlblCurrentAssignedTotal" runat="server" LabelType="Danger" ValidationGroup="Assignment" />
                    <Rock:NumberBox ID="numbAssignedPercent" runat="server" Label="Percent of Invoice Assigned"
                        Help="What percent of the total invoice is this person responsible for." NumberType="Integer" ValidationGroup="Assignment" Required="true" />

                </Content>
            </Rock:ModalDialog>





            <Rock:ModalDialog ID="dlgInvoiceItem" runat="server" ScrollbarEnabled="false" ValidationGroup="InvoiceItem"
                SaveButtonText="Add" OnCancelClick="ClearAllModalFields" OnSaveClick="btnSaveInvoiceItem_Click"
                Title="Create the Invoice Item">
                <Content>
                    <asp:ValidationSummary ID="vsInvoiceItem" runat="server" HeaderText="Please correct the following:"
                        CssClass="alert alert-validation" ValidationGroup="InvoiceItem" />
                    <Rock:DataTextBox ID="tbItemDescription" runat="server" Label="Description" SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceItem, online.kevinrutledge.InvoiceSystem" PropertyName="Description" Required="true" ValidationGroup="IvoiceItem" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="numbQuantity" runat="server" Label="Quantity" NumberType="Integer" Required="true" ValidationGroup="IvoiceItem" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="numbUnitPrice" runat="server" Label="Unit Price" NumberType="Currency" Required="true" ValidationGroup="IvoiceItem" />
                        </div>
                    </div>
                    <Rock:NumberBox ID="numbTaxPercent" runat="server" Label="Item Tax Rate" NumberType="Double" Help="If this value is left blank, the Tax Rate value on the invoice type will be used for all items." />
                    <div class="row">
                        <div class="col-md-12">
                            <div class="panel panel-success">
                                <div class="panel-heading">
                                    <h1 class="panel-title">Discounts to Apply</h1>
                                </div>
                                <div class="panel-body">
                                    <p>If you want to apply a discount to the item, enter the full price above and then choose either an Amount or Precent to apply.</p>
                                    <div class="row">
                                        <div class="col-md-6">

                                            <Rock:CurrencyBox ID="numbDiscountAmount" runat="server" Label="Discount Amount" NumberType="Currency" />
                                        </div>
                                        <div class="col-md-6">
                                            <Rock:NumberBox ID="numbDiscountPercent" runat="server" Label="Discount Percent" NumberType="Double" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </Content>
            </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
