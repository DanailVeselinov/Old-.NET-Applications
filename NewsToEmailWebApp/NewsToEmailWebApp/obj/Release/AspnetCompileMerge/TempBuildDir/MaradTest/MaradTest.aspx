<%@ Page Title="MaradTest" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MaradTest.aspx.cs" Inherits="NewsToEmailWebApp.MaradTest.MaradTest" %>
<asp:Content ID="MaradTestSelectContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label AssociatedControlID="UserNameTB" runat="server" Text="Потребителско Име" />
    <asp:TextBox ID="UserNameTB" runat="server"/>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="UserNameTB" ErrorMessage="Please enter your name" />
    <div>
        <div class="float-left-inline">
            <asp:Label runat="server" Text="Категория:" AssociatedControlID="categoriesLB" />
            <asp:ListBox ID="categoriesLB" runat="server" SelectionMode="Multiple" />
        </div>
        <div class="float-left-inline">
            <asp:Label runat="server" Text="Ниво:" AssociatedControlID="levelLB"/>
            <asp:ListBox ID="levelLB" runat="server" SelectionMode="Single" />
        </div>
        <div class="float-left-inline">
            <asp:Label runat="server" Text="Тип Тест" AssociatedControlID="TestTypeRBL" />
            <asp:RadioButtonList ID="TestTypeRBL" runat="server" TextAlign="Right" >
                <asp:ListItem Text="Original" />
                <asp:ListItem Text="All" />
                <asp:ListItem Text="Bad and Unviewed Questions" />
                <asp:ListItem Text="Unviewed Questions" />
                <asp:ListItem Text="Bad Questions Only" />
                <asp:ListItem Text="Bad Questions Review" />
            </asp:RadioButtonList>
        </div>
    </div>
    <asp:Button runat="server" Text="Започни Тест" OnClick="StartTest_Click" />
</asp:Content>
