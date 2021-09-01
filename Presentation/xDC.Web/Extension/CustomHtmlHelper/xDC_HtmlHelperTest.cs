using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using xDC.Utils;

namespace xDC_Web.Extension.CustomHtmlHelper
{
    public class xDC_HtmlHelperTest
    {
        public static IHtmlString LoggedInUser()
        {
            var username = HttpContext.Current.User.Identity.Name;
            var role = "Anonymous";

            if (HttpContext.Current.User.IsInRole(Config.Acl.Administrator))
            {
                role = Config.Acl.Administrator;
            }
            else if (HttpContext.Current.User.IsInRole(Config.Acl.PowerUser))
            {
                role = Config.Acl.PowerUser;
            }
            else if (HttpContext.Current.User.IsInRole(Config.Acl.Amsd))
            {
                role = Config.Acl.Amsd;
            }
            else if (HttpContext.Current.User.IsInRole(Config.Acl.Fid))
            {
                role = Config.Acl.Fid;
            }
            else if (HttpContext.Current.User.IsInRole(Config.Acl.Issd))
            {
                role = Config.Acl.Issd;
            }
            else 
            {
                role = "Error";
            }
            
            var output = String.Format("<p class='dropdown-header-title'>{0}</p> <p class='dropdown-header-subtitle'>{1}</p>", username, role);

            return new HtmlString(output);
        }

        /*public static MvcHtmlString WorkflowStepper(this HtmlHelper helper, string workflowStatus)
        {
            var sb = new StringBuilder();
            sb.Append(helper.Partial("_SomeOtherPartialView", workflowStatus));

            return new MvcHtmlString(sb.ToString());
        }*/
    }
}