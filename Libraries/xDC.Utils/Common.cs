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
        public static DocumentFormat ConvertIndexToFormat(int value)
        {
            switch (value)
            {
                case 1:
                    return DocumentFormat.Xlsx;
                case 2:
                    return DocumentFormat.Xlsm;
                case 3:
                    return DocumentFormat.Xlsb;
                case 4:
                    return DocumentFormat.Xls;
                case 5:
                    return DocumentFormat.Xltx;
                case 6:
                    return DocumentFormat.Xltm;
                case 7:
                    return DocumentFormat.Xlt;
                case 8:
                    return DocumentFormat.XmlSpreadsheet2003;
                case 9:
                    return DocumentFormat.Csv;
                case 10:
                    return DocumentFormat.Text;
            }
            return DocumentFormat.Undefined;
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

        public static string ConvertIndexToFileExtension(int value)
        {
            switch (value)
            {
                case 1:
                    return "xlsx";
                case 2:
                    return "xlsm";
                case 3:
                    return "xlsb";
                case 4:
                    return "xls";
                case 5:
                    return "xltx";
                case 6:
                    return "xltm";
                case 7:
                    return "xlt";
                case 8:
                    return "xml";
                case 9:
                    return "csv";
                case 10:
                    return "txt";
                case 11:
                    return "pdf";
                case 12:
                    return "html";
            }
            return String.Empty;
        }

        /// <summary>
        /// Get Form Status Mapping
        /// </summary>
        /// <param name="value">
        /// numbers representing the statuses. 0 Draft, 1 Preparation, 2 Pending Approval, 3 Approved, 4 Rejected, 5 Resubmission (used for workflow only)
        /// </param>
        /// <returns>string</returns>
        public static string FormStatusMapping(int value)
        {
            switch (value)
            {
                case 0:
                    return "Draft";
                case 1:
                    return "Preparation";
                case 2:
                    return "Pending Approval";
                case 3:
                    return "Approved";
                case 4:
                    return "Rejected";
                case 5:
                    return "Pending Approval (Resubmission)";
            }
            return String.Empty;
        }

        public static string TradeSettlementMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return "EQUITY";
                case 2:
                    return "BOND";
                case 3:
                    return "COMMERCIAL PAPER";
                case 4:
                    return "NOTES AND PAPERS";
                case 5:
                    return "REPO";
                case 6:
                    return "COUPON";
                case 7:
                    return "FEES";
                case 8:
                    return "PAYMENT/RECEIVED (MTM)";
                case 9:
                    return "FX SETTLEMENT";
                case 10:
                    return "CONTRIBUTION CREDITED";
                case 11:
                    return "ALTID DISTRIBUTION AND DRAWDOWN";
                case 12:
                    return "OTHERS";
            }
            return String.Empty;
        }

        public static string TradeSettlementUrlParamMapping(string value)
        {
            switch (value)
            {
                case "equity":
                    return "Equity";
                case "bond":
                    return "Bond";
                case "cp":
                    return "CP";
                case "notesPaper":
                    return "Notes & Papers";
                case "repo":
                    return "REPO";
                case "coupon":
                    return "Coupon Received";
                case "fees":
                    return "Fees";
                case "mtm":
                    return "Payment/ Received (MTM)";
                case "fxSettlement":
                    return "FX Settlement";
                case "contributionCredited":
                    return "Contribution Credited";
                case "altid":
                    return "ALTID Distribution & Drawdown";
                case "others":
                    return "Others";
            }
            return String.Empty;
        }

        public static string FormTypeMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return "Inflow Funds";
                case 2:
                    return "Trade Settlement";
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

        public static string DropdownConfigKeyMapping(int value)
        {
            switch (value)
            {
                case 1:
                    return "Amsd.InflowFunds.FundType";
                case 2:
                    return "Amsd.InflowFunds.Bank";
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
