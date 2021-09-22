using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using DevExpress.Spreadsheet;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;

namespace xDC_Web.Extension.DocGenerator
{
    public class DealCutOffForm_FCY : DocGeneratorBase
    {
        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#3498DB");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#E67E22");

        public IWorkbook GenerateWorkbook(DateTime selectedDate)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var dataObj = ConstructData(db, selectedDate);

                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_DealCutOff_FCY));
                    workbook = GenerateDocument(workbook, dataObj);

                    return workbook;

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public string GenerateFile(DateTime selectedDate, bool isExportAsExcel)
        {
            try
            {
                IWorkbook workbook = GenerateWorkbook(selectedDate);
                var randomFileName = Common.DownloadedFileName.FID_DealCutOff_FCY + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (isExportAsExcel)
                {
                    var documentFormat = DocumentFormat.Xlsx;
                    var tempFolder = Common.GetSystemTempFilePath(randomFileName + ".xlsx");
                    workbook.SaveDocument(tempFolder, documentFormat);
                }
                else
                {
                    var tempFolder = Common.GetSystemTempFilePath(randomFileName + ".pdf");
                    workbook.ExportToPdf(tempFolder);
                }

                return randomFileName;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        private IWorkbook GenerateDocument(IWorkbook workbook, FCY_DealCutOffData dataItem)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                foreach (var item in dataItem.Accounts)
                {
                    if (item.Account == "Maybank MFCA")
                    {
                        sheet["F2"].Value = dataItem.SelectedDate;

                        if (item.Currency == "USD")
                        {
                            sheet["F7"].Value = item.Ob;
                            sheet["F8"].Value = item.IF_DepositMaturity;
                            sheet["F9"].Value = item.IF_MoneyMarket;
                            sheet["F10"].Value = item.IF_Others;
                            sheet["F11"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F12"].Value = item.OF_MoneyMarket * -1;
                            sheet["F13"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "GBP")
                        {
                            sheet["F18"].Value = item.Ob;
                            sheet["F19"].Value = item.IF_DepositMaturity;
                            sheet["F20"].Value = item.IF_MoneyMarket;
                            sheet["F21"].Value = item.IF_Others;
                            sheet["F22"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F23"].Value = item.OF_MoneyMarket * -1;
                            sheet["F24"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "AUD")
                        {
                            sheet["F29"].Value = item.Ob;
                            sheet["F30"].Value = item.IF_DepositMaturity;
                            sheet["F31"].Value = item.IF_MoneyMarket;
                            sheet["F32"].Value = item.IF_Others;
                            sheet["F33"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F34"].Value = item.OF_MoneyMarket * -1;
                            sheet["F35"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "EUR")
                        {
                            sheet["F40"].Value = item.Ob;
                            sheet["F41"].Value = item.IF_DepositMaturity;
                            sheet["F42"].Value = item.IF_MoneyMarket;
                            sheet["F43"].Value = item.IF_Others;
                            sheet["F44"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F45"].Value = item.OF_MoneyMarket * -1;
                            sheet["F46"].Value = item.OF_Others * -1;
                        }
                    }

                    if (item.Account == "CITI MFCA")
                    {
                        sheet["M2"].Value = dataItem.SelectedDate;

                        if (item.Currency == "USD")
                        {
                            sheet["M7"].Value = item.Ob;
                            sheet["M8"].Value = item.IF_DepositMaturity;
                            sheet["M9"].Value = item.IF_MoneyMarket;
                            sheet["M10"].Value = item.IF_Others;
                            sheet["M11"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M12"].Value = item.OF_MoneyMarket * -1;
                            sheet["M13"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "GBP")
                        {
                            sheet["M18"].Value = item.Ob;
                            sheet["M19"].Value = item.IF_DepositMaturity;
                            sheet["M20"].Value = item.IF_MoneyMarket;
                            sheet["M21"].Value = item.IF_Others;
                            sheet["M22"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M23"].Value = item.OF_MoneyMarket * -1;
                            sheet["M24"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "AUD")
                        {
                            sheet["M29"].Value = item.Ob;
                            sheet["M30"].Value = item.IF_DepositMaturity;
                            sheet["M31"].Value = item.IF_MoneyMarket;
                            sheet["M32"].Value = item.IF_Others;
                            sheet["M33"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M34"].Value = item.OF_MoneyMarket * -1;
                            sheet["M35"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "EUR")
                        {
                            sheet["M40"].Value = item.Ob;
                            sheet["M41"].Value = item.IF_DepositMaturity;
                            sheet["M42"].Value = item.IF_MoneyMarket;
                            sheet["M43"].Value = item.IF_Others;
                            sheet["M44"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M45"].Value = item.OF_MoneyMarket * -1;
                            sheet["M46"].Value = item.OF_Others * -1;
                        }
                    }

                    if (item.Account == "Hong Leong Bank MFCA")
                    {
                        sheet["F52"].Value = dataItem.SelectedDate;

                        if (item.Currency == "USD")
                        {
                            sheet["F57"].Value = item.Ob;
                            sheet["F58"].Value = item.IF_DepositMaturity;
                            sheet["F59"].Value = item.IF_MoneyMarket;
                            sheet["F60"].Value = item.IF_Others;
                            sheet["F61"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F62"].Value = item.OF_MoneyMarket * -1;
                            sheet["F63"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "GBP")
                        {
                            sheet["F68"].Value = item.Ob;
                            sheet["F69"].Value = item.IF_DepositMaturity;
                            sheet["F70"].Value = item.IF_MoneyMarket;
                            sheet["F71"].Value = item.IF_Others;
                            sheet["F72"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F73"].Value = item.OF_MoneyMarket * -1;
                            sheet["F74"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "AUD")
                        {
                            sheet["F79"].Value = item.Ob;
                            sheet["F80"].Value = item.IF_DepositMaturity;
                            sheet["F81"].Value = item.IF_MoneyMarket;
                            sheet["F82"].Value = item.IF_Others;
                            sheet["F83"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F84"].Value = item.OF_MoneyMarket * -1;
                            sheet["F85"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "EUR")
                        {
                            sheet["F90"].Value = item.Ob;
                            sheet["F91"].Value = item.IF_DepositMaturity;
                            sheet["F92"].Value = item.IF_MoneyMarket;
                            sheet["F93"].Value = item.IF_Others;
                            sheet["F94"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F95"].Value = item.OF_MoneyMarket * -1;
                            sheet["F96"].Value = item.OF_Others * -1;
                        }
                    }

                    if (item.Account == "JP Morgan MFCA")
                    {
                        sheet["M52"].Value = dataItem.SelectedDate;

                        if (item.Currency == "USD")
                        {
                            sheet["M57"].Value = item.Ob;
                            sheet["M58"].Value = item.IF_DepositMaturity;
                            sheet["M59"].Value = item.IF_MoneyMarket;
                            sheet["M60"].Value = item.IF_Others;
                            sheet["M61"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M62"].Value = item.OF_MoneyMarket * -1;
                            sheet["M63"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "GBP")
                        {
                            sheet["M68"].Value = item.Ob;
                            sheet["M69"].Value = item.IF_DepositMaturity;
                            sheet["M70"].Value = item.IF_MoneyMarket;
                            sheet["M71"].Value = item.IF_Others;
                            sheet["M72"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M73"].Value = item.OF_MoneyMarket * -1;
                            sheet["M74"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "AUD")
                        {
                            sheet["M79"].Value = item.Ob;
                            sheet["M80"].Value = item.IF_DepositMaturity;
                            sheet["M81"].Value = item.IF_MoneyMarket;
                            sheet["M82"].Value = item.IF_Others;
                            sheet["M83"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M84"].Value = item.OF_MoneyMarket * -1;
                            sheet["M85"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "EUR")
                        {
                            sheet["M90"].Value = item.Ob;
                            sheet["M91"].Value = item.IF_DepositMaturity;
                            sheet["M92"].Value = item.IF_MoneyMarket;
                            sheet["M93"].Value = item.IF_Others;
                            sheet["M94"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["M95"].Value = item.OF_MoneyMarket * -1;
                            sheet["M96"].Value = item.OF_Others * -1;
                        }
                    }

                    if (item.Account == "CIMB FCA")
                    {
                        sheet["F102"].Value = dataItem.SelectedDate;

                        if (item.Currency == "USD")
                        {
                            sheet["F107"].Value = item.Ob;
                            sheet["F108"].Value = item.IF_DepositMaturity;
                            sheet["F109"].Value = item.IF_MoneyMarket;
                            sheet["F110"].Value = item.IF_Others;
                            sheet["F111"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F112"].Value = item.OF_MoneyMarket * -1;
                            sheet["F113"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "GBP")
                        {
                            sheet["F118"].Value = item.Ob;
                            sheet["F119"].Value = item.IF_DepositMaturity;
                            sheet["F120"].Value = item.IF_MoneyMarket;
                            sheet["F121"].Value = item.IF_Others;
                            sheet["F122"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F123"].Value = item.OF_MoneyMarket * -1;
                            sheet["F124"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "AUD")
                        {
                            sheet["F129"].Value = item.Ob;
                            sheet["F130"].Value = item.IF_DepositMaturity;
                            sheet["F131"].Value = item.IF_MoneyMarket;
                            sheet["F132"].Value = item.IF_Others;
                            sheet["F133"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F134"].Value = item.OF_MoneyMarket * -1;
                            sheet["F135"].Value = item.OF_Others * -1;
                        }

                        if (item.Currency == "EUR")
                        {
                            sheet["F140"].Value = item.Ob;
                            sheet["F141"].Value = item.IF_DepositMaturity;
                            sheet["F142"].Value = item.IF_MoneyMarket;
                            sheet["F143"].Value = item.IF_Others;
                            sheet["F144"].Value = item.OF_DepositPlacementRollover * -1;
                            sheet["F145"].Value = item.OF_MoneyMarket * -1;
                            sheet["F146"].Value = item.OF_Others * -1;
                        }
                    }
                }

                // details sheets
                foreach (var account in dataItem.Accounts)
                {
                    workbook.Worksheets.Add(account.Account);
                    workbook.Worksheets[account.Account].CopyFrom(workbook.Worksheets[1]);
                }


                workbook.Calculate();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

        private FCY_DealCutOffData ConstructData(kashflowDBEntities db, DateTime selectedDate)
        {
            try
            {
                var accounts = db.Config_FcaBankAccount.Select(x => x.AccountName1).Distinct().ToList();
                var currencies = new List<string>()
                {
                    "USD", "AUD", "GBP", "EUR"
                };

                var dataObj = new FCY_DealCutOffData()
                {
                    SelectedDate = selectedDate,
                    Accounts = new List<FCY_DealCutOffData_Account>()
                };

                foreach (var account in accounts)
                {
                    foreach (var currency in currencies)
                    {
                        dataObj.Accounts.Add(new FCY_DealCutOffData_Account()
                        {
                            Account = account,
                            Currency = currency
                        });
                    }
                }

                foreach (var item in dataObj.Accounts)
                {
                    var availableAccount2 = db.Config_FcaBankAccount
                        .Where(x => x.Currency == item.Currency && x.AccountName1 == item.Account)
                        .Select(x => x.AccountName2)
                        .ToList();

                    if (availableAccount2.Any())
                    {
                        var ob = db.EDW_BankBalance
                            .AsNoTracking()
                            .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                        && x.Currency == item.Currency
                                        && availableAccount2.Contains(x.InstrumentType))
                            .Select(x => x.Amount ?? 0)
                            .DefaultIfEmpty(0)
                            .Sum();

                        item.Ob = ob;
                    }
                }

                foreach (var item in dataObj.Accounts)
                {
                    var availableAccount3 = db.Config_FcaBankAccount
                        .Where(x => x.Currency == item.Currency && x.AccountName1 == item.Account)
                        .Select(x => x.AccountName3)
                        .ToList();

                    if (availableAccount3.Any())
                    {
                        var approvedFidForms = db.FID_Treasury
                            .Where(x => x.FormStatus == Common.FormStatus.Approved
                                        && DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate)
                                        && x.Currency == item.Currency)
                            .Select(x => x.Id)
                            .ToList();

                        if (approvedFidForms.Any())
                        {
                            var IF_DepositMaturity = db.FID_Treasury_Deposit
                                .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .Select(x => x.PrincipalIntProfitReceivable)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var IF_MoneyMarket = db.FID_Treasury_MMI
                                .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .Select(x => x.Proceeds)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var OF_RolloverNewPlacement = db.FID_Treasury_Deposit
                                .Where(x => x.CashflowType == Common.Cashflow.Outflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .Select(x => x.PrincipalIntProfitReceivable)
                                .DefaultIfEmpty(0)
                                .Sum();
                            var OF_MoneyMarket = db.FID_Treasury_MMI
                                .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .Select(x => x.Proceeds)
                                .DefaultIfEmpty(0)
                                .Sum();

                            item.IF_DepositMaturity = IF_DepositMaturity;
                            item.IF_MoneyMarket = IF_MoneyMarket;
                            item.OF_DepositPlacementRollover = OF_RolloverNewPlacement;
                            item.OF_MoneyMarket = OF_MoneyMarket;
                        }
                    }
                }

                foreach (var item in dataObj.Accounts)
                {
                    var availableAccount3 = db.Config_FcaBankAccount
                        .Where(x => x.Currency == item.Currency && x.AccountName1 == item.Account)
                        .Select(x => x.AccountName3)
                        .ToList();

                    if (availableAccount3.Any())
                    {
                        var approvedIssdForms = db.ISSD_FormHeader
                            .Where(x => x.FormStatus == Common.FormStatus.Approved
                                        && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                        && x.Currency == item.Currency)
                            .Select(x => x.Id)
                            .ToList();

                        if (approvedIssdForms.Any())
                        {
                            var IF_Others = db.ISSD_TradeSettlement
                                .Where(x => approvedIssdForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.InflowTo))
                                .Select(x => x.InflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var OF_Others = db.ISSD_TradeSettlement
                                .Where(x => approvedIssdForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.OutflowFrom))
                                .Select(x => x.OutflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();

                            item.IF_Others = IF_Others;
                            item.OF_Others = OF_Others;
                        }
                    }
                }

                return dataObj;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new FCY_DealCutOffData();
            }
        }

        private void SummaryTable(Worksheet sheet, DateTime selectedDate, FCY_DealCutOffData_Account item, int index, string columnAlphabet)
        {
            sheet["F2"].Value = selectedDate;

            if (item.Currency == "USD")
            {
                sheet["7"].Value = item.Ob;
                sheet["8"].Value = item.IF_DepositMaturity;
                sheet["9"].Value = item.IF_MoneyMarket;
                sheet["10"].Value = item.IF_Others;
                sheet["11"].Value = item.OF_DepositPlacementRollover * -1;
                sheet["12"].Value = item.OF_MoneyMarket * -1;
                sheet["13"].Value = item.OF_Others * -1;
            }

            if (item.Currency == "GBP")
            {
                sheet["F18"].Value = item.Ob;
                sheet["F19"].Value = item.IF_DepositMaturity;
                sheet["F20"].Value = item.IF_MoneyMarket;
                sheet["F21"].Value = item.IF_Others;
                sheet["F22"].Value = item.OF_DepositPlacementRollover * -1;
                sheet["F23"].Value = item.OF_MoneyMarket * -1;
                sheet["F24"].Value = item.OF_Others * -1;
            }

            if (item.Currency == "AUD")
            {
                sheet["F29"].Value = item.Ob;
                sheet["F30"].Value = item.IF_DepositMaturity;
                sheet["F31"].Value = item.IF_MoneyMarket;
                sheet["F32"].Value = item.IF_Others;
                sheet["F33"].Value = item.OF_DepositPlacementRollover * -1;
                sheet["F34"].Value = item.OF_MoneyMarket * -1;
                sheet["F35"].Value = item.OF_Others * -1;
            }

            if (item.Currency == "EUR")
            {
                sheet["F40"].Value = item.Ob;
                sheet["F41"].Value = item.IF_DepositMaturity;
                sheet["F42"].Value = item.IF_MoneyMarket;
                sheet["F43"].Value = item.IF_Others;
                sheet["F44"].Value = item.OF_DepositPlacementRollover * -1;
                sheet["F45"].Value = item.OF_MoneyMarket * -1;
                sheet["F46"].Value = item.OF_Others * -1;
            }
        }
    }

    public class FCY_DealCutOffData
    {
        public DateTime SelectedDate { get; set; }
        public List<FCY_DealCutOffData_Account> Accounts { get; set; }
    }

    public class FCY_DealCutOffData_Account
    {
        public string Account { get; set; }
        public string AccountNo { get; set; }
        public string Currency { get; set; }
        public double Ob { get; set; }
        public double IF_DepositMaturity { get; set; }
        public double IF_MoneyMarket { get; set; }
        public double IF_Others{ get; set; }
        public double OF_DepositPlacementRollover { get; set; }
        public double OF_MoneyMarket { get; set; }
        public double OF_Others { get; set; }
    }
}