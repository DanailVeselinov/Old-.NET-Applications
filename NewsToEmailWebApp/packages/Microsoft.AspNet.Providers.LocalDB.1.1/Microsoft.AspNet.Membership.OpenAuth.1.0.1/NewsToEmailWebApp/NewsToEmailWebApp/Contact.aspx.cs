using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                var context = new Entities();
                var admin = context.Users.FirstOrDefault(u=>u.userName == "didoeddy");
                if (admin != null)
                {
                    ViewState["Contacts"] = admin.userEmail;                    
                }
                else
                {
                    ViewState["Contacts"] = "NewsToEmail@yahoo.com";
                }
            }
            Page.DataBind();

        }
    }
}