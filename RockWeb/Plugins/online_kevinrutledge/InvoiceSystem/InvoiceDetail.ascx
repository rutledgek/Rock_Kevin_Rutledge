<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceDetail.ascx.cs"
    Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceDetail" %>
    <asp:UpdatePanel ID="upnlContent" runat="server">
        <ContentTemplate>
                            

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
                    <h1 class="panel-title">Invoice</h1>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-8">
                            <div class="panel panel-block">
                                <div class="panel-heading">
                                    <h1 class="panel-title">Invoice Details</h1>
                                </div>
                                <!-- Invoice Details Section -->
                                <div class="panel-body">

                                    <div class="row">

                                        <div class="col-md-12">
                                            <p>Invoice Id: #24<br />
                                                Invoice Status: #24</p>
                                            <Rock:DataTextBox ID="tbInvoiceName" runat="server"
                                                SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.Invoice, online.kevinrutledge.InvoiceSystem"
                                                PropertyName="Name" />
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-md-12">
                                            <Rock:DataTextBox ID="tbSummary" runat="server"
                                                SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.Invoice, online.kevinrutledge.InvoiceSystem"
                                                PropertyName="Summary" TextMode="MultiLine" Rows="4" />
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col-md-4">
                                            <Rock:DatePicker ID="dpDueDate" runat="server" Label="Due Date" />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:DatePicker ID="dpLateDate" runat="server" Label="Late Date"
                                                Help="The Date the invoice will be considered late.  Alternatively, you can select a number of days after the due date." />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:NumberBox ID="numbLateDays" runat="server"
                                                Label="Number of Days After Due Date"
                                                Help="This is the number of days after the Due Date the invoice should be considered late.  This will be used to calculate the due date." />

                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-4">


                            <div class="row">
                                <div class="col-md-12">

                                    <div class="panel panel-block">
                                        <div class="panel-heading">
                                            <h1 class="panel-title">Invoice Assignments</h1>
                                            <Rock:HighlightLabel ID="hlblCurrentAssignedTotalGridView" runat="server"
                                                LabelType="Danger" />
                                        </div>
                                        <div class="panel-body">

                                            <div class="grid grid-panel">
                                                <Rock:Grid ID="gAssignments" runat="server" DisplayType="Light"
                                                    RowItemText="Assignment" ShowConfirmDeleteDialog="false"
                                                    OnRowSelected="gAssignments_RowSelected" DataKeyNames="Guid">
                                                    <Columns>
                                                        <Rock:RockBoundField DataField="PersonAliasName"
                                                            HeaderText="Person" />
                                                        <Rock:RockBoundField DataField="AssignedPercent"
                                                            HeaderText="Percent Assigned" />
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
                </div>
            </div>
            <asp:HiddenField ID="hfActiveDialog" runat="server" />
            <asp:HiddenField ID="hfAssignmentGuid" runat="server" />
            <Rock:ModalDialog ID="dlgAssignment" runat="server" ScrollbarEnabled="false" ValidationGroup="Assignment"
                SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAssignment_Click"
                Title="Enter Person and Percent">
                <Content>
                    <asp:ValidationSummary ID="vsAssignment" runat="server" HeaderText="Please correct the following:"
                        CssClass="alert alert-validation" ValidationGroup="Assignment" />
                    <Rock:PersonPicker ID="ppAssignment" runat="server" Required="true" Label="Person" />
                    <Rock:HighlightLabel ID="hlblCurrentAssignedTotal" runat="server" LabelType="Danger" />
                    <Rock:NumberBox ID="numbAssignedPercent" runat="server" Label="Percent of Invoice Assigned"
                        Help="What percent of the total invoice is this person responsible for." />

                </Content>
            </Rock:ModalDialog>
        </ContentTemplate>
    </asp:UpdatePanel>