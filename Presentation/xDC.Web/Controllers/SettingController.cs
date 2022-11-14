using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.Web.Setting;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings)]
    public class SettingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings_EmailNotification)]
        public ActionResult EmailNotiConfig()
        {
            try
            {
                var model = SettingService.EmailConfiguration();
                return View(model);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return View("Error");
            }
            
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings_EmailNotification)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmailNotiConfig(EmailConfigSetting req)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var SaveStatus = SettingService.EmailConfigurationUpdate(req, User.Identity.Name);
                    return RedirectToAction("EmailNotiConfig");
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return View("Error");
            }

        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings_ApproverAssignment)]
        public ActionResult ApproverManagement()
        {
            return View();
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings_DropdownData)]
        public ActionResult DropdownConfig()
        {
            return View();
        }
    }
}