using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using xDC.Services;
using xDC.Services.Application;
using xDC.Services.Membership;

namespace xDC_Web.Extension.CustomAttribute
{
    public class KflowApiAuthorize : AuthorizeAttribute
    {
        private List<string> _permissionNames = new();
        public KflowApiAuthorize(params string[] PermissionName)
        {
            foreach (var item in PermissionName)
            {
                _permissionNames.Add(item);
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext.RequestContext.Principal.Identity.Name == null)
            {
                return false;
            }
            else
            {
                IRoleManagementService _roleService = Startup.Container.GetInstance<IRoleManagementService>();
                
                var autorizationList = new List<bool>();

                foreach (var permissionName in _permissionNames)
                {
                    bool isAuthorized = _roleService.IsUserHaveAccess(actionContext.RequestContext.Principal.Identity.Name, permissionName);

                    autorizationList.Add(isAuthorized);
                }

                var allowed = autorizationList.Any(x => x == true);


                return allowed;
            }
            
        }
    }
}