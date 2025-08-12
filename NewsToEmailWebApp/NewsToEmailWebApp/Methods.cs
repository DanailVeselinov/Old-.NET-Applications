using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace NewsToEmailWebApp
{
    public class Methods
    {
        public static bool IsTextOnly(string text)
        {
            if (text == null)
            {
                return false;
            }
            foreach (char c in text)
            {
                if (!(char.IsLetterOrDigit(c) | char.IsWhiteSpace(c)))
                {
                    return false;
                }
            }
            return true;
        }


        public static bool IsNumber(string numberString)
        {
            if (numberString == null)
            {
                return false;
            }
            foreach (char c in numberString)
            {
                if (!char.IsNumber(c))
                {
                    return false;
                }
            }
            return true;
        }


        public static Control FindControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl == null)
            {
                return null;
            }
            if (rootControl.ID == controlID) return rootControl;

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn =
                    FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null) return controlToReturn;
            }
            return null;
        }

        public static Control FindSimilarControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl == null)
            {
                return null;
            }
            if (rootControl.ID.ToUpper().Contains(controlID.ToUpper())) return rootControl;

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn =
                    FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null) return controlToReturn;
            }
            return null;
        }

        public static Control FindParentControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl.Parent == null)
            {
                return null;
            }
            if (rootControl.Parent.ID == controlID)
            {
                return rootControl.Parent;
            }
            else
            {
                Control controlToReturn =
                    FindParentControlRecursive(rootControl.Parent, controlID);
                if (controlToReturn != null) return controlToReturn;
            }
            return null;
        }
    }
}