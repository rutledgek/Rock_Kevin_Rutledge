<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceDetail.ascx.cs" Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceDetail" %>
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
                        <Rock:DataTextBox ID="tbInvoiceName" runat="server" SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.Invoice, online.kevinrutledge.InvoiceSystem" PropertyName="Name" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbSummary" runat="server" SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.Invoice, online.kevinrutledge.InvoiceSystem" PropertyName="Summary" TextMode="MultiLine" Rows="4"/>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:DatePicker ID="dpDueDate" runat="server" Label="Due Date" />
                    </div>
                    <div class="col-md-4">
                        <Rock:DatePicker ID="dpLateDate" runat="server" Label="Late Date" Help="The Date the invoice will be considered late.  Alternatively, you can select a number of days after the due date." />
                    </div>
                    <div class="col-md-4">
                        <Rock:NumberBox ID="numbLateDays" runat="server" Label="Number of Days After Due Date" Help="This is the number of days after the Due Date the invoice should be considered late.  This will be used to calculate the due date."/>
        
                    </div>
                </div>
        </div>
    

        <div class="col-md-6">

    <h3>Invoice Assigned To:</h3>
   <Rock:Grid ID="gInvoiceAssignments" runat="server" AllowSorting="true" ShowActionRow="true" ShowDeleteField="true" TooltipField="Tooltip">
    <Columns>
        
        <asp:BoundField DataField="AuthorizedPerson" HeaderText="Authorized Person" SortExpression="AuthorizedPerson" />

        
        <asp:BoundField DataField="AssignedAmount" HeaderText="Assigned Amount" SortExpression="AssignedAmount" DataFormatString="{0:C}" />

        
        

        
        <asp:TemplateField>
            <HeaderTemplate>
                Actions
            </HeaderTemplate>
            <ItemTemplate>
               
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</Rock:Grid>
        </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
