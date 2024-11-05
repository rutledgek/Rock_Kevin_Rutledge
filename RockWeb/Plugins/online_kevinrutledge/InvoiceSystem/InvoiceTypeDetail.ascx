<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InvoiceTypeDetail.ascx.cs" Inherits="RockWeb.Plugins.online_kevinrutledge.InvoiceSystem.InvoiceTypeDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfInvoiceTypeId" runat="server" />

            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />


            <div class="row">
                <div class="col-md-6">
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="online.kevinrutledge.InvoiceSystem.Model.InvoiceType, online.kevinrutledge.InvoiceSystem" PropertyName="Name" />

                </div>
                <div class="col-md-6">
                </div>
            </div>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>