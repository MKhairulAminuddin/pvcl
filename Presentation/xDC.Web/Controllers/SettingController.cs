using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
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
                using (var db = new kashflowDBEntities())
                {
                    var appConfigs = db.Config_Application.ToList();
                    
                    var cutOffTimeValue =
                        appConfigs.FirstOrDefault(x => x.Key == Common.AppConfigKey.AMSD_IF_CutOffTime);
                    DateTime.TryParseExact(cutOffTimeValue.Value, "HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var tryParseValue);
                    
                    var issd_TsCnEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_CnEmail);
                    var issd_TsCnEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_CnEmail_Cc);
                    var issd_TsPeEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PeEmail);
                    var issd_TsPeEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PeEmail_Cc);
                    var issd_TsPropertyEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PropertyEmail);
                    var issd_TsPropertyEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PropertyEmail_Cc);
                    var issd_TsLoanEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_LoanEmail);
                    var issd_TsLoanEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_LoanEmail_Cc);
                    var issd_FcaTaggingEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_FcaTagging);
                    var issd_TApprovedEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_T_Approval);

                    var issd_Enable_TsCnEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_CnEmail);
                    var issd_Enable_TsCnEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_CnEmail_Cc);
                    var issd_Enable_TsPeEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PeEmail);
                    var issd_Enable_TsPeEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PeEmail_Cc);
                    var issd_Enable_TsPropertyEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PropertyEmail);
                    var issd_Enable_TsPropertyEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PropertyEmail_Cc);
                    var issd_Enable_TsLoanEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_LoanEmail);
                    var issd_Enable_TsLoanEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_LoanEmail_Cc);
                    var issd_Enable_FcaTaggingEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_FcaTagging);
                    var issd_Enable_TApprovedEmail = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_T_Approval);

                    var fid_TSubmissionEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Submission_Cc);
                    var fid_TApprovedEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Approval_Cc);

                    var fid_Enable_TSubmissionEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Submission_Cc);
                    var fid_Enable_TApprovedEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Approval_Cc);


                    NotificationConfigViewModel viewModel = new NotificationConfigViewModel()
                    {
                        InflowFundCutOffTime = tryParseValue,

                        issd_TsCn = issd_TsCnEmail.Value,
                        issd_TsCnCc = issd_TsCnEmailCc.Value,
                        issd_TsPe = issd_TsPeEmail.Value,
                        issd_TsPeCc = issd_TsPeEmailCc.Value,
                        issd_TsProperty = issd_TsPropertyEmail.Value,
                        issd_TsPropertyCc = issd_TsPropertyEmailCc.Value,
                        issd_TsLoan = issd_TsLoanEmail.Value,
                        issd_TsLoanCc = issd_TsLoanEmailCc.Value,
                        issd_FcaTagging = issd_FcaTaggingEmail.Value,
                        issd_TApproved = issd_TApprovedEmail.Value,

                        issd_Enable_TsCn = Convert.ToBoolean(issd_Enable_TsCnEmail?.Value),
                        issd_Enable_TsCnCc = Convert.ToBoolean(issd_Enable_TsCnEmailCc?.Value),
                        issd_Enable_TsPe = Convert.ToBoolean(issd_Enable_TsPeEmail?.Value),
                        issd_Enable_TsPeCc = Convert.ToBoolean(issd_Enable_TsPeEmailCc?.Value),
                        issd_Enable_TsProperty = Convert.ToBoolean(issd_Enable_TsPropertyEmail?.Value),
                        issd_Enable_TsPropertyCc = Convert.ToBoolean(issd_Enable_TsPropertyEmailCc?.Value),
                        issd_Enable_TsLoan = Convert.ToBoolean(issd_Enable_TsLoanEmail?.Value),
                        issd_Enable_TsLoanCc = Convert.ToBoolean(issd_Enable_TsLoanEmailCc?.Value),
                        issd_Enable_FcaTagging = Convert.ToBoolean(issd_Enable_FcaTaggingEmail?.Value),
                        issd_Enable_TApproved = Convert.ToBoolean(issd_Enable_TApprovedEmail?.Value),


                        fid_T_SubmissionCc = fid_TSubmissionEmailCc.Value,
                        fid_T_ApprovalCc = fid_TApprovedEmailCc.Value,

                        fid_Enable_T_SubmissionCc = Convert.ToBoolean(fid_Enable_TSubmissionEmailCc?.Value),
                        fid_Enable_T_ApprovalCc = Convert.ToBoolean(fid_Enable_TApprovedEmailCc?.Value)
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