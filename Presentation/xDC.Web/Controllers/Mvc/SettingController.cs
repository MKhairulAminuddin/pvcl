using System;
using System.Web.Mvc;
using xDC.Domain.Web.Setting;
using xDC.Logging;
using xDC.Services.Application;
using xDC.Services.Membership;
using xDC_Web.Extension.CustomAttribute;

namespace xDC_Web.Controllers.Mvc
{
    [Authorize]
    [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings)]
    public class SettingController : Controller
    {
        #region Fields

        private readonly ISettingService _settingService = Startup.Container.GetInstance<ISettingService>();

        #endregion


        public ActionResult Index()
        {
            return View();
        }

        [KflowAuthorize(xDC.Utils.Common.PermissionKey.Settings_EmailNotification)]
        public ActionResult EmailNotiConfig()
        {
            try
            {
                var model = _settingService.EmailConfiguration();
                return View(model);
            }
            catch (Exception ex)
            {
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
                    var SaveStatus = _settingService.EmailConfigurationUpdate(req, User.Identity.Name);
                    return RedirectToAction("EmailNotiConfig");
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
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