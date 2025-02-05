<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledInvoiceList.ascx.cs" Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.ScheduledInvoiceList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    
    <ContentTemplate>
        <asp:HiddenField ID="hfInvoiceTypeIds" runat="server" />
         <Rock:GridFilter ID="gfSettings" runat="server">
             <Rock:RockCheckBoxList ID="cblInvoiceTypes" runat="server" Label="Invoice Type" DataTextField="Text" DataValueField="Value" />
             <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
        </Rock:GridFilter>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:Grid ID="gScheduledInvoices" runat="server" AllowSorting="true" OnRowSelected="gScheduledInvoices_Edit" TooltipField="Description">
            <Columns>

                <asp:BoundField DataField="Name" HeaderText="Invoice Name" SortExpression="Name" />
                <asp:BoundField DataField="Schedule" HeaderText="Schedule" SortExpression="Schedule" />
                <Rock:BoolField DataField="IsActive" HeaderText="Is Active" SortExpression="IsActive" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>