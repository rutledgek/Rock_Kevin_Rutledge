<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceTypeList.ascx.cs" Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceTypeList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
         <Rock:GridFilter ID="gfSettings" runat="server">
            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category"  EntityTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType" />
            <Rock:RockCheckBox ID="cbShowInActive" runat="server" Label="Show Inactive" Checked="false"  />
        </Rock:GridFilter>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:Grid ID="gInvoiceTypes" runat="server" AllowSorting="true" OnRowSelected="gInvoiceTypes_Edit" TooltipField="Description">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Invoice Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <Rock:BoolField DataField="IsActive" HeaderText="Is Active" SortExpression="IsActe" />
                <Rock:DeleteField OnClick="gInvoiceTypes_Delete" />
                <Rock:SecurityField TitleField="Name" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>