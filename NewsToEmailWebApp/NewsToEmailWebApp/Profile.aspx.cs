using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp
{
    public partial class Profile : System.Web.UI.Page
    {
        [Serializable]
        public partial class UsersProfile
        {
            public UsersProfile(Users user)
            {
                this.id = user.id;
                this.isEnabled = user.isEnabled;
                this.convertCyrlicToLatin = user.convertCyrlicToLatin;
                this.sendNewsHour = user.sendNewsHour;
                this.sizeLimit = user.sizeLimit;
                this.userEmail = user.userEmail;
                this.userName = user.userName;
                this.webSitesMaxNumber = user.webSitesMaxNumber;
                this.recieveFile = user.recieveFile;
            }
            public int id { get; set; }
            public string userName { get; set; }
            public string userEmail { get; set; }
            public bool isEnabled { get; set; }
            public Nullable<short> sendNewsHour { get; set; }
            public bool convertCyrlicToLatin { get; set; }
            public Nullable<int> sizeLimit { get; set; }
            public short webSitesMaxNumber { get; set; }
            public bool recieveFile { get; set; }
        }


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
                if (User.Identity.Name == "didoeddy")
                {
                    var userId = Request.QueryString["id"];
                    if (userId!=null)
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
                    currentUser = context.Users.FirstOrDefault(u => u.userName == User.Identity.Name);
                }                
                if (currentUser== null)
                {
                    Server.Transfer("~/Account/Login.aspx");
                    return;
                }
                ViewState["CurrentUserId"] = currentUser.id;
                ViewState["CurrentUserName"] = currentUser.userName;
                this.DataBind();
                BindPateControls(currentUser);
                LiveScoresTV_Init(LiveScoresTV,new EventArgs());
                BindWebSitesListview();
            }
        }        

        private void BindPateControls(Users currentUser)
        {
            UserName.Text = currentUser.userName;
            Email.Text = currentUser.userEmail;
            IsActiveCB.Checked = !currentUser.isEnabled;
            RecieveFileCB.Checked = currentUser.recieveFile;
            ConvertToLatinCB.Checked = currentUser.convertCyrlicToLatin;
            foreach (ListItem item in SendNewsHourDDL.Items)
	        {
                if(item.Text == currentUser.sendNewsHour.ToString())
                    item.Selected = true;
	        }
            limitTB.Text = currentUser.sizeLimit == null ? "" : currentUser.sizeLimit.ToString();
        }

        

        protected void EditUserButton_Click(object sender, EventArgs e)
        {
            var context = new NewsToEmailDBEntities();
            int id = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == id);
            if (user == null)
            {
                FormsAuthentication.SignOut();
                Server.Transfer("~/Account/Login.aspx");
                return;
            }
            //user.userName = GetValidUsername(UserName.Text);
            if (user.userName != (string)ViewState["CurrentUserName"])
            {
                var similaruserNameUser = context.Users.FirstOrDefault(u => u.userName.ToUpper() == user.userName.ToUpper());
                if (similaruserNameUser != null | user.userName == "invalid")
                {
                    this.ErrorMessage.Text = "User Name is busy. Please try again!";                
                    return;
                }                
            }
            var validEmail = GetValidEmail(Email.Text);
            if (user.userEmail.ToUpper() != validEmail.ToUpper())
            {
                var samePrevEmail = context.UsedEmails.Where(em => em.userId != user.id & em.usedEmail1.ToUpper() == validEmail.Trim().ToUpper()).FirstOrDefault();
                var currentUsersEmail = context.Users.FirstOrDefault(us => us.userEmail.ToUpper() == validEmail.ToUpper());
                if (samePrevEmail == null &  currentUsersEmail == null)
                {
                    var myPrevEmail = context.UsedEmails.FirstOrDefault(ue => ue.userId == user.id & ue.usedEmail1.ToUpper() == validEmail.ToUpper());
                    if (myPrevEmail == null)
                    {
                        context.UsedEmails.Add(new UsedEmails() { usedEmail1 = user.userEmail, userId = user.id });                                
                    }
                    user.userEmail = validEmail;
                }
                else
                {
                    if (User.Identity.Name == "didoeddy")
                    {                        
                        user.userEmail = validEmail;
                    }
                    else
                    {
                        this.ErrorMessage.Text = "Email is busy. Please try again!";
                        return;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(Password.Text))
            {
                if (IsPasswordValid(Password.Text))
                {
                    CustomValidatorPassword.IsValid = false;
                    CustomValidatorPassword.Visible = true;
                    return;
                }
                CompareValidatorConfirmPassword.Validate();
                if (!CompareValidatorConfirmPassword.IsValid)
                {
                    CompareValidatorConfirmPassword.Visible = true;
                    return;
                }
                user.password = Password.Text;                
            }
            user.isEnabled = !IsActiveCB.Checked;
            user.recieveFile = RecieveFileCB.Checked;
            user.convertCyrlicToLatin = ConvertToLatinCB.Checked;
            short sendhour = short.Parse(SendNewsHourDDL.SelectedValue);
            if (sendhour < 0 | sendhour > 168)
            {
                this.ErrorMessage.Text = "Invalid Send News Hour Selected!";
                return;
            }
            user.sendNewsHour = short.Parse(SendNewsHourDDL.SelectedValue);
            SizeLimitRangeValidator.Validate();
            if (!SizeLimitRangeValidator.IsValid)
	        {
		            return;
	        }
            if (!string.IsNullOrWhiteSpace(limitTB.Text))
                user.sizeLimit = int.Parse(limitTB.Text);
            context.SaveChanges();
        }

        private string GetValidEmail(string p)
        {

            if (string.IsNullOrWhiteSpace(p) | !p.Contains('@'))
            {
                this.ErrorMessage.Text = "Enter valid E-mail!";
                return "invalid";
            }
            return p;
        }


        private bool IsValidEmail(string email)
        {
            email = email.Trim();
            foreach (char c in email)
            {
                if (!char.IsLetterOrDigit(c) & c != '@' & c != '.')
                {
                    return false;
                }
            }
            return true;
        }

        private string GetValidUsername(string tb)
        {
            foreach (char item in tb)
            {
                if (!char.IsLetterOrDigit(item))
                {
                    return "invalid";
                }
            }
            return tb;
        }
        
        protected void BindWebSitesListview()
        {
            var context = new NewsToEmailDBEntities();
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u=>u.id == userId);
            if (user==null)
	        {
                ////DIsplay essage to create user first;
                Server.Transfer("~/Account/Login.aspx");
		        return;
	        }
            var slotsList = new List<UsersWebsites>(user.webSitesMaxNumber);
            if (user.webSitesMaxNumber<1)
            {
                return;
            }
            var usersWebSites = user.UsersWebsites.Where(u=>u.isSelected).ToList();
            var usersUnActiveWS = user.UsersWebsites.Where(u => !u.isSelected).ToList();
            for (int i = 0; i < user.webSitesMaxNumber; i++)
            {
                if (i < user.webSitesMaxNumber)
                {
                    var currentSlot = new UsersWebsites() {userId = user.id, webSiteId = 1, isSelected = true, validUntil = DateTime.Now.AddYears(7), lastSendDate = DateTime.Now.AddDays(-1) };
                    if (usersWebSites.Count > i)
                    {
                        currentSlot = usersWebSites[i];
                    }
                    else
                    {
                        if (usersUnActiveWS.Count>0)
                        {
                            currentSlot.webSiteId = usersUnActiveWS[0].webSiteId;
                            usersUnActiveWS[0].isSelected = true;
                            usersUnActiveWS.RemoveAt(0);
                            context.SaveChanges();
                        }
                        context.UsersWebsites.Add(currentSlot);
                        context.SaveChanges();
                    }
                    slotsList.Add(currentSlot);
                }
                else
                {
                    context.UsersWebsites.Remove(usersWebSites[i]);
                    context.SaveChanges();
                }
            }
            //while (counter < user.webSitesMaxNumber)
            //{
            //    var currentSlot = new UsersWebsites() { id = 0, userId = user.id, webSiteId = 0, isSelected = true, validUntil = DateTime.Now.AddYears(7) };
            //    if (usersWebSites.Count > wsId)
            //    {
            //        currentSlot = usersWebSites[wsId];
            //    }
            //    slotsList.Add(currentSlot);
            //    counter++;
            //    wsId++;
            //}
            WebSitesListView.DataSource = slotsList;
            WebSitesListView.DataBind();
        }

        public bool IsVisible(int id)
        {
            return id < 1 ? false : true;
        }

        protected void WebSitesSelect_Init(object sender, EventArgs e)
        {
            var context = new NewsToEmailDBEntities();
            var webSitesDDL = sender as DropDownList;
            var webSitesList = context.WebSites.ToList();
            webSitesList.Add(new WebSites() { id = 0, siteName = "Add New WebSite" });
            webSitesDDL.DataSource = webSitesList;
            webSitesDDL.DataBind();
        }

        protected void AddNewSlot_Click(object sender, EventArgs e)
        {
            var context = new NewsToEmailDBEntities();
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            if (user == null)
            {
                ////DIsplay essage to create user first;
                Server.Transfer("~/Account/Login.aspx");
                return;
            }
            if (user.userName != "didoeddy")
            {
                if (user.webSitesMaxNumber>3)
                {
                    ErrorMessage.Text = "This is a free version so maximum 2 web-site slots are allowed. Please contact us!";
                    ErrorMessage.Visible = true;                    
                    return;
                }
            }
            user.webSitesMaxNumber++;
            context.SaveChanges();
            BindWebSitesListview();
        }

        protected void DropDownListSelectWebSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                return;
            }
            var context = new NewsToEmailDBEntities();
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            var webSiteDDL = sender as DropDownList;
            int selectedWebSiteId = int.Parse(webSiteDDL.SelectedValue);
            var editedUsersWebsites = GetEditedListViewItems(user);
            switch (selectedWebSiteId)
            {
                case 0:
                    ViewState["EditedUsersWebsite"] = editedUsersWebsites.Count>0?editedUsersWebsites.FirstOrDefault().id:0;
                    var addWSPanel = Methods.FindControlRecursive(this, "AddNewWebSitePanel") as Panel;
                    addWSPanel.Visible = true;
                    AddNewWebSiteTB.Focus();
                    return;
                case -1:
                    foreach (var item in editedUsersWebsites)
                    {
                        context.UsersWebsites.Remove(context.UsersWebsites.FirstOrDefault(u=>u.id == item.id));
                    }
                    context.SaveChanges();
                    BindWebSitesListview();
                    return;
                default:
                    if (editedUsersWebsites.Count < 1)
                    {
                        AddUsersWebsite(context, user, selectedWebSiteId);
                    }
                    else
                    {
                        foreach (var item in editedUsersWebsites)
                        {
                            var editedItem = context.UsersWebsites.FirstOrDefault(uw => uw.id == item.id);
                            if (editedItem != null)
                            {
                                editedItem.webSiteId = selectedWebSiteId;
                            }
                        }
                    }
                    break;
            }
            context.SaveChanges();
            var websiteIndex = int.Parse(webSiteDDL.SelectedValue);
            AddNewWebSitePanel.Visible = false;
            BindWebSitesListview();
        }

        private static void AddUsersWebsite(NewsToEmailDBEntities context, Users user, int selectedWebSiteId)
        {
            var similarWS = context.UsersWebsites.FirstOrDefault(uw => uw.userId == user.id & uw.webSiteId == selectedWebSiteId);
            if (similarWS == null)
            {
                context.UsersWebsites.Add(new UsersWebsites() { isSelected = true, userId = user.id, validUntil = DateTime.Now.AddYears(7), webSiteId = selectedWebSiteId });
            }
            else
            {
                similarWS.isSelected = true;
            }
        }

        private List<UsersWebsites> GetEditedListViewItems(Users user)
        {
            WebSitesListView.Items.ToList();
            var usersWebsites = user.UsersWebsites.Where(u=>u.isSelected).ToList();
            foreach (ListViewItem item in WebSitesListView.Items)
            {
                DropDownList lvDDL = SiteMaster.FindControTypeRecursive(item, "DropDownList") as DropDownList;
                var itemSelectedValue = int.Parse(lvDDL.SelectedValue);
                var match = usersWebsites.FirstOrDefault(uw => uw.webSiteId == itemSelectedValue);
                usersWebsites.Remove(match);
            }
            return usersWebsites;
        }

        protected void CancelUserButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void DropDownListSelectWebSite_DataBound(object sender, EventArgs e)
        {
            var webSitesDDL = sender as DropDownList;
            var webSitesList = webSitesDDL.Items;
            UsersWebsites boundItem = null;
            try
            {
                boundItem = ((sender as Control).DataItemContainer as ListViewDataItem).DataItem as UsersWebsites;
            }
            catch (Exception)
            {
            }
            if (boundItem != null)
            {
                for (int i = 0; i < webSitesList.Count; i++)
                {
                    if (boundItem.webSiteId < 1)
                    {
                        webSitesDDL.SelectedIndex = 0;//webSitesList.Count - 1;
                        return;
                    }
                    if (webSitesList[i].Value == boundItem.webSiteId.ToString())
                    {
                        webSitesDDL.SelectedIndex = i;
                        AddNewWebSitePanel.Visible = false;
                        return;
                    }
                }
            }
        }
        public int GetSizeLimit(int? sizeLimit)
        {
            if (sizeLimit==null)
            {
                return 0;                
            }
            else
            {
                if (sizeLimit<1 | sizeLimit>10000)
	            {
                    return 0;
	            }
                else
                {
                    return (int)sizeLimit;
                }
            }

        }

        protected void AddWS_Click(object sender, EventArgs e)
        {
            var context = new NewsToEmailDBEntities();
            var newWebsiteTB = Methods.FindControlRecursive(this,"AddNewWebSiteTB") as TextBox;
            var NewWebsiteValid = newWebsiteTB.Text;
            if (string.IsNullOrWhiteSpace(NewWebsiteValid))
            {
                newWebsiteTB.Text = "Enter valid web site";
                newWebsiteTB.Focus();
                return;
            }
            else
            {
                foreach (var item in NewWebsiteValid)
                {
                    if (!char.IsLetterOrDigit(item) & !char.IsPunctuation(item) & !char.IsSeparator(item))
                    {
                        newWebsiteTB.Text = "Enter valid web site";
                        newWebsiteTB.Focus();
                        return;       
                    }
                }
            }
            //var dotIndex = NewWebsiteValid.IndexOf('.') + 1;
            //var slashIndex = NewWebsiteValid.IndexOf('/', dotIndex);
            //var domain = NewWebsiteValid.Substring(dotIndex, slashIndex <= 0 ? NewWebsiteValid.Length - dotIndex : slashIndex - dotIndex);
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            var newWebSite = context.WebSites.FirstOrDefault(w => w.siteName.ToUpper() == NewWebsiteValid.ToUpper());
            if (newWebSite == null)
            {
                newWebSite = new WebSites() { siteName = NewWebsiteValid };
                GetDomain(newWebSite);
                context.WebSites.Add(newWebSite);
                context.Categories.Add(new Categories() { categoryName = "All", webSiteId = newWebSite.id });
                context.SaveChanges();
            }
            var usersWebsite = (int)ViewState["EditedUsersWebsite"];
            if (usersWebsite>0)
            {
                var activeUWS = context.UsersWebsites.FirstOrDefault(u => u.id == usersWebsite);
                activeUWS.isSelected = true;
                activeUWS.webSiteId = newWebSite.id;
            }
            else
            {
                AddUsersWebsite(context, user, newWebSite.id);
            }
            context.SaveChanges();
            BindWebSitesListview();
            var addWSPanel = Methods.FindControlRecursive(this, "AddNewWebSitePanel") as Panel;
            addWSPanel.Visible = false;
            AddNewWebSiteTB.Text = "";
            WebSitesListView.Focus();
        }

        private static void GetDomain(WebSites ws)
        {
            var domain = ws.siteName.ToUpper().Replace("HTTP://", "").Replace("HTTPS://", "").Split('/').FirstOrDefault();
            var domParts = domain.Split('.');
            string[] startUrl = new string[] { "M", "MOBILE", "WWW", "WAP", "WEP" };
            foreach (var item in startUrl)
            {
                if (domParts[0].ToUpper() == item)
                {
                    domain = domain.Substring(item.Length + 1);
                    break;
                }
            }
            ws.domain = domain;
        }

        public Users GetUser(int id)
        {
            var context = new NewsToEmailDBEntities();
            return context.Users.FirstOrDefault(u=>u.id == id);
        }

        public bool IsPasswordValid(string p)
        {
            bool result = true;
            var password = p;
            //Is more than 8 and less than 13 symbols.
            if (password.Length < 8 | password.Length > 12)
            {                
                result = false;
            }
            bool hasLetter = false;
            bool hasDigit = false;
            bool hasUpper = false;
            foreach (char c in password)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    result = false;
                }
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                    if (char.IsUpper(c))
                    {
                        hasUpper = true;
                    }
                }
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
            }
            if (!hasDigit | !hasLetter | !hasUpper)
            {
                result = false;
            }
            return result;
        }

        protected void DeleteNewsPaper_Click(object sender, EventArgs e)
        {
            var context = new NewsToEmailDBEntities();
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            if (user == null)
            {
                ////DIsplay essage to create user first;
                Server.Transfer("~/Account/Login.aspx");
                return;
            }
            if (user.webSitesMaxNumber>0)
            {
                user.webSitesMaxNumber--;                
            }
            var activeWs = user.UsersWebsites.Where(u => u.isSelected).ToList();
            if (activeWs.Count>user.webSitesMaxNumber)
            {
                context.UsersWebsites.Remove(context.UsersWebsites.Find(activeWs.Last().id));
            }
            context.SaveChanges();
            BindWebSitesListview();
        }
        
        protected void WebSitesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                return;
            }
            var context = new NewsToEmailDBEntities();
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            var selectedItem = (sender as ListView).Items[(sender as ListView).SelectedIndex] as ListViewDataItem;            
            var webSiteDDL = SiteMaster.FindControTypeRecursive(selectedItem,"DropDownList") as DropDownList;
            int selectedWebSiteId = int.Parse(webSiteDDL.SelectedValue);
            string currentUserName = (string)ViewState["CurrentUserName"];
            
            if (userId > 0 & selectedWebSiteId > 0)
            {
                Response.Redirect("~/Edit-Filters.aspx?id=" + userId+"&wsid="+ selectedWebSiteId);
            }
        }

        protected void WebSitesListView_SelectedIndexChanging(object sender, ListViewSelectEventArgs e)
        {

        }

        protected void ButtonLivescores_Click(object sender, EventArgs e)
        {
            string currentUserName = (string)ViewState["CurrentUserName"];
            int id = 0;
            if (User.Identity.Name == "didoeddy" & User.Identity.Name!=currentUserName)
            {
                var userId = Request.QueryString["id"];
                if (userId != null)
                {
                    id = int.Parse(userId);
                }
            }
            if (id>0)
            {
                Response.Redirect("~/Livescore.aspx?id="+id);
            }
            else
            {
                Response.Redirect("~/Livescore");
            }
        }

        protected void LiveScoresTV_Init(object sender, EventArgs e)
        {

            var context = new NewsToEmailDBEntities();
            if (ViewState["CurrentUserId"]==null)
            {
                return;
            }
            int userId = (int)ViewState["CurrentUserId"];
            var user = context.Users.FirstOrDefault(u => u.id == userId);
            if (user == null)
            {
                return;
            }
            //Change this with user.Livescores.Tolis();
            var usersLivescores = user.Livescores.ToList();
            var lstv = sender as TreeView;
            foreach (Livescores uls in usersLivescores)
            {
                var usport = uls.sport.ToUpper();
                var ucomp = uls.competition.ToUpper();
                var ulevel = uls.level;
                string type ="";
                if (!string.IsNullOrWhiteSpace(ulevel))
                {
                    type = "LEVEL";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(ucomp))
                    {
                        type = "COMPETITION";
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(usport))
                        {
                            type = "SPORT";
                        }
                    }
                }

                string parentNodeValuePath = "";
                TreeNode parentNode = null;
                TreeNode newNode = null;
                switch (type)
                {
                    case "SPORT":
                        parentNodeValuePath = usport;
                        parentNode = lstv.FindNode(parentNodeValuePath);
                        if (parentNode==null)
                        {
                            parentNode = new TreeNode(usport, usport);
                            lstv.Nodes.Add(parentNode);
                        }
                        break;
                    case "COMPETITION":
                        parentNodeValuePath = usport;
                        parentNode = lstv.FindNode(parentNodeValuePath);
                        if (parentNode==null)
                        {
                            parentNode = new TreeNode(usport, usport);
                            lstv.Nodes.Add(parentNode);                            
                        }
                        parentNodeValuePath = usport + "/" + ucomp;
                        newNode = lstv.FindNode(parentNodeValuePath);
                        if (newNode==null)
                        {
                            var compNode = new TreeNode(ucomp, ucomp);
                            parentNode.ChildNodes.Add(compNode);
                        }
                        break;
                    case "LEVEL":
                        parentNodeValuePath = usport;
                        parentNode = lstv.FindNode(parentNodeValuePath);
                        if (parentNode==null)
                        {
                            parentNode = new TreeNode(usport, usport);
                            lstv.Nodes.Add(parentNode);                            
                        }
                        parentNodeValuePath = usport + "/" + ucomp;
                        newNode = lstv.FindNode(parentNodeValuePath);
                        if (newNode==null)
                        {
                            newNode = new TreeNode(ucomp, ucomp);
                            parentNode.ChildNodes.Add(newNode);
                        }
                        parentNode = newNode;
                        parentNodeValuePath = usport + "/" + ucomp + "/" + ulevel;
                        newNode = lstv.FindNode(parentNodeValuePath);
                        if (newNode==null)
                        {
                            newNode = new TreeNode(ulevel, ulevel);
                            parentNode.ChildNodes.Add(newNode);
                        }
                        break;
                    default:
                        break;
                }                
            }
        }


    }
}