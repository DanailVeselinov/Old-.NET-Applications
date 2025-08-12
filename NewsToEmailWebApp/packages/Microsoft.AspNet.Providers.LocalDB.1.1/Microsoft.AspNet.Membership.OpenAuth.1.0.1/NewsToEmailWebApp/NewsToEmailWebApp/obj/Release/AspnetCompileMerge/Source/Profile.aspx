<%@ Page Language="C#" Title="Profile" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Site.Master" CodeBehind="Profile.aspx.cs"
    Inherits="NewsToEmailWebApp.Profile" ViewStateEncryptionMode="Always" %>
   
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
        <div style="display:inline-block; float:left; vertical-align:top; max-width:75%;">
            <div>
                <label class="site-title" style="text-align: center; word-wrap:break-word"><%# ViewState["CurrentUserName"] %>'s Profile</label>
                <asp:Panel ID="PanelButtons" runat="server" HorizontalAlign="Center">                    
                    <asp:Button CssClass="controlButtons" Width="90px" ID="ButtonUpdate" runat="server" AccessKey="U" Text="Update" OnClick="EditUserButton_Click" />
                    <asp:Button CssClass="controlButtons" Width="90px" ID="ButtonClose" runat="server" AccessKey="C" Text="Close" OnClick="CancelUserButton_Click" />
                </asp:Panel>
            </div>
            <div id="CreateUserTable" runat="server">
                <asp:Literal runat="server" ID="ErrorMessage" />                                    
                <span class="help-me">This checkbox allows you to change your password. Make shure you click Update button after entering correctly the new password in both Password and Confirm Password text fields.</span>
                <div>
                    <input type="checkbox" id="ChangePasswordCB" class="label-checkbox" onchange='CheckHideUnhideClass("passwordDiv")'/>
                    <label style="display:inline" for="ChangePasswordCB">Change Password</label>
                </div>
                <span class="help-me">This is your User details panel Here you can change your "User Name" "E-mail" and "Password". Just make shure you click the "Update" Button to accept changes.</span>
                <div style="display:inline-block;">
                    <label>User Name</label>
                    <asp:TextBox runat="server" ID="UserName" ToolTip="Enter Username. Only Letters and Digits allowed."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorUser" ValidationGroup="UserNameValidationGroup" ControlToValidate="UserName"
                        runat="server" ErrorMessage="* Please enter valid Username. Only Letters and Digits allowed." ForeColor='Red' Display="Dynamic" />
                </div>
                <div style="display:inline-block;">
                    <label>E-mail</label>
                    <asp:TextBox runat="server" ID="Email" TextMode="Email" ToolTip="Enter E-mail."></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorEmail" ValidationGroup="EmailValidationGroup" ControlToValidate="Email"
                        runat="server" ErrorMessage="* Please enter valid E-mail." ForeColor='Red' Display="Dynamic" />
                </div>
                <div id="ChangePasswordPanel" class="passwordDiv">
                    <div style="display:inline-block;">
                        <label>Change Password</label>
                        <asp:TextBox runat="server" ID="Password" TextMode="Password" ToolTip="Password must be 8 to 12 symbols. At least 1 Upper, 1 lower letter and at least 1 digit."></asp:TextBox>
                        <asp:CustomValidator ID="CustomValidatorPassword" ControlToValidate="Password" runat="server" ErrorMessage="* Password must be 8 to 12 symbols. At least 1 Upper, 1 lower letter and at least 1 digit."
                        ForeColor="Red" Display="Dynamic" ValidateEmptyText="false" />
                    </div>
                    <div style="display:inline-block;">
                        <label>Confirm Password</label>
                        <asp:TextBox runat="server" ClientIDMode="Static" ID="ConfirmPassword" TextMode="Password" ToolTip="Please Re-Type Password for security matter."></asp:TextBox>
                        <asp:CompareValidator ID="CompareValidatorConfirmPassword" CssClass="field-validation-error" runat="server" ControlToValidate="ConfirmPassword"
                        ControlToCompare="Password" Display="Dynamic" ErrorMessage="* Password and Confirm Password fields must match." ForeColor="Red" />
                    </div>
                </div>
            </div>
                <span class="help-me">This is your settings panel Here you can adjust the Frequency, Type, Limit etc.. Just make shure you click the "Update" Button to accept changes. For more information click the i icon.</span>
                <asp:Panel style="display:inline-block;" ID="Panel1" runat="server" Font-Size="Large" HorizontalAlign="Center">
                    <asp:CheckBox CssClass="label-checkbox" ID="IsActiveCB" runat="server" TextAlign="Left" Text="STOP Recieving" ToolTip="Check it if you want to stop recieving e-mails."/><img src="Images/info.png" title="Check it if you want to stop recieving e-mails." onclick="alert(this.title)" Height="20"/><br />
                    <asp:CheckBox CssClass="label-checkbox"  ID="RecieveFileCB" runat="server" TextAlign="Left" Text="Recieve As Attachment" ToolTip="Check it if you want to recieve File Attachemt."/><img src="Images/info.png" title="Check it if you want to recieve File Attachemt." onclick="alert(this.title)" Height="20"/><br />                    
                    <asp:CheckBox CssClass="label-checkbox" ID="ConvertToLatinCB" runat="server" TextAlign="Left" Text="Convert Cyrlic to Latin" ToolTip="Check this box if you want to convert cyrlic letters to latin. This is usefull when your e-mail provider is not using UTF-8 encoding. If you have this problem with your provider You can select 'Recieve as Attachment' Option." />
                    <img src="Images/info.png" title="Check this box if you want to convert cyrlic letters to latin. This is usefull when your e-mail provider is not using UTF-8 encoding. If you have this problem with your provider You can select 'Recieve as Attachment' Option." onclick="alert(this.title)" Height="20"/>
                    <label class="label-checkbox">Send me News Every:</label>
                    <asp:DropDownList ID="SendNewsHourDDL" runat="server" ToolTip="Select the period of hours to recieve your news.">
                        <asp:ListItem>3</asp:ListItem>
                        <asp:ListItem>6</asp:ListItem>
                        <asp:ListItem>12</asp:ListItem>
                        <asp:ListItem>24</asp:ListItem>
                        <asp:ListItem>48</asp:ListItem>
                        <asp:ListItem>72</asp:ListItem>
                        <asp:ListItem>168</asp:ListItem>
                    </asp:DropDownList><span>Hours</span><img src="Images/info.png" onclick="alert(this.title)" Height="20" title="Select the period of hours to recieve your news."/>
                    <label class="label-checkbox">E-mail Limit (KB)</label>
                    <asp:TextBox runat="server" Width="80%" ID="limitTB" TextMode="Number" ToolTip="Enter the maximum size of an e-mail. Some e-mail providers has less limit of an e-mail. So if you don't recieve news it's probbably the reason why. Just enter some limit eg. 200KB"></asp:TextBox>
                    <img src="Images/info.png" onclick="alert(this.title)" Height="20" title="Enter the maximum size of an e-mail. Some e-mail providers has less limit of an e-mail. So if you don't recieve news it's probbably the reason why. Just enter some limit eg. 200KB"/><br />
                    <asp:RangeValidator ID="SizeLimitRangeValidator" runat="server" MinimumValue="0" MaximumValue="50000" ControlToValidate="limitTB"
                        ErrorMessage="Enter Valid Size in KB" ForeColor="Red"></asp:RangeValidator>
                </asp:Panel>
                <div style="display:inline-block;">
                    <asp:Panel ID="AddNewWebSitePanel" Visible="false" runat="server">
                        <span class="help-me">Enter your favourite news Web Site domain name. Ex: www.sportal.bg. Or select it from the List below. Make sure you hit the "Add New Website" button.</span>    
                        <asp:TextBox ID="AddNewWebSiteTB" runat="server" Width="120px" ToolTip="Enter your favourite news Web Site domain name. Ex: www.sportal.bg. Or select it from the List below."></asp:TextBox>
                        <asp:Button ID="Button2" runat="server" Text="Add New Website" OnClick="AddWS_Click" />
                    </asp:Panel>
                        <span class="help-me">NEWSPAPERS Section is the panel where you can add, delete or edit your NEWSPAPER Slots.<br />Each NEWSPAPER Slot can contain single Web Site, "Edit" button and expire date. So if you need to add another Slot click the "AddNewsPaper" Link Below.</br>This is free version so The slots number is limited if you need to add third or more slots please contact us.</span>
                    <asp:ListView ID="WebSitesListView" runat="server" OnSelectedIndexChanged="WebSitesListView_SelectedIndexChanged" OnSelectedIndexChanging=WebSitesListView_SelectedIndexChanging>
                        <ItemTemplate>                            
                            <td style="padding-bottom:10px;" title="This is a web-site slot, where you can select or add the desired news web-site. You can have more than one slot respectively recieve news from more than one web-site just contact us.">                                    
                                <asp:Panel ID="FullSlot" runat="server" HorizontalAlign="Center" style="border:solid; border-width:5px; border-color:lightslategrey; border-radius:12px; padding:10px;">                                        
                                    <asp:Label runat="server" CssClass="site-title" Font-Size="Large" Text="NEWSPAPER SLOT"></asp:Label><br />
                                    <span class="help-me">Select your Web Site here. <br />**Or if you don't find it just select "Add New WebSite"</span>
                                    <asp:DropDownList ID="DropDownListSelectWebSite" AutoPostBack="true" OnSelectedIndexChanged="DropDownListSelectWebSite_SelectedIndexChanged"
                                        OnDataBound="DropDownListSelectWebSite_DataBound" OnInit="WebSitesSelect_Init"
                                        DataTextField="siteName" DataValueField="id" runat="server">
                                    </asp:DropDownList><br />
                                    <span class="help-me">Use "Edit Filters" button to transfer to the page where you can filter the news by Category and text, also you can reduce the symbols count of the filtered news.<br />All thease options you can access using this button.</span>
                                    <asp:Button ID="ButtonEditFilters" CommandName="Select" align="center" runat=server Text="Edit Filters" />
                                    <span class="help-me">This is the expire date label. If your web-site expires just contact us.</span>
                                    <asp:Label ID="ExpDate" style="display:block; margin-bottom:3px" runat="server">Exp:<%#(GetDataItem() as NewsToEmailWebApp.UsersWebsites).validUntil.ToShortDateString()%></asp:Label>
                                </asp:Panel>
                            </td>
                            <td><img src="Images/info.png" onclick="alert(this.title)" Height="20" title="This is a web-site slot, where you can select or add the desired news web-site. You can have more than one slot respectively recieve news from more than one web-site just contact us."/></td>
                        </ItemTemplate>
                        <LayoutTemplate>
                            <table>
                                <tr id="groupPlaceholder" runat="server">
                                </tr>
                            </table>
                        </LayoutTemplate>
                        <GroupTemplate>
                            <tr>
                                <td id="itemPlaceholder" runat="server" onmouseup="Select"></td>
                            </tr>
                        </GroupTemplate>
                    </asp:ListView>
                    <br />
                    <asp:LinkButton ID="AddNewsPaper" runat="server" Text="Add_Newspaper" OnClick="AddNewSlot_Click"></asp:LinkButton>
                    <asp:LinkButton ID="DeleteNewsPaper" runat="server" Text="Delete_Newspaper" OnClick=DeleteNewsPaper_Click></asp:LinkButton>
                </div>                
            </div>
            <div style="display:inline-block; float:left; vertical-align:top; max-width:25%;">
                <label class="site-title">Livescores</label>                    
                <span class="help-me">LIVESCORES Section gives you information about currently selected livescores. If you like to add or edit your livescores selection just click the "Add Livescores" button.</span>
                <asp:Button ID="ButtonLivescores" OnClick="ButtonLivescores_Click" align="center" runat=server Text="Add Livescores" />
                <asp:Panel ID="Panel3" runat="server" HorizontalAlign="Center" Width="250px"
                    ScrollBars="Auto">
                    <asp:TreeView EnableViewState="true" ID="LiveScoresTV" OnInit="LiveScoresTV_Init" runat="server" ShowCheckBoxes="None" NodeWrap="False" PopulateNodesFromClient="False"
                        AutoGenerateDataBindings="False" ParentNodeStyle-CssClass="livescore-tree">
                    </asp:TreeView>
                </asp:Panel>
            </div>
</asp:Content>
