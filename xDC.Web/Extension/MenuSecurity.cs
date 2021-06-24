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
                    (HttpContext.Current.User.IsInRole(Config.AclAdministrator))
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
                    (HttpContext.Current.User.IsInRole(Config.AclAdministrator) || HttpContext.Current.User.IsInRole(Config.AclPowerUser)))
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
                    (HttpContext.Current.User.IsInRole(Config.AclAdministrator) || HttpContext.Current.User.IsInRole(Config.AclPowerUser) || HttpContext.Current.User.IsInRole(Config.AclAmsd)))
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
                    (HttpContext.Current.User.IsInRole(Config.AclAdministrator) || HttpContext.Current.User.IsInRole(Config.AclPowerUser) || HttpContext.Current.User.IsInRole(Config.AclFid)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IisdMenu
        {
            get
            {
                if (IsAuthenticated() &&
                    (HttpContext.Current.User.IsInRole(Config.AclAdministrator) || HttpContext.Current.User.IsInRole(Config.AclPowerUser) || HttpContext.Current.User.IsInRole(Config.AclIisd)))
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