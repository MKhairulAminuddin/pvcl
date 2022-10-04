using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.AuditTrail)]
    public class AuditController : Controller
    {
        [KflowAuthorize(Common.PermissionKey.AuditTrail)]
        public ActionResult Index()
        {
            return View();
        }

        [KflowAuthorize(Common.PermissionKey.AuditTrail_FormAudit)]
        public ActionResult Form()
        {
            return View();
        }

        [KflowAuthorize(Common.PermissionKey.AuditTrail_UserAccessAudit)]
        public ActionResult UserAccess()
        {
            return View();
        }
    }
}