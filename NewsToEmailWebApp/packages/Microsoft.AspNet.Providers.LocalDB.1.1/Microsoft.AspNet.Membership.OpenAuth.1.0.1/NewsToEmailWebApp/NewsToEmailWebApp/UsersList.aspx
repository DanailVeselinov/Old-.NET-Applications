<%@ Page Language="C#" Title="Users List" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Site.Master" CodeBehind="UsersList.aspx.cs" Inherits="NewsToEmailWebApp.UsersList" ViewStateEncryptionMode="Always" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <meta name="robots" content="noindex,nofollow" />
    <asp:ListView ID="ListUsers" runat="server">
        <ItemTemplate>
            <div><a href="Profile.aspx?id=<%# Eval("id") %>"><%# Eval("id") %>. <%# Eval("userName") %></a></div>            
        </ItemTemplate>
    </asp:ListView>
</asp:Content>