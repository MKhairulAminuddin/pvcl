using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User")]
    public class SettingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        

        public ActionResult ApplicationConfig()
        {
            return View();
        }

        public ActionResult Workflow()
        {
            return View();
        }

        public ActionResult AuditTrail()
        {
            return View();
        }

        #region Dropdown Configuration

        public ActionResult DropdownConfig()
        {
            return View();
        }

        #endregion
    }
}