using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Spreadsheet;

namespace xDC.Utils
{
    public static class Common
    {
        public static class TsItemCategory
        {
            public const string Equity = "EQUITY";
            public const string Bond = "BOND";
            public const string Cp = "COMMERCIAL PAPER";
            public const string NotesPapers = "NOTES AND PAPERS";
            public const string Repo = "REPO";
            public const string Coupon = "COUPON";
            public const string Fees = "FEES";
            public const string Mtm = "PAYMENT/RECEIVED (MTM)";
            public const string Fx = "FX SETTLEMENT";
            public const string Cn = "CONTRIBUTION CREDITED";
            public const string Altid = "ALTID DISTRIBUTION AND DRAWDOWN";
            public const string Others = "OTHERS";
        }

        public static class FormStatus
        {
            public const string Draft = "Draft";
            public const string PendingApproval = "Pending Approval";
            public const string PendingApprovalResubmission = "Pending Approval (Resubmission)";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
        }

        public static class FormType
        {
            public const string AMSD_IF = "Inflow Fund";
            public const string ISSD_TS = "Trade Settlement";
            public const string ISSD_TS_A = "Trade Settlement (Part A)";
            public const string ISSD_TS_B = "Trade Settlement (Part B)";
            public const string ISSD_TS_C = "Trade Settlement (Part C)";
            public const string ISSD_TS_D = "Trade Settlement (Part D)";
            public const string ISSD_TS_E = "Trade Settlement (Part E)";
            public const string ISSD_TS_F = "Trade Settlement (Part F)";
            public const string ISSD_TS_G = "Trade Settlement (Part G)";
            public const string ISSD_TS_H = "Trade Settlement (Part H)";
            public const string FID_TREASURY = "Treasury";
        }

        public static class TsOthersTypeItem
        {
            public const string Loan = "Loan";
            public const string Property = "Property";
            public const string Others = "Others";
        }

        public static class TsCouponTypeItem
        {
            public const string GOV = "GOV";
            public const string CORP = "CORP";
        }

        public static class TsBondTypeItem
        {
            public const string GOV = "GOV";
            public const string CORP = "CORP";
        }

        public static class Cashflow
        {
            public const string Inflow = "INFLOW";
            public const string Outflow = "OUTFLOW";
        }

        public static class TsEmailNotificationType
        {
            public const string Contribution = "Contribution";
            public const string PE = "PE";
            public const string Load = "Loan";
            public const string Property = "Property";
            public const string CouponBond = "Coupon - Bond";
        }

        public static class ExcelTemplateLocation
        {
            public const string AMSD_IF = "~/App_Data/Inflow Funds Template.xltx";
            public const string ISSD_TS = "~/App_Data/Trade Settlement Template.xltx";
            public const string ISSD_TS_Consolidated = "~/App_Data/Trade Settlement Template - Consolidated.xltx";
            public const string FID_Treasury = "~/App_Data/Treasury Template.xltx";
            public const string FID_DealCutOff_MYR = "~/App_Data/Deal Cut Off MYR Template.xltx";
            public const string FID_DealCutOff_FCY = "~/App_Data/Deal Cut Off FCY Template.xltx";
            public const string FID_TenAmDealCutOff = "~/App_Data/10am Deal Cut Off.xltx";
        }

        public static class DownloadedFileName
        {
            public const string AMSD_IF = "AMSD Inflow Fund Form - ";
            public const string ISSD_TS = "ISSD Trade Settlement - ";
            public const string ISSD_TS_Consolidated = "ISSD Trade Settlement - ";
            public const string FID_Treasury = "FID Treasury - ";
            public const string FID_DealCutOff_MYR = "FID Deal Cut Off MYR - ";
            public const string FID_DealCutOff_FCY = "FID Deal Cut Off FCY - ";
            public const string FID_TenAmDealCutOff = "10am Deal Cut Off - ";
        }

