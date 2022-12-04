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
        private readonly string _permissionName;

        public KflowAuthorize(string PermissionName)
        {
            _permissionName = PermissionName;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If they are authorized, handle accordingly
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                IRoleManagementService _roleService = Startup.Container.GetInstance<IRoleManagementService>();
                bool isAuthorized = _roleService.IsUserHaveAccess(filterContext.HttpContext.User.Identity.Name, _permissionName);

                if (!isAuthorized)
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