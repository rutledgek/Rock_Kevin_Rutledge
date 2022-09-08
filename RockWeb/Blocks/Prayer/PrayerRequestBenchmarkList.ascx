<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestBenchmarkList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestBenchmarkList" %>

<asp:UpdatePanel ID="upnlPrayerRequestList" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i> Prayer Requests Benchmark List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gPrayerRequests" runat="server" RowItemText="Prayer Request" AllowSorting="true" OnGridRebind="gPrayerRequests_GridRebind">
                        <Columns>
                            <Rock:RockTemplateField HeaderText="Name" SortExpression="LastName,FirstName">
                                <ItemTemplate>
                                    <%# Eval("FirstName") %> <%# Eval("LastName") %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <Rock:DateTimeField DataField="EnteredDateTime" HeaderText="Entered On" SortExpression="EnteredDateTime" />
                            <Rock:DateTimeField DataField="ExpirationDate" HeaderText="Expires" SortExpression="ExpirationDate" />
                            <Rock:BoolField DataField="IsUrgent" HeaderText="Urgent" SortExpression="IsUrgent" />
                            <Rock:BoolField DataField="IsPublic" HeaderText="IsPublic" SortExpression="IsPublic" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
