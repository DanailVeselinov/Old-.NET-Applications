<%@ Page Title="Edit Filters" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Edit-Filters.aspx.cs" Inherits="NewsToEmailWebApp.EditFilters" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="EditFilters" ContentPlaceHolderID="MainContent" runat="server">
    <asp:EntityDataSource ID="EntityDataSourceFilters" runat="server" EnableDelete="True" EnableInsert="True" EnableUpdate="True" AutoGenerateWhereClause="True" Where="" ConnectionString="name=NewsToEmailDBEntities" DefaultContainerName="NewsToEmailDBEntities" EnableFlattening="False" EntitySetName="FiltersCategories" EntityTypeFilter="FiltersCategories">
        <WhereParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="userId" QueryStringField="id" Type="Int32" />
            <asp:ControlParameter ControlID="CategoriesListView" PropertyName="SelectedDataKey.Value" Type="Int32" Name="categoryId" />
        </WhereParameters>
    </asp:EntityDataSource>
    <div>
        <div>
            <label class="site-title"><%: Title%></label>        
            <asp:Button ID="ButtonBack" Text="Back To Profile" OnClick="Button_Cancel_Click" runat=server />
        </div>
        <span class="help-me">You have to know that average news message size is about 400kb. Which is a not so cheep if you use Satelite communications. <br /> Also not all of the information that a Web site provides is desired. That's why we tried to make possible to filter this information by categories. Although it's not applicable for all web sites. <br />If your web site does not have categories you should be able to filter you news by using the "All" filter.<br /> "All" filter is available for every web site. As every Category filter "All" filter allow's you to filter your news by key words and Symbols count.<br /> Remember that if you have even one selected Filter you will recieve only the news containing your key word.<br /> If you leave the Key Word field blank this means that you will recieve all the news from the selected Category. If this Category is "All" so you will recieve all the news from this web site which is the same as unselecting all Categories filters. The only difference is that you can give your filter symbols count value.<br />For example if you want to recieve full news only from Category named "Sport" and all other news to be reduced to 1000 symbols. You should select "Sport" Category and add filter as you leave both fields "Text" and "Symbols Count" Blank. That way you will recieve all News from Sports Category. The other task you will accomplish similarly as you select "All" Category and add filter as you fill only the "Symbols Count" field with value "1000".<br /> If you have any questions please contact us.</span>
        
        <h3><asp:Label ID="CatNameLabel1" runat="server"></asp:Label> Filters</h3>
        <asp:Panel ID="AddFilterPanel" runat="server">

            <div style="display:inline-block; vertical-align:top;">
                <label>Text</label>
                <asp:TextBox ID="FilterTextTB" runat="server" Width="100"></asp:TextBox>
            </div>
            <div style="display:inline-block; vertical-align:top;">
                <label>Max symbols</label>
                <asp:TextBox ID="FilterSizeTB" TextMode="Number" Width="100" ToolTip="1~10000" runat="server"></asp:TextBox>
                <asp:RangeValidator style="display:block;" ID="RangeValidator1" runat="server" MaximumValue="10000" ValidationGroup="FilterValidationGroup"
                    MinimumValue="0" Type="Integer" EnableClientScript="true" ErrorMessage="Enter value 1~10 000"
                    ForeColor="Red" ControlToValidate="FilterSizeTB"></asp:RangeValidator>
            </div>                        
            <div style="display:inline-block; vertical-align:middle;">
                <asp:Button  ID="Button3" runat="server" OnClick="AddFilter_Click" Text="Add Filter" />   
            </div>
        </asp:Panel>
        <div>
        <asp:Panel CssClass="float-left-inline" ID="Panel3" runat="server" HorizontalAlign="Left" Width="250px" Height="500px"
            ScrollBars="Auto">
                    <asp:ListView ID="CategoriesListView" runat="server" EnableViewState="true" EnablePersistedSelection="true" DataKeyNames="id" SelectedIndex="0" OnSelectedIndexChanged="CategoriesListView_SelectedIndexChanged" OnSelectedIndexChanging="CategoriesListView_SelectedIndexChanging">
                        <ItemTemplate>                                                    
                                <asp:Panel ID="CategorySlot" runat="server">                                        
                                    <asp:CheckBox CssClass="label-checkbox" runat="server" AutoPostBack="true" OnCheckedChanged="Unnamed_CheckedChanged" Text='<%#GetCheckBoxText(GetDataItem()) %>' Checked='<%# CheckIsSelected(GetDataItem()) %>'/><asp:ImageButton ImageAlign="TextTop" CommandName="Select" runat="server" ImageUrl="~/Images/arrow.png" Height="15" Width="15" />                                
                                </asp:Panel>                
                        </ItemTemplate>
                    </asp:ListView>
         </asp:Panel>
            <div style="display:inline-block; vertical-align:top;">
                <h5><asp:Label ID="CatNameLabel" runat="server"></asp:Label> Filters</h5>
                <asp:GridView runat="server" DataSourceID="EntityDataSourceFilters" ID="FiltersGridView" AutoGenerateColumns="false" DataKeyNames="id" AllowSorting="True" CellPadding="4" ForeColor="#333333" GridLines="None">
                    <AlternatingRowStyle BackColor="White" ForeColor="#284775"></AlternatingRowStyle>
                    <EditRowStyle BackColor="#999999"></EditRowStyle>

                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White"></FooterStyle>

                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White"></HeaderStyle>

                    <PagerStyle HorizontalAlign="Center" BackColor="#284775" ForeColor="White"></PagerStyle>

                    <RowStyle BorderStyle="Solid" BorderWidth="1px" BorderColor="CadetBlue" BackColor="#F7F6F3" ForeColor="#333333" />

                    <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>

                    <SortedAscendingCellStyle BackColor="#E9E7E2"></SortedAscendingCellStyle>

                    <SortedAscendingHeaderStyle BackColor="#506C8C"></SortedAscendingHeaderStyle>

                    <SortedDescendingCellStyle BackColor="#FFFDF8"></SortedDescendingCellStyle>

                    <SortedDescendingHeaderStyle BackColor="#6F8DAE"></SortedDescendingHeaderStyle>
                    <Columns>
                            <asp:BoundField DataField="filterText" HeaderText="Text"></asp:BoundField>
                            <asp:BoundField DataField="symbolsCount" HeaderText="Symbols"></asp:BoundField>
                            <asp:CommandField ShowEditButton="true" ShowDeleteButton="true" DeleteText="Del" />
                        </Columns>
                    </asp:GridView>
            </div>
        </div>
        
</div>
</asp:Content>
