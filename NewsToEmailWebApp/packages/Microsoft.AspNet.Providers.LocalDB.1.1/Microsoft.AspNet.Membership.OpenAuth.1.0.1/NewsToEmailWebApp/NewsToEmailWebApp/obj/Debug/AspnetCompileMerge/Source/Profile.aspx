<%@ Page Language="C#" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Site.Master" CodeBehind="Profile.aspx.cs"
    Inherits="NewsToEmailWebApp.Profile" ViewStateEncryptionMode="Always" %>
   
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
<asp:Table runat="server">
    <asp:TableRow VerticalAlign=Top>
        <asp:TableCell>
            <div>
                <label class="site-title"><%# ViewState["CurrentUserName"] %>'s Profile</label>
                <asp:Panel ID="PanelButtons" runat="server" HorizontalAlign="Center">
                    <a href="UsersList.aspx"><img src="info.jpg" width="40">Help me?</a>
                    <asp:Button CssClass="controlButtons" Width="90px" ID="ButtonUpdate" runat="server" AccessKey="U" Text="Update" OnClick="EditUserButton_Click" />
                    <asp:Button CssClass="controlButtons" Width="90px" ID="ButtonClose" runat="server" AccessKey="C" Text="Close" OnClick="CancelUserButton_Click" />
                </asp:Panel>
            </div>
            <div class="float-left">
                <div class="float-left-inline" id="CreateUserTable" runat="server">
                    <asp:Literal runat="server" ID="ErrorMessage" />    
                    <label>User Name</label>
                    <asp:TextBox runat="server" ID="UserName" ></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorUser" ValidationGroup="UserNameValidationGroup" ControlToValidate="UserName"
                        runat="server" ErrorMessage="* Please enter valid Username" ForeColor='Red' Display="Dynamic" />
                    <label>E-mail</label>
                    <asp:TextBox runat="server" ID="Email" TextMode="Email"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorEmail" ValidationGroup="EmailValidationGroup" ControlToValidate="Email"
                        runat="server" ErrorMessage="* Please enter valid Username" ForeColor='Red' Display="Dynamic" />
                    <label>Change Password</label>
                    <asp:TextBox runat="server" ID="Password" TextMode="Password" ></asp:TextBox>
                    <asp:CustomValidator ID="CustomValidatorPassword" ControlToValidate="Password" runat="server" ErrorMessage="* The Password is invalid"
                    ForeColor="Red" Display="Dynamic" ValidateEmptyText="false" />
                    <label>Confirm Password</label>
                    <asp:TextBox runat="server" ClientIDMode="Static" ID="ConfirmPassword" TextMode="Password"></asp:TextBox>
                    <asp:CompareValidator ID="CompareValidatorConfirmPassword" CssClass="field-validation-error" runat="server" ControlToValidate="ConfirmPassword"
                ControlToCompare="Password" Display="Dynamic" ErrorMessage="* The Password is invalid"
                ForeColor="Red" />
                </div>-
                <asp:Panel CssClass="float-left-inline" ID="Panel1" runat="server" Font-Size="Large"
                    HorizontalAlign="Center">
                    <asp:CheckBox CssClass="label-checkbox" ID="IsActiveCB" runat="server" TextAlign="Left" Text="STOP Recieving"/><img ID="Image1" src="Images/info.png" title="Check it if you want to stop recieving emails." onclick="alert(this.title)" Height="25"/><br />
                    <asp:CheckBox CssClass="label-checkbox" ID="ConvertToLatinCB" runat="server" TextAlign="Left" Text="Convert Cyrlic to Latin" /><asp:Image ID="Image2" runat="server" ImageUrl="Images/info.png" Height="25" ImageAlign="Middle" ToolTip="Check this box if you want to convert cyrlic letters to latin. This is usefull when your e-mail provider is not using UTF-8 encoding."/>
                    <label class="label-checkbox">Send me News Every:</label>
                    <asp:DropDownList ID="SendNewsHourDDL" runat="server" >
                        <asp:ListItem>1</asp:ListItem>
                        <asp:ListItem>2</asp:ListItem>
                        <asp:ListItem>3</asp:ListItem>
                        <asp:ListItem>6</asp:ListItem>
                        <asp:ListItem>12</asp:ListItem>
                        <asp:ListItem>24</asp:ListItem>
                        <asp:ListItem>48</asp:ListItem>
                        <asp:ListItem>72</asp:ListItem>
                        <asp:ListItem>168</asp:ListItem>
                    </asp:DropDownList><span>Hours</span><asp:Image ID="Image3" runat="server" ImageUrl="Images/info.png" Height="25" ImageAlign="Middle" ToolTip="Select the period of hours to recieve your news."/>
                    <label class="label-checkbox">E-mail Limit (KB)"</label>
                    <asp:TextBox runat="server" ID="limitTB" TextMode="Number"></asp:TextBox><asp:Image ID="Image4" runat="server" ImageUrl="Images/info.png" Height="25" ImageAlign="Middle" ToolTip="Enter the maximum size of an e-mail. Some e-mail providers has less limit of an e-mail. So if you don't recieve news it's probbably the reason why. Just enter some limit eg. 200KB"/><br />
                    <asp:RangeValidator ID="SizeLimitRangeValidator" runat="server" MinimumValue="0" MaximumValue="50000" ControlToValidate="limitTB"
                        ErrorMessage="Enter Valid Size in KB" ForeColor="Red"></asp:RangeValidator>
                </asp:Panel>
                <div class="float-left-inline">
                    <asp:Panel ID="AddNewWebSitePanel" Visible="false" runat="server">
                        <asp:TextBox ID="AddNewWebSiteTB" runat="server" Width="150px"></asp:TextBox><asp:Button ID="Button2"
                            runat="server" Text="Add New Website" OnClick="AddWS_Click" />
                    </asp:Panel>
                    <asp:ListView ID="WebSitesListView" runat="server" OnSelectedIndexChanged="WebSitesListView_SelectedIndexChanged" OnSelectedIndexChanging=WebSitesListView_SelectedIndexChanging>
                        <ItemTemplate>
                            <td>                                    
                                <asp:Panel ID="FullSlot" runat="server">                                        
                                    <asp:LinkButton ID="SelectButton" runat="server" CommandName="Select">
                                    <asp:Label ID="Label2" runat="server" Text="Web Site Slot"></asp:Label><br />
                                    </asp:LinkButton>
                                    <asp:DropDownList ID="DropDownListSelectWebSite" AutoPostBack="true" OnSelectedIndexChanged="DropDownListSelectWebSite_SelectedIndexChanged"
                                        OnDataBound="DropDownListSelectWebSite_DataBound" OnInit="WebSitesSelect_Init"
                                        DataTextField="siteName" DataValueField="id" runat="server">
                                    </asp:DropDownList>
                                    <br />
                                </asp:Panel>
                            </td>
                        </ItemTemplate>
                        <LayoutTemplate>
                            <table border="1">
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
                    <asp:LinkButton ID="AddNewsPaper" Visible="false" runat="server" Text="Add Newspaper" OnClick="AddNewSlot_Click"></asp:LinkButton>
                    <asp:LinkButton ID="DeleteNewsPaper" Visible="false" runat="server" Text="Delete Newspaper" OnClick=DeleteNewsPaper_Click></asp:LinkButton>
                </div>
                <div class="float-left-inline">
                    <asp:Panel ID="FiltersPanel" Visible="false" Width="200px" HorizontalAlign="Center"
                        runat="server">
                        <h3>
                            <asp:Label ID="WSNameHeader" runat="server"></asp:Label>'s Categories</h3>
                        <asp:DropDownList ID="CategoriesDDL" runat="server" Width="100%" DataMember="Categories"
                            DataTextField="CategoryName" AutoPostBack="true" DataValueField="id" OnSelectedIndexChanged="CategoriesDDL_SelectedIndexChanged">
                        </asp:DropDownList>
                        <h3>Filter Text</h3>
                        <asp:TextBox ID="FilterTextTB" runat="server" Width="100%"></asp:TextBox>
                        <h3>Filter Max symbols</h3>
                        <asp:TextBox ID="FilterSizeTB" TextMode="Number" Width="100%" ToolTip="1~10000" runat="server"></asp:TextBox>
                        <br />
                        <asp:RangeValidator ID="RangeValidator1" runat="server" MaximumValue="10000" ValidationGroup="FilterValidationGroup"
                            MinimumValue="0" Type="Integer" EnableClientScript="true" ErrorMessage="Enter value 1~10 000"
                            ForeColor="Red" ControlToValidate="FilterSizeTB"></asp:RangeValidator><br />
                        <asp:Button ID="Button3" align="center" runat="server" OnClick="AddFilter_Click" Text="Add Filter" />
                        <h3>Category's Filters List</h3>
                        <asp:DataGrid ID="FiltersGrid" runat="server" Width="100%" DataKeyField="id" OnDeleteCommand="FiltersGrid_DeleteCommand"
                            AutoGenerateColumns="false">
                            <Columns>
                                <asp:BoundColumn DataField="id" Visible="false"></asp:BoundColumn>
                                <asp:BoundColumn DataField="filterText" HeaderText="Text"></asp:BoundColumn>
                                <asp:BoundColumn DataField="symbolsCount" HeaderText="Lenght"></asp:BoundColumn>
                                <asp:ButtonColumn ButtonType="LinkButton" Text="Del" CommandName="Delete"></asp:ButtonColumn>
                            </Columns>
                        </asp:DataGrid>
                    </asp:Panel>
                </div>
            </div>
        </asp:TableCell>
        
        <asp:TableCell>
            <div>
                <label class="site-title">Livescores</label>                    
                <asp:Button ID="ButtonLivescores" OnClick="ButtonLivescores_Click" align="center" runat=server Text="Add Livescores" />
                <asp:Panel ID="Panel3" runat="server" HorizontalAlign="Center" Width="250px"
                    ScrollBars="Auto">
                    <asp:TreeView EnableViewState="true" ID="LiveScoresTV" OnInit="LiveScoresTV_Init" runat="server" ShowCheckBoxes="None" NodeWrap="False" PopulateNodesFromClient="False"
                        AutoGenerateDataBindings="False" ParentNodeStyle-CssClass="livescore-tree">
                    </asp:TreeView>
                </asp:Panel>
            </div>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
</asp:Content>
