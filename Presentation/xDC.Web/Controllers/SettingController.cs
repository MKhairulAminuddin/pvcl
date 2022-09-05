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
                    
                    var cnEmail_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail);
                    var cnEmailList = new List<string>();
                    if (cnEmail_db != null)
                    {
                        if (!string.IsNullOrEmpty(cnEmail_db.Value))
                        {
                            cnEmailList = cnEmail_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var cnEmailCc_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmailCc);
                    var cnEmailCcList = new List<string>();
                    if (cnEmailCc_db != null)
                    {
                        if (!string.IsNullOrEmpty(cnEmailCc_db.Value))
                        {
                            cnEmailCcList = cnEmailCc_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var peEmail_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmail);
                    var peEmailList = new List<string>();
                    if (peEmail_db != null)
                    {
                        if (!string.IsNullOrEmpty(peEmail_db.Value))
                        {
                            peEmailList = peEmail_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var peEmailCc_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmailCc);
                    var peEmailCcList = new List<string>();
                    if (peEmailCc_db != null)
                    {
                        if (!string.IsNullOrEmpty(peEmailCc_db.Value))
                        {
                            peEmailCcList = peEmailCc_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var propertyEmail_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmail);
                    var propertyEmailList = new List<string>();
                    if (propertyEmail_db != null)
                    {
                        if (!string.IsNullOrEmpty(propertyEmail_db.Value))
                        {
                            propertyEmailList = propertyEmail_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var propertyEmailCc_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmailCc);
                    var propertyEmailCcList = new List<string>();
                    if (propertyEmailCc_db != null)
                    {
                        if (!string.IsNullOrEmpty(propertyEmailCc_db.Value))
                        {
                            propertyEmailCcList = propertyEmailCc_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var loanEmail_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmail);
                    var loanEmailList = new List<string>();
                    if (loanEmail_db != null)
                    {
                        if (!string.IsNullOrEmpty(loanEmail_db.Value))
                        {
                            loanEmailList = loanEmail_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var loanEmailCc_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmailCc);
                    var loanEmailCcList = new List<string>();
                    if (loanEmailCc_db != null)
                    {
                        if (!string.IsNullOrEmpty(loanEmailCc_db.Value))
                        {
                            loanEmailCcList = loanEmailCc_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var fcaTaggingEmail_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_FcaTagging);
                    var fcaTaggingEmailList = new List<string>();
                    if (fcaTaggingEmail_db != null)
                    {
                        if (!string.IsNullOrEmpty(fcaTaggingEmail_db.Value))
                        {
                            fcaTaggingEmailList = fcaTaggingEmail_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }

                    var approvedTreasuryEmail_db =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_TreasuryApproval);
                    var approvedTreasuryEmailList = new List<string>();
                    if (approvedTreasuryEmail_db != null)
                    {
                        if (!string.IsNullOrEmpty(approvedTreasuryEmail_db.Value))
                        {
                            approvedTreasuryEmailList = approvedTreasuryEmail_db.Value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        }
                    }


                    NotificationConfigViewModel viewModel = new NotificationConfigViewModel()
                    {
                        InflowFundCutOffTime = tryParseValue,
                        tsCnEmail = cnEmailList,
                        tsCnEmailCc = cnEmailCcList,
                        tsPeEmail = peEmailList,
                        tsPeEmailCc = peEmailCcList,
                        tsPropertyEmail = propertyEmailList,
                        tsPropertyEmailCc = propertyEmailCcList,
                        tsLoanEmail = loanEmailList,
                        tsLoanEmailCc = loanEmailCcList,
                        tsFcaTaggingEmail = fcaTaggingEmailList,
                        tsApprovedTreasury = approvedTreasuryEmailList
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