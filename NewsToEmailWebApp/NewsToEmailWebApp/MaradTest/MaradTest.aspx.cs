using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp.MaradTest
{
    public partial class MaradTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Page.User.Identity.IsAuthenticated)
                {
                    UserNameTB.Text = User.Identity.Name;
                }
                BindStartLists();
            }
        }

        private void BindStartLists()
        {
            var context = new MaradTestDBL.MaradDBEntities();
            var questions = context.Questions.ToList();
            var categoriesList = new List<string>();
            foreach (var q in questions)
            {
                if (!categoriesList.Contains(q.category))
                {
                    categoriesList.Add(q.category);
                }
            }
            categoriesLB.DataSource = categoriesList;
            levelLB.DataSource = new List<string>() { "Оперативно", "Управленско" };
            categoriesLB.DataBind();
            levelLB.DataBind();
        }

        protected void StartTest_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (Page.IsValid)
            {
                var context = new MaradTestDBL.MaradDBEntities();
                var user = context.Users.FirstOrDefault(u=>u.name==UserNameTB.Text);
                if (user==null)
                {
                    user = new MaradTestDBL.Users(){name = UserNameTB.Text, password=""};
                    context.Users.Add(user);
                    context.SaveChanges();
                }
                string categories ="";
                foreach (ListItem item in categoriesLB.Items)
                {
                    if (item.Selected)
                    {
                        categories += item.Text + ";";
                    }
                }
                try
                {
                    string level = levelLB.SelectedValue;
                    string testType = TestTypeRBL.SelectedItem.Text;
                    if (!string.IsNullOrWhiteSpace(categories) & !string.IsNullOrWhiteSpace(level) & !string.IsNullOrWhiteSpace(testType))
                    {
                        Response.Redirect("~/MaradTest/Test.aspx?id=" + user.id + "&category=" + Server.UrlEncode(categories) + "&level=" + Server.UrlEncode(level) + "&type=" + Server.UrlEncode(testType));
                    }
                }
                catch (Exception n)
                {
                    n = null;
                }                
            }
        }
    }
}