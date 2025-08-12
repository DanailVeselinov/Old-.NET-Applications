<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="NewsToEmailWebApp.Account.Login" %>
<%@ Register Src="~/Account/OpenAuthProviders.ascx" TagPrefix="uc" TagName="OpenAuthProviders" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>
    <section id="loginForm">    
        <h2>Use a local account to log in.</h2>
        <asp:Login runat="server" ViewStateMode="Disabled" RenderOuterTable="false" ClientIDMode="Static" ID="LoginView">
            <LayoutTemplate>
                <p class="validation-summary-errors">
                    <asp:Literal runat="server" ID="FailureText" />
                </p>
                <fieldset>
                    <legend>Log in Form</legend>
                    <ol>
                        <li>
                            <asp:Label runat="server" AssociatedControlID="UserName">User name</asp:Label>
                            <asp:TextBox runat="server" ClientIDMode=Static ID="UserName" OnLoad="UserName_Load"/>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidatorUserName" ControlToValidate="UserName" runat="server" ErrorMessage="* Please enter valid Username" ForeColor='Red' Display="Dynamic"/>
                        </li>
                        <li>
                            <asp:Label runat="server" AssociatedControlID="Password">Password</asp:Label>
                            <asp:TextBox runat="server" ID="Password" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="RequiredFieldValidatorPassword" CssClass="field-validation-error"  ControlToValidate="Password"
                                        runat="server" ErrorMessage="* The Password is invalid" ForeColor='Red' Display="Dynamic" />
                                    <asp:CustomValidator ID="CustomValidatorPassword" OnServerValidate="CustomValidatorPassword_ServerValidate"
                                        ControlToValidate="Password" runat="server" ErrorMessage="* The Password is invalid"
                                        ForeColor="Red" Display="Dynamic" ValidateEmptyText="false" />
                        </li>
                    </ol>                    
                    <asp:LinkButton runat="server" style="display:block; margin-top:-25px;" OnClick="ForgotPassword_Click" CausesValidation="false" ID="ForgotPassowrdLB" Text="I Forgot my password!"></asp:LinkButton>
                    <asp:Button runat="server" OnClick="Login_Click" Text="Log in" />
                </fieldset>
            </LayoutTemplate>
        </asp:Login>
        <p>
            <asp:HyperLink runat="server" ID="RegisterHyperLink" ViewStateMode="Disabled">Register</asp:HyperLink>
            if you don't have an account.
        </p>
    </section>
</asp:Content>
