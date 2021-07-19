using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using xDC.Utils;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace xDC_Web.Extension.CustomAttribute
{
    
    public class InflowFundFormPermissionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var user = HttpContext.Current.User.Identity.Name;
            if (!(HttpContext.Current.User.IsInRole(Config.AclPowerUser) || HttpContext.Current.User.IsInRole(Config.AclAmsd) || HttpContext.Current.User.IsInRole(Config.AclAdministrator)))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));

                base.OnActionExecuting(actionContext);
            }
        }
    }
}