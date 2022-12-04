using System.Web;
using xDC.Services.Membership;
using xDC.Utils;

namespace xDC_Web.Extension
{
    public static class MenuSecurity
    {
        private static readonly IRoleManagementService _roleService = Startup.Container.GetInstance<IRoleManagementService>();

        public static bool IsAuthenticated()
        {
            return (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated);
        }

        public static bool IsAuthenticatedAndAllowed(string permissionKey)
        {
            return IsAuthenticated() && _roleService.IsUserHaveAccess(HttpContext.Current.User.Identity.Name, permissionKey);
        }

        public static string CurrentUserRole()
        {
            return _roleService.GetUserRoles(HttpContext.Current.User.Identity.Name);
        }
    }
}