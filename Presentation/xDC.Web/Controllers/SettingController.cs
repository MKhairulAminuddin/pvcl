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
                    
                    var cnEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail);
                    var cnEmailCcList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmailCc);
                    var peEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmail);
                    var peEmailCcList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmailCc);
                    var propertyEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmail);
                    var propertyEmailCcList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmailCc);
                    var loanEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmail);
                    var loanEmailCcList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmailCc);
                    var fcaTaggingEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_FcaTagging);
                    var approvedTreasuryEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_TreasuryApproval);

                    var cnEmailEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail_Enable);
                    var cnEmailCcEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmailCc_Enable);
                    var peEmailEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmail_Enable);
                    var peEmailCcEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmailCc_Enable);
                    var propertyEmailEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmail_Enable);
                    var propertyEmailCcEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmailCc_Enable);
                    var loanEmailEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmail_Enable);
                    var loanEmailCcEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmailCc_Enable);
                    var fcaTaggingEmailEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_FcaTagging_Enable);
                    var approvedTreasuryEmailEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_TreasuryApproval_Enable);

                    var tSubmissionEmailCcList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasurySubmissionCc);
                    var tApprovedCcEmailList = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasuryApprovalCc);

                    var tSubmissionEmailCcEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasurySubmissionCc_Enable);
                    var tApprovedEmailCcEnable = appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasuryApprovalCc_Enable);


                    NotificationConfigViewModel viewModel = new NotificationConfigViewModel()
                    {
                        InflowFundCutOffTime = tryParseValue,
                        tsCnEmail = cnEmailList?.Value,
                        tsCnEmailCc = cnEmailCcList?.Value,
                        tsPeEmail = peEmailList?.Value,
                        tsPeEmailCc = peEmailCcList?.Value,
                        tsPropertyEmail = propertyEmailList?.Value,
                        tsPropertyEmailCc = propertyEmailCcList?.Value,
                        tsLoanEmail = loanEmailList?.Value,
                        tsLoanEmailCc = loanEmailCcList?.Value,
                        tsFcaTaggingEmail = fcaTaggingEmailList?.Value,
                        tsApprovedTreasury = approvedTreasuryEmailList?.Value,
                        tSubmissionEmailCc = tSubmissionEmailCcList?.Value,
                        tApprovedEmailCc = tApprovedCcEmailList?.Value,

                        tsCnEmailEnable = Convert.ToBoolean(cnEmailEnable?.Value),
                        tsCnEmailCcEnable = Convert.ToBoolean(cnEmailCcEnable?.Value),
                        tsPeEmailEnable = Convert.ToBoolean(peEmailEnable?.Value),
                        tsPeEmailCcEnable = Convert.ToBoolean(peEmailCcEnable?.Value),
                        tsPropertyEmailEnable = Convert.ToBoolean(propertyEmailEnable?.Value),
                        tsPropertyEmailCcEnable = Convert.ToBoolean(propertyEmailCcEnable?.Value),
                        tsLoanEmailEnable = Convert.ToBoolean(loanEmailEnable?.Value),
                        tsLoanEmailCcEnable = Convert.ToBoolean(loanEmailCcEnable?.Value),
                        tsFcaTaggingEmailEnable = Convert.ToBoolean(fcaTaggingEmailEnable?.Value),
                        tsApprovedTreasuryEnable = Convert.ToBoolean(approvedTreasuryEmailEnable?.Value),
                        tSubmissionEmailCcEnable = Convert.ToBoolean(tSubmissionEmailCcEnable?.Value),
                        tApprovedEmailCcEnable = Convert.ToBoolean(tApprovedEmailCcEnable?.Value)
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