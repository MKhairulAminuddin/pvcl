using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using xDC.Services;
using xDC.Services.Membership;

namespace xDC_Web.Extension.CustomAttribute
{
    public class KflowAuthorize : AuthorizeAttribute
    {
        private List<string> _permissionNames = new();

        public KflowAuthorize(params string[] PermissionName)
        {
            foreach (var item in PermissionName)
            {
                _permissionNames.Add(item);
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If they are authorized, handle accordingly
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                IRoleManagementService _roleService = Startup.Container.GetInstance<IRoleManagementService>();
                var autorizationList = new List<bool>();

                foreach (var permissionName in _permissionNames)
                {
                    bool isAuthorized = _roleService.IsUserHaveAccess(filterContext.HttpContext.User.Identity.Name, permissionName);

                    autorizationList.Add(isAuthorized);
                }

                var allowed = autorizationList.Any(x => x == true);

                if (!allowed)
                {
                    filterContext.Result = new RedirectResult("~/Base/NoPermission");
                }
            }
            else
            {
                // Otherwise redirect to your specific authorized area
                filterContext.Result = new RedirectResult("~/Base/Unauthorized");
            }
        }
    }
}