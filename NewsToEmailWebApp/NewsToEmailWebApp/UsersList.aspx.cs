using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp
{
    public partial class UsersList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var context = new NewsToEmailDBEntities();
            ListUsers.DataSource = context.Users.ToList();
            ListUsers.DataBind();
        }

        protected void UsersList_DataBinding(object sender, EventArgs e)
        {

        }
    }
}