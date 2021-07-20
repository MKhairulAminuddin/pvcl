using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xDC.Utils;

namespace xDC_Web.Extension.CustomHtmlHelper
{
    public class xDC_HtmlHelper
    {
        public static IHtmlString LoggedInUser()
        {
            var username = HttpContext.Current.User.Identity.Name;
            var role = "Anonymous";

            if (HttpContext.Current.User.IsInRole(Config.AclAdministrator))
            {
                role = Config.AclAdministrator;
            }
            else if (HttpContext.Current.User.IsInRole(Config.AclPowerUser))
            {
                role = Config.AclPowerUser;
            }
            else if (HttpContext.Current.User.IsInRole(Config.AclAmsd))
            {
                role = Config.AclAmsd;
            }
            else if (HttpContext.Current.User.IsInRole(Config.AclFid))
            {
                role = Config.AclFid;
            }
            else if (HttpContext.Current.User.IsInRole(Config.AclIisd))
            {
                role = Config.AclIisd;
            }
            else 
            {
                role = "Error";
            }


            var output = String.Format("<li class='dropdown-header' style='color: white'>{0}</li> <li class='dropdown-header' style='color: white'>{1}</li>", username, role);

            return new HtmlString(output);
        }
    }
}