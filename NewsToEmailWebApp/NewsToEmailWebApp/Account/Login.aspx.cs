using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace NewsToEmailWebApp.Account
{
    public partial class Login : Page
    {        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/");
                return;
            }
            RegisterHyperLink.NavigateUrl = "Register";            
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
            }
        }


        protected void Login_Click(object sender, EventArgs e)
        {
            Validate();
            if (!Page.IsValid)
            {
                return;
            }
            try
            {
                var context = new NewsToEmailDBEntities();
                var newUser = context.Users.FirstOrDefault(u => u.userName == LoginView.UserName);
                if (newUser == null)
                {
                    return;
                }
                if (newUser.password != LoginView.Password)
                {
                    return;
                }
            }
            catch (Exception)
            {
                //Error Occured
                return;
            }            
            FormsAuthentication.SetAuthCookie(LoginView.UserName, createPersistentCookie: false);
            Response.Redirect("~/Profile");
        }

        public void CustomValidatorPassword_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = source as CustomValidator;
            if (args.Value == null)
            {
                args.IsValid = false;
                return;
            }
            var password = args.Value;
            //Is more than 8 and less than 13 symbols.
            if (password.Length < 8 | password.Length > 12)
            {
                args.IsValid = false;
                validator.ErrorMessage = "* Password must be less than 12 and more than 8 symbols.";
                return;
            }

            bool hasLetter = false;
            bool hasDigit = false;
            bool hasUpper = false;
            foreach (char c in password)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    args.IsValid = false;
                    validator.ErrorMessage = "* Password must contain Only Digits and Numbers";
                    return;
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
                args.IsValid = false;
                validator.ErrorMessage = "* Password must contain at least one Capital, one Lower and one Digit.";
                return;
            }
            args.IsValid = true;
        }

        protected void UserName_Load(object sender, EventArgs e)
        {
            (sender as TextBox).Focus();
        }

        protected void ForgotPassword_Click(object sender, EventArgs e)
        {
            
            var reqUName = Methods.FindControlRecursive(Page,"RequiredFieldValidatorUserName") as RequiredFieldValidator;
            var userName = LoginView.UserName;
            if (string.IsNullOrWhiteSpace(userName))
            {
                reqUName.Validate();
                return;
            }
            else
            {
                var context = new NewsToEmailDBEntities();
                var user = context.Users.FirstOrDefault(u => u.userName == userName);
                if (user != null)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Thease are your credentials:");
                    sb.AppendLine("Username: " + user.userName);
                    sb.AppendLine("Password: "+ user.password);
                    sb.AppendLine("Thank you for using our service.");                    
                    MailMessage mail = new MailMessage("emailsender8479@yahoo.com", user.userEmail, "Password Recovery", sb.ToString());                         
                    SmtpClient client = new SmtpClient("smtp.mail.yahoo.com", 587);
                    mail.Priority = MailPriority.Normal;
                    client.EnableSsl = true;
                    client.DeliveryFormat = SmtpDeliveryFormat.International;
                    client.UseDefaultCredentials = false;
                    mail.IsBodyHtml = false;
                    client.Credentials = new NetworkCredential("emailsender8479@yahoo.com", "Eddy8479");                                        
                    try
                    {
                        client.Send(mail);
                        var script = string.Format("alert('You recieved your password on e-mail: {0}');",user.userEmail);
                        Page.ClientScript.RegisterStartupScript(
                        this.GetType(),
                        "update_Script",
                        script,
                        true);
                    }
                    catch (Exception)
                    {
                        var fText = Methods.FindControlRecursive(Page, "FailureText") as Literal;
                        fText.Text = "Error sending message. Please try again.";
                        fText.Visible = true;
                        return;
                    }
                }
            }
        }

        
    }
}