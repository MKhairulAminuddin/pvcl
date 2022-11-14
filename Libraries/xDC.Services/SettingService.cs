using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.Setting;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services
{
    public static class SettingService
    {
        public static EmailConfigSetting EmailConfiguration()
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

                    var fid_TSubmissionEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Submitted_Cc);
                    var fid_TApprovedEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Approved_Cc);
                    var fid_TSApprovedEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_TS_Approved_Cc);

                    var fid_Enable_TSubmissionEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Submitted_Cc);
                    var fid_Enable_TApprovedEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Approved_Cc);
                    var fid_Enable_TSApprovedEmailCc = appConfigs.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_TS_Approved_Cc);


                    var viewModel = new EmailConfigSetting()
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


                        fid_T_SubmittedCc = fid_TSubmissionEmailCc.Value,
                        fid_T_ApprovedCc = fid_TApprovedEmailCc.Value,
                        fid_TS_ApprovedCc = fid_TSApprovedEmailCc.Value,

                        fid_Enable_T_SubmittedCc = Convert.ToBoolean(fid_Enable_TSubmissionEmailCc?.Value),
                        fid_Enable_T_ApprovedCc = Convert.ToBoolean(fid_Enable_TApprovedEmailCc?.Value),
                        fid_Enable_TS_ApprovedCc = Convert.ToBoolean(fid_Enable_TSApprovedEmailCc?.Value)
                    };

                    return viewModel;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public static bool EmailConfigurationUpdate(EmailConfigSetting req, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    if (req != null)
                    {
                        var config = db.Config_Application.ToList();

                        var issd_Ts_CnEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_CnEmail), req.issd_TsCn, currentUser);
                        var issd_Ts_CnEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_CnEmail_Cc), req.issd_TsCnCc, currentUser);
                        var issd_Ts_PeEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PeEmail), req.issd_TsPe, currentUser);
                        var issd_Ts_PeEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PeEmail_Cc), req.issd_TsPeCc, currentUser);
                        var issd_Ts_PropertyEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PropertyEmail), req.issd_TsProperty, currentUser);
                        var issd_Ts_PropertyEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PropertyEmail_Cc), req.issd_TsPropertyCc, currentUser);
                        var issd_Ts_LoanEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_LoanEmail), req.issd_TsLoan, currentUser);
                        var issd_Ts_LoanEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_LoanEmail_Cc), req.issd_TsLoanCc, currentUser);
                        var issd_FcaTaggingEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_FcaTagging), req.issd_FcaTagging, currentUser);
                        var issd_T_ApprovedEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_T_Approval), req.issd_TApproved, currentUser);

                        var enable_issd_Ts_cnEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_CnEmail), req.issd_Enable_TsCn, currentUser);
                        var enable_issd_Ts_cnEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_CnEmail_Cc), req.issd_Enable_TsCnCc, currentUser);
                        var enable_issd_Ts_peEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PeEmail), req.issd_Enable_TsPe, currentUser);
                        var enable_issd_Ts_peEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PeEmail_Cc), req.issd_Enable_TsPeCc, currentUser);
                        var enable_issd_Ts_propertyEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PropertyEmail), req.issd_Enable_TsProperty, currentUser);
                        var enable_issd_Ts_propertyEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PropertyEmail_Cc), req.issd_Enable_TsPropertyCc, currentUser);
                        var enable_issd_Ts_loanEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_LoanEmail), req.issd_Enable_TsLoan, currentUser);
                        var enable_issd_Ts_loanEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_LoanEmail_Cc), req.issd_Enable_TsLoanCc, currentUser);
                        var enable_issd_fcaTagging = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_FcaTagging), req.issd_Enable_FcaTagging, currentUser);
                        var enable_issd_T_ApprovalEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_T_Approval), req.issd_Enable_TApproved, currentUser);


                        var fid_T_ApprovalCc = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Approved_Cc), req.fid_T_ApprovedCc, currentUser);
                        var fid_T_SubmissionCc = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Submitted_Cc), req.fid_T_SubmittedCc, currentUser);
                        var fid_TS_ApprovedCc = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_TS_Approved_Cc), req.fid_TS_ApprovedCc, currentUser);

                        var enable_fid_T_ApprovalCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Approved_Cc), req.fid_Enable_T_ApprovedCc, currentUser);
                        var enable_fid_T_SubmissionCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Submitted_Cc), req.fid_Enable_T_SubmittedCc, currentUser);
                        var enable_fid_TS_ApprovedCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_TS_Approved_Cc), req.fid_Enable_TS_ApprovedCc, currentUser);

                        db.SaveChanges();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        private static Config_Application UpdateEnableEmailNotiConfigValue(Config_Application itemToUpdate, bool updatedValue, string updatedBy)
        {
            if (itemToUpdate != null)
            {
                itemToUpdate.Value = updatedValue.ToString().ToLower();
                itemToUpdate.UpdatedBy = updatedBy;
                itemToUpdate.UpdatedDate = DateTime.Now;
                return itemToUpdate;
            }
            else
            {
                return itemToUpdate;
            }
        }

        private static Config_Application UpdateEmailNotiConfigValue(Config_Application itemToUpdate, string updatedValue, string updatedBy)
        {
            if (itemToUpdate != null)
            {
                var emailString = String.Join(",", updatedValue ?? null);
                if (itemToUpdate.Value != emailString)
                {
                    itemToUpdate.Value = emailString;
                    itemToUpdate.UpdatedBy = updatedBy;
                    itemToUpdate.UpdatedDate = DateTime.Now;
                }
                return itemToUpdate;
            }
            else
            {
                return itemToUpdate;
            }
        }

    }
}
