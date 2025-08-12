<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="NewsToEmailWebApp.Account.Register" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
        <h2>Use the form below to create a new account.</h2>
    </hgroup>
    <p class="validation-summary-errors">
        <asp:Literal runat="server" ID="ErrorMessage" />
    </p>
    <fieldset>
        <legend>Registration Form</legend>
        <ol>
            <li>
                <asp:Label runat="server" AssociatedControlID="UserName">User name</asp:Label>
                <asp:TextBox runat="server" ID="UserName" OnLoad="UserName_Load" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName"
                CssClass="field-validation-error" ErrorMessage="The user name field is required." />
            </li>
            <li>
                <asp:Label runat="server" AssociatedControlID="Email">Email address</asp:Label>
                <asp:TextBox runat="server" ID="Email" TextMode="Email" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Email"
                    CssClass="field-validation-error" ErrorMessage="The email address field is required." />
            </li>
            <li>
                <asp:Label runat="server" AssociatedControlID="Password">Password</asp:Label>                
                <asp:TextBox runat="server" ID="Password" TextMode="Password" ToolTip="Password must be 8 to 12 symbols. At least 1 Upper, 1 lower letter and at least 1 digit."/>
                <p style="color:red; text-wrap:normal">Password must be 8 to 12 symbols. At least 1 Upper, 1 lower letter and at least 1 digit.</p>
            <asp:RequiredFieldValidator ID="RequiredFieldValidatorPassword" CssClass="field-validation-error"  ControlToValidate="Password"
                        runat="server" ErrorMessage="* The Password is invalid" ForeColor='Red' Display="Dynamic" />
                    <asp:CustomValidator ID="CustomValidatorPassword" OnServerValidate="CustomValidatorPassword_ServerValidate"
                        ControlToValidate="Password" runat="server" ErrorMessage="* Password must be 8 to 12 symbols. At least 1 Upper, 1 lower letter and at least 1 digit."
                        ForeColor="Red" Display="Dynamic" ValidateEmptyText="false" />
            </li>
            <li>
                <asp:Label runat="server" AssociatedControlID="ConfirmPassword">Confirm password</asp:Label>
                <asp:TextBox runat="server" ID="ConfirmPassword" TextMode="Password" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidatorConfirmPassword" CssClass="field-validation-error" ControlToValidate="ConfirmPassword"
                    runat="server" ErrorMessage="* Please enter valid Username" ForeColor='Red' Display="Dynamic" />
                <asp:CompareValidator ID="CompareValidatorConfirmPassword" CssClass="field-validation-error" runat="server" ControlToValidate="ConfirmPassword"
                    ControlToCompare="Password" Display="Dynamic" ErrorMessage="* The Password is invalid"
                    ForeColor="Red" />
            </li>
        </ol>
        <asp:Button runat="server" OnClick="RegisterUser_CreatedUser" Text="Register" />
    </fieldset>
</asp:Content>