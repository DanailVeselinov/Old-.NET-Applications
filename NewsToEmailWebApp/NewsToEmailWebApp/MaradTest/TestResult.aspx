<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TestResult.aspx.cs" Inherits="NewsToEmailWebApp.MaradTest.TestResult" %>
<asp:Content ID="TestResultContent" ContentPlaceHolderID="MainContent" runat="server">
        <asp:Label runat="server" ID="message" Text="Congragulations your result is perrfect" Font-Size="Larger"/>
        <asp:Panel ID="wrongPanel" runat="server" Visible="false">
            <asp:Label runat="server" Text="Would you like to review wrong questions." />
            <asp:Button runat="server" Text="Review" OnClick="Review_Click" style="display:inline-block"/>
            <asp:Button runat="server" Text="Back to main" OnClick="BackToMain_Click" style="display:inline-block" />
        </asp:Panel>
        <asp:Panel ID="perfectPanel" runat="server" Visible ="false">
            <asp:Button runat="server" Text="Back to Main" OnClick="BackToMain_Click"/>
        </asp:Panel>
</asp:Content>
