using System.Web;
using xDC.Utils;

namespace xDC_Web.Extension
{
    public static class MenuSecurity
    {
        public static bool IsAuthenticated()
        {
            return (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated);
        }

        public static bool AdminOnlyMenu
        {
            get
            {
                if (IsAuthenticated() &&
                    (HttpContext.Current.User.IsInRole(Config.Acl.Administrator))
                )
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public static bool PowerUsersMenu
        {
            get
            {
                if (IsAuthenticated() &&
                    (HttpContext.Current.User.IsInRole(Config.Acl.Administrator) || HttpContext.Current.User.IsInRole(Config.Acl.PowerUser)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool AmsdMenu
        {
            get
            {
                if (IsAuthenticated() &&
                    (HttpContext.Current.User.IsInRole(Config.Acl.Administrator) || HttpContext.Current.User.IsInRole(Config.Acl.PowerUser) || HttpContext.Current.User.IsInRole(Config.Acl.Amsd)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool FidMenu
        {
            get
            {
                if (IsAuthenticated() &&
                    (HttpContext.Current.User.IsInRole(Config.Acl.Administrator) || HttpContext.Current.User.IsInRole(Config.Acl.PowerUser) || HttpContext.Current.User.IsInRole(Config.Acl.Fid)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IssdMenu
        {
            get
            {
                if (IsAuthenticated() &&
                    (HttpContext.Current.User.IsInRole(Config.Acl.Administrator) || HttpContext.Current.User.IsInRole(Config.Acl.PowerUser) || HttpContext.Current.User.IsInRole(Config.Acl.Issd)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        /*public static bool AllowedMenu()
        {
            RolePrincipal r = (RolePrincipal)HttpContext.Current.User;
            var rolesArray = r.GetRoles();
        }*/
    }
}