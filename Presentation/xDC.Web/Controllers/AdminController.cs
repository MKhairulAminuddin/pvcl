using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult UserManagement()
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
    }
}