        public static string ConvertIndexToContentType(int value)
        {
            switch (value)
            {
                case 1:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case 2:
                    return "application/vnd.ms-excel.sheet.macroEnabled.12";
                case 3:
                    return "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
                case 4:
                    return "application/vnd.ms-excel";
                case 5:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                case 6:
                    return "application/vnd.ms-excel.template.macroEnabled.12";
                case 7:
                    return "application/vnd.ms-excel";
                case 8:
                    return "text/xml";
                case 9:
                    return "text/comma-separated-values";
                case 10:
                    return "text/plain";
                case 11:
                    return "application/pdf";
                case 12:
                    return "text/html";
            }
            return String.Empty;
        }
        
        public static string TsCategoryUrlParamMapping(string value)
        {
            switch (value)
            {
                case "equity":
                    return TsItemCategory.Equity;
                case "bond":
                    return TsItemCategory.Bond;
                case "cp":
                    return TsItemCategory.Cp;
                case "notesPaper":
                    return TsItemCategory.NotesPapers;
                case "repo":
                    return TsItemCategory.Repo;
                case "coupon":
                    return TsItemCategory.Coupon;
                case "fees":
                    return TsItemCategory.Fees;
                case "mtm":
                    return TsItemCategory.Mtm;
                case "fxSettlement":
                    return TsItemCategory.Fx;
                case "contributionCredited":
                    return TsItemCategory.Cn;
                case "altid":
                    return TsItemCategory.Altid;
                case "others":
                    return TsItemCategory.Others;
            }
            return String.Empty;
        }
        
        public static string TsUrlParamInstrumentTypeMapFormType(string value)
        {
            var finalValue = string.Empty;
            switch (value)
            {
                case "equity":
                    finalValue = FormType.ISSD_TS_A;
                    break;
                case "bond":
                case "cp":
                case "notesPaper":
                case "repo":
                case "coupon":
                    finalValue = FormType.ISSD_TS_B;
                    break;
                case "mtm":
                case "fxSettlement":
                    finalValue = FormType.ISSD_TS_C;
                    break;
                case "altid":
                    finalValue = FormType.ISSD_TS_D;
                    break;
                case "fees":
                case "contributionCredited":
                case "others":
                    finalValue = FormType.ISSD_TS_E;
                    break;
            }
            return finalValue;
        }

        public static string TsReverseInstrumentTypeMapping(string value)
        {
            var finalValue = string.Empty;
            switch (value)
            {
                case TsItemCategory.Equity:
                    finalValue = "equity";
                    break;
                case TsItemCategory.Bond:
                    finalValue = "bond";
                    break;
                case TsItemCategory.Cp:
                    finalValue = "cp";
                    break;
                case TsItemCategory.NotesPapers:
                    finalValue = "notesPaper";
                    break;
                case TsItemCategory.Repo:
                    finalValue = "repo";
                    break;
                case TsItemCategory.Coupon:
                    finalValue = "coupon";
                    break;
                case TsItemCategory.Mtm:
                    finalValue = "mtm";
                    break;
                case TsItemCategory.Fx:
                    finalValue = "fxSettlement";
                    break;
                case TsItemCategory.Altid:
                    finalValue = "altid";
                    break;
                case TsItemCategory.Fees:
                    finalValue = "fees";
                    break;
                case TsItemCategory.Cn:
                    finalValue = "contributionCredited";
                    break;
                case TsItemCategory.Others:
                    finalValue = "others";
                    break;
            }
            return finalValue;
        }

