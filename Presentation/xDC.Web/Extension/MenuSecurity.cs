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

        public static bool IsAuthenticatedAndAllowed(string permissionKey)
        {
            return IsAuthenticated() && new xDC.Services.AuthService().IsUserHaveAccess(HttpContext.Current.User.Identity.Name, permissionKey);
        }
    }
}