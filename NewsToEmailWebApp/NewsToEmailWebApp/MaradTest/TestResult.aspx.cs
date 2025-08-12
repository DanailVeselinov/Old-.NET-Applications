using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp.MaradTest
{
    public partial class TestResult : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var wrong = Page.Request.Params["wrong"];
            var total = Page.Request.Params["total"];
            if (!string.IsNullOrWhiteSpace(wrong))
            {
                //how much and percent
                var wrongList = wrong.Trim(';').Split(';').ToList();
                wrongList.RemoveAll(q=>string.IsNullOrWhiteSpace(q));
                int totalInt; 
                int.TryParse(total,out totalInt);
                double percent = (totalInt - wrongList.Count) / (double)totalInt;
                message.Text = string.Format("Your result is {0:F0}%. {1} wrong out of {2} questions.", percent*100, wrongList.Count , totalInt);
                wrongPanel.Visible = true;
            }
            else
            {
                message.Text = "Your score is perfect.";
                perfectPanel.Visible = true;
            }
        }

        protected void BackToMain_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/MaradTest/MaradTest.aspx");
        }
        protected void Review_Click(object sender, EventArgs e)
        {
            var wrong = Page.Request.Params["wrong"];
            int userId = 0;
            int.TryParse(Page.Request.Params["id"], out userId);
            Response.Redirect("~/MaradTest/Test.aspx?id=" + userId + "&type=" + wrong);
        }
    }
}