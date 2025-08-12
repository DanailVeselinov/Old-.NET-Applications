<%@ Page Title="Test" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="NewsToEmailWebApp.MaradTest.Test" %>
<asp:Content ID="TestContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel runat="server" ID="UpdateQuestionPanel" OnLoad="UpdateQuestionPanel_Load" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Label runat="server" ID="qIdLabel" Visible="false" style="display:block" />
            <asp:Label runat="server" ID="statusLabel" style="display:block"/>
            <asp:Label runat="server" AssociatedControlID="questionLabel" Text="Въпрос:" style="display:block"/>
            <asp:Label runat="server" ID="questionLabel"  style="display:block" Font-Size="Large"/>
            <asp:Image ID="Correct" runat="server" ImageUrl="~/Images/info.png" onclick="alert(this.alt)" Height="20"/>    
            <asp:Image runat="server" ID="Image" OnLoad="Image_Load" style="display:block"/>
            <asp:Label runat="server" AssociatedControlID="AnswersRBL" Text="Отговор:" style="display:block"/>            
            <asp:RadioButtonList ID="AnswersRBL" runat="server" OnDataBound="AnswersRBL_DataBound" AutoPostBack="true" style="display:block"/>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:Button runat="server" Text="Следващ" OnClick="nextBtn_Click" ID="nextBtn" />
    <asp:Button runat="server" Text="Край" ID="finishBtn" OnClick="finishBtn_Click"/>
</asp:Content>
