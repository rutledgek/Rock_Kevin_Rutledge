<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceList.ascx.cs" Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceTypeList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    
    <ContentTemplate>
        <asp:HiddenField ID="hfInvoiceTypeIds" runat="server" />
         <Rock:GridFilter ID="gfSettings" runat="server">
             <Rock:RockCheckBoxList ID="cblInvoiceTypes" runat="server" Label="Invoice Type" DataTextField="Text" DataValueField="Value" />
             <Rock:RockCheckBoxList ID="cblInvoiceStatuses" runat="server" Label="Invoice Status" DataTextField="Text" DataValueField="Value"/>
        </Rock:GridFilter>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:Grid ID="gInvoices" runat="server" AllowSorting="true" OnRowSelected="gInvoiceTypes_Edit" TooltipField="Description">
            <Columns>
                <asp:BoundField DataField="InvoiceStatus" HeaderText="Invoice Status" SortExpression="InvoiceStatus" />
                <asp:BoundField DataField="Name" HeaderText="Invoice Name" SortExpression="Name" />
                <Rock:DateField DataField="DueDate" HeaderText="Invoice Due Date" SortExpression="DueDate" />
                <Rock:DateField DataField="LateDate" HeaderText="Invoice Late Date" SortExpression="LateDate" />
                
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>