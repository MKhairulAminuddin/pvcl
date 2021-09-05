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
                    
                    var cutOffTimeValue =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.AMSD_IF_CutOffTime);
                    DateTime.TryParseExact(cutOffTimeValue.Value, "HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var tryParseValue);
                    
                    var contributionEmailValue =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail);
                    var emailList = new List<string>();
                    if (contributionEmailValue != null)
                    {
                        if (!string.IsNullOrEmpty(contributionEmailValue.Value))
                        {
                            emailList = contributionEmailValue.Value.Split(';').ToList();
                        }
                    }

                    NotificationConfigViewModel viewModel = new NotificationConfigViewModel()
                    {
                        InflowFundCutOffTime = tryParseValue,
                        TradeSettlementContributionEmail = emailList
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