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
            }
            return String.Empty;
        }
        

        public static string FormCurrencyMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return "MYR";
                case 2:
                    return "USD";
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
        public static string FormUrlViewMapping(string formType)
        {
            switch (formType)
            {
                case "Inflow Funds":
                    return "../amsd/InflowFundsFormStatus?id=";

                case "Trade Settlement (Part A)":
                    return "../issd/TradeSettlement/PartA/View/";
                case "Trade Settlement (Part B)":
                    return "../issd/TradeSettlement/PartB/View/";
                case "Trade Settlement (Part C)":
                    return "../issd/TradeSettlement/PartC/View/";
                case "Trade Settlement (Part D)":
                    return "../issd/TradeSettlement/PartD/View/";
                case "Trade Settlement (Part E)":
                    return "../issd/TradeSettlement/PartE/View/";
                case "Trade Settlement (Part F)":
                    return "../issd/TradeSettlement/PartF/View/";
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
        public static string FormUrlViewMappingForEmailNotification(string formType)
        {
            switch (formType)
            {
                case "Inflow Funds":
                    return "/amsd/InflowFundsFormStatus?id=";

                case "Trade Settlement (Part A)":
                    return "/issd/TradeSettlement/PartA/View/";
                case "Trade Settlement (Part B)":
                    return "/issd/TradeSettlement/PartB/View/";
                case "Trade Settlement (Part C)":
                    return "/issd/TradeSettlement/PartC/View/";
                case "Trade Settlement (Part D)":
                    return "/issd/TradeSettlement/PartD/View/";
                case "Trade Settlement (Part E)":
                    return "/issd/TradeSettlement/PartE/View/";
                case "Trade Settlement (Part F)":
                    return "/issd/TradeSettlement/PartF/View/";
            }
            return String.Empty;
        }

        public static string DropdownConfigKeyMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return "Amsd.InflowFunds.FundType";
                case 2:
                    return "Amsd.InflowFunds.Bank";
                case 3:
                    return "ISSD.TradeSettlement.Currency";
            }
            return String.Empty;
        }

        public static string ApplicationConfigKeyMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return "Amsd.InflowFunds.CutOffTime";
                case 2:
                    return "Amsd.InflowFunds.Notification";
                case 3:
                    return "Amsd.InflowFunds.AdminEditNotification";
                case 4:
                    return "Issd.TradeSettlement.ContributionEmail";


            }
            return String.Empty;
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
    }
}
