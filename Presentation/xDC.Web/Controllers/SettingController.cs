using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.ViewModels;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User")]
    public class SettingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        

        public ActionResult NotificationConfig()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var appConfigs = db.Config_Application.ToList();

                    var cutOffTimeKey = Common.ApplicationConfigKeyMapping(1);
                    var cutOffTimeValue =
                        appConfigs.FirstOrDefault(x => x.Key == cutOffTimeKey);
                    DateTime.TryParseExact(cutOffTimeValue.Value, "HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var tryParseValue);

                    var inflowFundNotificationKey = Common.ApplicationConfigKeyMapping(2);
                    var inflowFundNotificationValue =
                        appConfigs.FirstOrDefault(x => x.Key == inflowFundNotificationKey);
                    bool.TryParse(inflowFundNotificationValue.Value, out var inflowFundNotificationValueParsed);

                    var inflowFundAdminEditNotificationKey = Common.ApplicationConfigKeyMapping(3);
                    var inflowFundAdminEditNotificationValue =
                        appConfigs.FirstOrDefault(x => x.Key == inflowFundAdminEditNotificationKey);
                    bool.TryParse(inflowFundAdminEditNotificationValue.Value, out var inflowFundAdminEditNotificationValueParsed);

                    NotificationConfigViewModel viewModel = new NotificationConfigViewModel()
                    {
                        InflowFundCutOffTime = tryParseValue,
                        InflowFundEnableNotification = inflowFundNotificationValueParsed,
                        InflowFundEnableAdminModificationNotification = inflowFundAdminEditNotificationValueParsed,
                        
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return View("Error");
            }
            
        }

        public ActionResult ApproverManagement()
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