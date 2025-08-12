using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Membership.OpenAuth;

namespace NewsToEmailWebApp.Account
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void RegisterUser_CreatedUser(object sender, EventArgs e)
        {
            Validate();
            if (!Page.IsValid)
            {
                return;
            }
            var context = new NewsToEmailDBEntities();
            var newUser = new Users();
            newUser.userName = this.UserName.Text;
            newUser.password = this.Password.Text;
            newUser.userEmail = this.Email.Text;
            newUser.sendNewsHour = 24;
            newUser.isEnabled = true;
            newUser.webSitesMaxNumber = 1;
            var similaruserNameUser = context.Users.FirstOrDefault(u => u.userName.ToUpper() == newUser.userName.ToUpper());
            if (similaruserNameUser!=null)
            {
                this.ErrorMessage.Text = "User Name is busy. Please try again!";
                Page.Validate();
                return;
            }
            var similarEmailUser = context.Users.FirstOrDefault(u => u.userEmail.ToUpper().Contains(newUser.userEmail.ToUpper()));
            var similarOldEmail = context.UsedEmails.FirstOrDefault(em => em.usedEmail1.ToUpper().Contains(newUser.userEmail.ToUpper()));
            if (similarEmailUser != null | similarOldEmail !=null)
            {
                this.ErrorMessage.Text = "Email is busy. Please try again!";
                Page.Validate();
                return;
            }
            try
            {                
                var usr = context.Users.Add(newUser);
                context.SaveChanges();
                context.UsersWebsites.Add(new UsersWebsites() { isSelected = true, userId = usr.id, validUntil = DateTime.Now.AddYears(7), webSiteId = context.WebSites.FirstOrDefault().id, lastSendDate=DateTime.Now.AddDays(-1) });
                context.SaveChanges();
                FormsAuthentication.SetAuthCookie(newUser.userName, createPersistentCookie: false);
                Response.Redirect("~/Profile");
            }
            catch (Exception)
            {
            }            
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
                validator.ErrorMessage = "* Password must be 8 to 12 symbols. At least 1 Upper, 1 lower letter and at least 1 digit.";
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

    }
}