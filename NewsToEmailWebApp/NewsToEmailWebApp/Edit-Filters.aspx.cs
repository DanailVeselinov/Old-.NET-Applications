using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp
{
    public partial class EditFilters : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Server.Transfer("~/Account/Login.aspx");
                return;
            }
            if (!this.IsPostBack)
            {
                var context = new NewsToEmailDBEntities();
                Users currentUser = null;
                var wsIdString = Request.QueryString["wsid"];
                if (string.IsNullOrWhiteSpace(wsIdString) | !Methods.IsNumber(wsIdString))
                {
                    Server.Transfer("~/Account/Login.aspx");
                    return;
                }
                int wsId = int.Parse(wsIdString);
                if (User.Identity.Name == "didoeddy")
                {
                    var userId = Request.QueryString["id"];
                    if (userId != null & Methods.IsNumber(userId))
                    {
                        int id = int.Parse(userId);
                        currentUser = context.Users.FirstOrDefault(u => u.id == id);
                    }
                    else
                    {
                        currentUser = context.Users.FirstOrDefault(u => u.userName == User.Identity.Name);
                    }
                }
                else
                {
                    if (Methods.IsTextOnly(User.Identity.Name))
                    {
                        currentUser = context.Users.FirstOrDefault(u => u.userName == User.Identity.Name);                        
                    }
                    else
                    {
                        //log out
                    }
                }
                if (currentUser == null)
                {
                    Server.Transfer("~/Account/Login.aspx");
                    return;
                }
                ViewState["CurrentUserId"] = currentUser.id;
                ViewState["CurrentUserName"] = currentUser.userName;
                ViewState["WebSiteId"] = wsId;
                BindFilters(wsId);                
            }
        }
        
        private void BindFilters(int wsId)
        {
            var context = new NewsToEmailDBEntities();
            var ws = context.WebSites.FirstOrDefault(w => w.id == wsId);
            if (ws!=null)
            {
                CategoriesListView.DataSource = ws.Categories.OrderByDescending(c => c.count).ToList();
                CategoriesListView.DataBind();                
            }            
        }

        public bool CheckIsSelected(object dataItem)
        {
            try
            {
                int userId = (int)ViewState["CurrentUserId"];
                Categories item = dataItem as Categories;
                if (dataItem == null)
                {
                    return false;
                }
                if (item.FiltersCategories.Where(f => f.userId == userId & f.isSelected).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public string GetCheckBoxText(object dataItem)
        {
            try
            {
                int userId = (int)ViewState["CurrentUserId"];
                Categories item = dataItem as Categories;
                if (dataItem == null)
                {
                    return "";
                }
                return item.categoryName + " (" + item.FiltersCategories.Where(f => f.userId == userId).Count() + ")";
            }
            catch (Exception)
            {
                return "";
            }            
        }

        public string GetCategoryName()
        {
            try
            {
                var container = CategoriesListView.Items[CategoriesListView.SelectedIndex];
                var name = container.DataItem.ToString();
                return name;
            }
            catch (Exception)
            {
                return "noname";
            }            
        }
        
        protected void Button_Cancel_Click(object sender, EventArgs e)
        {
            BackToProfile();
        }

        private void BackToProfile()
        {
            string currentUserName = (string)ViewState["CurrentUserName"];
            int id = 0;
            if (User.Identity.Name == "didoeddy" & User.Identity.Name != currentUserName)
            {
                var userId = Request.QueryString["id"];
                if (userId != null)
                {
                    id = int.Parse(userId);
                }
            }
            if (id > 0)
            {
                Response.Redirect("~/Profile.aspx?id="+id);
            }
            else
            {
                Response.Redirect("~/Profile");
            }
        }

        protected void AddFilter_Click(object sender, EventArgs e)
        {
            Validate("FilterValidationGroup");
            if (!Page.IsValid)
            {
                return;
            }
            var context = new NewsToEmailDBEntities();
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            var categoryId = (int)CategoriesListView.SelectedDataKey.Value;
            var fTextTB = (Methods.FindControlRecursive(this, "FilterTextTB") as TextBox);
            var fText = fTextTB.Text;
            if (!Methods.IsTextOnly(fText))
            {  
                return;
            }
            var fSizeTB = (Methods.FindControlRecursive(this, "FilterSizeTB") as TextBox);
            var fSizeText = fSizeTB.Text;
            short? fSizeShort = null;
            if (!String.IsNullOrWhiteSpace(fSizeText) & Methods.IsNumber(fSizeText))
            {
                fSizeShort = short.Parse(fSizeText);
            }
            var isActiveCB = SiteMaster.FindControTypeRecursive(CategoriesListView.Items[CategoriesListView.SelectedIndex], "CheckBox") as CheckBox;
            isActiveCB.Checked = true;
            var freeFilter = context.FiltersCategories.FirstOrDefault(f => string.IsNullOrEmpty(f.filterText)& (!f.symbolsCount.HasValue | f.symbolsCount<1) & f.categoryId == categoryId & f.userId == user.id);
            if (freeFilter != null)
            {
                context.FiltersCategories.Remove(freeFilter);
                context.SaveChanges();
            }
            FiltersCategories fc = new FiltersCategories() { categoryId = categoryId, userId = user.id, filterText = fText, symbolsCount = fSizeShort, isSelected = isActiveCB.Checked };
            var sameFilter = context.FiltersCategories.FirstOrDefault(f => f.filterText == fc.filterText & f.categoryId == fc.categoryId & f.userId == fc.userId);
            if (sameFilter == null)
            {
                context.FiltersCategories.Add(fc);
                context.SaveChanges();
                //GridView filtersGridView = SiteMaster.FindSimilarControlRecursive(CategoriesListView.Items[CategoriesListView.SelectedIndex], "FiltersGridView") as GridView;
                //filtersGridView.DataBind();
                FiltersGridView.DataBind();
                int wsId = (int)ViewState["WebSiteId"];
                BindFilters(wsId);
                fTextTB.Text = "";
                fSizeTB.Text = "";
            }
            else
            {
                sameFilter.isSelected = isActiveCB.Checked;
                if (sameFilter.symbolsCount > fSizeShort){sameFilter.symbolsCount = fSizeShort;}
                context.SaveChanges();
            }
        }


        protected void Unnamed_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (sender as CheckBox);
            var dataItem = cb.Parent.Parent as ListViewDataItem;
            int catId = (int)CategoriesListView.DataKeys[dataItem.DisplayIndex].Value;
            int userId = (int)ViewState["CurrentUserId"];
            var context = new NewsToEmailDBEntities();
            if (cb.Checked)
            {
                var filtersList = context.FiltersCategories.Where(f => f.categoryId == catId & f.userId == userId).ToList();
                if (filtersList.Count>0)
                {
                    foreach (var filter in filtersList)
                    {
                        filter.isSelected = true;
                    }
                }
                else
                {
                    var fc = new FiltersCategories();
                    fc.categoryId = catId; fc.isSelected=true; fc.userId = userId;
                    context.FiltersCategories.Add(fc);
                }
                context.SaveChanges();
                //Label catNameLabel = SiteMaster.FindSimilarControlRecursive(CategoriesListView.Items[CategoriesListView.SelectedIndex], "CatNameLabel") as Label;
                //catNameLabel.Text = context.Categories.Find(catId).categoryName;
                CatNameLabel.Text = context.Categories.Find(catId).categoryName;
                CatNameLabel1.Text = context.Categories.Find(catId).categoryName;
                CategoriesListView.SelectedIndex = dataItem.DisplayIndex;
                cb.Focus();
                //TextBox filterTextTB = SiteMaster.FindSimilarControlRecursive(CategoriesListView.Items[CategoriesListView.SelectedIndex], "FilterTextTB") as TextBox;
                //filterTextTB.Focus();
                FilterTextTB.Focus();
            }
            else
            {
                var filtersList = context.FiltersCategories.Where(f => f.categoryId == catId & f.userId == userId).ToList();
                foreach (var filter in filtersList)
                {
                    filter.isSelected = false;
                }
                context.SaveChanges();
                CategoriesListView.SelectedIndex = 0;
                CategoriesListView_SelectedIndexChanged(CategoriesListView, new EventArgs());
                cb.Focus();
            }
        }

        public bool AddFilterVisible(object dataItem)
        {
            try
            {
                Categories item = dataItem as Categories;
                if (dataItem == null)
                {
                    return false;
                }
                if (item.id == (int)CategoriesListView.SelectedDataKey.Value)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        protected void CategoriesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lv = sender as ListView;
            var context = new NewsToEmailDBEntities();
            var id = (int)lv.SelectedValue;
            CatNameLabel.Text = context.Categories.FirstOrDefault(wde => wde.id == id).categoryName;
            CatNameLabel1.Text = CatNameLabel.Text;
        }

        protected void CategoriesListView_SelectedIndexChanging(object sender, ListViewSelectEventArgs e)
        {            
        }
    }
}