        /// <summary>
        /// Get form type
        /// </summary>
        /// <param name="value">
        /// 1 Inflow Funds 2 Trade Settlement
        /// </param>
        /// <returns></returns>
        public static string FormTypeMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return FormType.AMSD_IF;
                case 2:
                    return FormType.ISSD_TS;
                case 3:
                    return FormType.ISSD_TS_A;
                case 4:
                    return FormType.ISSD_TS_B;
                case 5:
                    return FormType.ISSD_TS_C;
                case 6:
                    return FormType.ISSD_TS_D;
                case 7:
                    return FormType.ISSD_TS_E;
                case 8:
                    return FormType.ISSD_TS_F;
                case 9:
                    return FormType.ISSD_TS_G;
                case 10:
                    return FormType.ISSD_TS_H;
                case 11:
                    return FormType.FID_TREASURY;
            }
            return String.Empty;
        }
        
        /// <summary>
        /// Get form view url
        /// </summary>
        /// <param name="formType">
        /// woi
        /// </param>
        /// <returns></returns>
        public static string PushNotification_FormUrlMap(string formType)
        {
            switch (formType)
            {
                case FormType.AMSD_IF:
                    return "../amsd/InflowFund/View/";

                case FormType.ISSD_TS_A:
                    return "../issd/TradeSettlement/PartA/View/";
                case FormType.ISSD_TS_B:
                    return "../issd/TradeSettlement/PartB/View/";
                case FormType.ISSD_TS_C:
                    return "../issd/TradeSettlement/PartC/View/";
                case FormType.ISSD_TS_D:
                    return "../issd/TradeSettlement/PartD/View/";
                case FormType.ISSD_TS_E:
                    return "../issd/TradeSettlement/PartE/View/";
                case FormType.ISSD_TS_F:
                    return "../issd/TradeSettlement/PartF/View/";
                case FormType.ISSD_TS_G:
                    return "../issd/TradeSettlement/PartG/View/";
                case FormType.ISSD_TS_H:
                    return "../issd/TradeSettlement/PartH/View/";

                case FormType.FID_TREASURY:
                    return "../fid/Treasury/View/";
            }
            return String.Empty;
        }

        /// <summary>
        /// Get form view url
        /// </summary>
        /// <param name="formType">
        /// woi
        /// </param>
        /// <returns></returns>
        public static string  Email_FormUrlMap(string formType)
        {
            switch (formType)
            {
                case FormType.AMSD_IF:
                    return "/amsd/InflowFund/View/";
                case FormType.ISSD_TS_A:
                    return "/issd/TradeSettlement/PartA/View/";
                case FormType.ISSD_TS_B:
                    return "/issd/TradeSettlement/PartB/View/";
                case FormType.ISSD_TS_C:
                    return "/issd/TradeSettlement/PartC/View/";
                case FormType.ISSD_TS_D:
                    return "/issd/TradeSettlement/PartD/View/";
                case FormType.ISSD_TS_E:
                    return "/issd/TradeSettlement/PartE/View/";
                case FormType.ISSD_TS_F:
                    return "/issd/TradeSettlement/PartF/View/";
                case FormType.ISSD_TS_G:
                    return "/issd/TradeSettlement/PartG/View/";
                case FormType.ISSD_TS_H:
                    return "/issd/TradeSettlement/PartH/View/";

                case FormType.FID_TREASURY:
                    return "/fid/Treasury/View/";
            }
            return String.Empty;
        }

        public static class ConfigKeyType
        {
            public const string Dropdown = "Dropdown";
            public const string Application = "Application";
        }

        public static class DropdownConfigKey
        {
            public const string AMSD_IF_FundType = "AMSD.InflowFunds.FundType";
            public const string AMSD_IF_Bank = "AMSD.InflowFunds.Bank";

            public const string ISSD_TS_Currency = "ISSD.TradeSettlement.Currency";

            public const string FID_Treasury_Notes = "FID.Treasury.Notes";
            public const string FID_Treasury_AssetType = "FID.Treasury.AssetType";
            public const string FID_Treasury_ProductType = "FID.Treasury.ProductType";
        }

        public static class AppConfigKey
        {
            public const string AMSD_IF_CutOffTime = "Amsd.InflowFunds.CutOffTime";
            public const string AMSD_IF_Notification = "Amsd.InflowFunds.Notification";

            public const string ISSD_TS_CnEmail = "Issd.TradeSettlement.ContributionEmail";
        }

        public static class  CutOffViolationAction
        {
            public const string Approval = "Approval";
            public const string Submission = "Submission";
        }

        public static string GetFileExt(FileStream fs)
        {
            var ext = Path.GetExtension(fs.Name);
            return ext.ToLower();
        }

        public static string GetFileName(FileStream fs)
        {
            var fileName = Path.GetFileName(fs.Name);
            return fileName;
        }

        public static string GetSystemTempFolder()
        {
            var tempFolder = Config.TempFolderPath;
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            return tempFolder;
        }

        public static string GetSystemTempFilePath(string filenameWithPath)
        {
            var tempFolder = Config.TempFolderPath;
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            tempFolder = Path.Combine(tempFolder, filenameWithPath);

            return tempFolder;
        }

        public static DateTime? ConvertEpochToDateTime(long unixTime, int addDays = 0)
        {
            
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(addDays);
            var result = epoch.ToLocalTime().AddSeconds(unixTime);
            return result;
        }

        /// <summary>
        /// https://stackoverflow.com/a/36476600/2343
        /// </summary>
        public class Table : IDisposable
        {
            private StringBuilder _sb;

            public Table(StringBuilder sb, string id = "default", string classValue = "")
            {
                _sb = sb;
                _sb.Append($"<table id=\"{id}\" style='border-collapse: collapse;' class=\"{classValue}\">\n");
            }

            public void Dispose()
            {
                _sb.Append("</table>");
            }

            public Row AddRow()
            {
                return new Row(_sb);
            }

            public Row AddHeaderRow(string headerColor = null, string fontColor = null)
            {
                return new Row(_sb, true, headerColor, fontColor);
            }

            public void StartTableBody()
            {
                _sb.Append("<tbody>");

            }

            public void EndTableBody()
            {
                _sb.Append("</tbody>");

            }
        }

        public class Row : IDisposable
        {
            private StringBuilder _sb;
            private bool _isHeader;
            public Row(StringBuilder sb, bool isHeader = false, string headerColor = null, string fontColor = null)
            {
                _sb = sb;
                _isHeader = isHeader;
                if (_isHeader)
                {
                    _sb.Append("<thead style='border: 1px solid #999;padding: 8px;font-weight: bold;" +
                               (!string.IsNullOrEmpty(headerColor) ? "background-color:" + headerColor + ";" : "") +
                               (!string.IsNullOrEmpty(fontColor) ? "color:" + fontColor : "") +
                               "'>\n");
                }
                _sb.Append("\t<tr>\n");
            }

            public void Dispose()
            {
                _sb.Append("\t</tr>\n");
                if (_isHeader)
                {
                    _sb.Append("</thead>\n");
                }
            }

            public void AddCell(string innerText)
            {
                _sb.Append("\t\t<td style='border: 1px solid #999;padding: 8px;'>\n");
                _sb.Append("\t\t\t" + innerText);
                _sb.Append("\t\t</td>\n");
            }

            public void AddCell_IntegerType(string innerText, bool isGreen)
            {
                if (isGreen)
                {
                    _sb.Append(innerText == "0.00"
                        ? "\t\t<td style='border: 1px solid #999;padding: 8px;text-align: right;width:180px;'>\n"
                        : "\t\t<td style='border: 1px solid #999;padding: 8px;text-align: right;width:180px;color:#16A085'>\n");
                }
                else
                {
                    _sb.Append(innerText == "0.00"
                        ? "\t\t<td style='border: 1px solid #999;padding: 8px;text-align: right;width:180px;'>\n"
                        : "\t\t<td style='border: 1px solid #999;padding: 8px;text-align: right;width:180px;color:#E74C3C'>\n");
                }
                
                _sb.Append("\t\t\t" + innerText);
                _sb.Append("\t\t</td>\n");
            }
        }

        public static string MailSubjectWithDate(string subject)
        {
            return $"{subject} [{DateTime.Now:dd/MM/yyyy}]";
        }

        public static bool IsTsFormType(string formType)
        {
            return formType == FormType.ISSD_TS_A || formType == FormType.ISSD_TS_B || formType == FormType.ISSD_TS_C || 
                   formType == FormType.ISSD_TS_D || formType == FormType.ISSD_TS_E || formType == FormType.ISSD_TS_F || 
                   formType == FormType.ISSD_TS_G || formType == FormType.ISSD_TS_H;
        }

        #region Audit

        public static class ActionType
        {
            public const string Create = "CREATE";
            public const string Modify = "MODIFY";
            public const string Delete = "DELETE";
            public const string Approve = "APPROVE";
            public const string Reject = "REJECT";
            public const string Reassign = "REASSIGN";
            public const string RequestApproval = "REQUEST APPROVAL";
        }

        public static string FlattenStrings(params string[] parameters)
        {
            var result = string.Empty;

            foreach (var item in parameters)
            {
                result.Concat(item.ToString() + " | ");
            }

            return result;
        }
        #endregion
    }
}
