using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        public ActionResult UserManagement()
        {
            return View();
        }

        public ActionResult RoleManagement()
        {
            return View();
        }

        public ActionResult ApplicationConfig()
        {
            return View();
        }

        public ActionResult SystemLogs()
        {
            return View();
        }

        public ActionResult SystemInformation()
        {
            return View();
        }

        public ActionResult Utility()
        {
            return View();
        }

        public ActionResult UserAccessLog()
        {
            return View();
        }

        public ActionResult UserActivityLog()
        {
            return View();
        }

        public ActionResult AdminLog()
        {
            return View();
        }
    }
}