using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using xDC.Services;

namespace xDC_Web.Extension.CustomAttribute
{
    public class KflowApiAuthorize : AuthorizeAttribute
    {
        private readonly string _permissionName;

        public KflowApiAuthorize(string PermissionName)
        {
            _permissionName = PermissionName;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext.RequestContext.Principal.Identity.Name == null)
            {
                return false;
            }
            else
            {
                bool isAuthorized = new AuthService().IsUserHaveAccess(actionContext.RequestContext.Principal.Identity.Name, _permissionName);

                return isAuthorized;
            }
            
        }
    }
}