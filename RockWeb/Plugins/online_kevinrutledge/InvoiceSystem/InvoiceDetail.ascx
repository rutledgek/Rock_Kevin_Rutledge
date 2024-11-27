<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceDetail.ascx.cs"
    Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceDetail" %>
    <asp:UpdatePanel ID="upnlContent" runat="server">
        <ContentTemplate>



            <div>
                <!-- Invoice Details Section -->
                <h2>Create/Edit Invoice</h2>


                <div class="row">
                    <div class="col-md-6">
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
                                <Rock:NumberBox ID="numbLateDays" runat="server" Label="Number of Days After Due Date"
                                    Help="This is the number of days after the Due Date the invoice should be considered late.  This will be used to calculate the due date." />

                            </div>
                        </div>
                    </div>


                    <div class="col-md-6">

                       
                        <div class="row">
                            <div class="col-md-12">

 <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Invoice Assignments</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                               <Rock:Grid ID="gAssignments" runat="server" DisplayType="Light" RowItemText="Assignment" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="PersonAliasName" HeaderText="Person" />
                                        <Rock:RockBoundField DataField="AssignedPercent" HeaderText="Percent Assigned" />
                                        <Rock:DeleteField OnClick="gAssignment_Delete" />
                                    </Columns>
                                </Rock:Grid>
                                    </div>
                            </div>
     </div>
                        </div>
                    </div>
                </div>
                <asp:HiddenField ID="hfActiveDialog" runat="server" />
                <Rock:ModalDialog ID="dlgAssignment" runat="server" ScrollbarEnabled="false"
                    ValidationGroup="Assignment" SaveButtonText="Add" OnCancelScript="clearActiveDialog();"
                    OnSaveClick="btnSaveAssignment_Click" Title="Enter Person and Percent">
                    <Content>
                        <asp:ValidationSummary ID="vsAssignment" runat="server"
                            HeaderText="Please correct the following:" CssClass="alert alert-validation"
                            ValidationGroup="Assignment" />
                        <Rock:PersonPicker ID="ppAssignment" runat="server" Required="true" Label="Person" />
                        <Rock:NumberBox ID="numbAssignedPercent" runat="server" Label="Percent of Invoice Assigned"
                            Help="What percent of the total invoice is this person responsible for." />

                    </Content>
                </Rock:ModalDialog>
        </ContentTemplate>
    </asp:UpdatePanel>