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
            var accounts = db.Config_FcaBankAccount.ToList();

            var dataObj = new FCY_DealCutOffData()
            {
                SelectedDate = selectedDate,
                Accounts = new List<FCY_DealCutOffData_Account>()
            };

            foreach (var account in accounts)
            {
                var ob = db.EDW_BankBalance
                    .AsNoTracking()
                    .Where(x => x.InstrumentType == account.AccountName2
                                && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == account.Currency)
                    .Select(x => x.Amount ?? 0)
                    .DefaultIfEmpty(0)
                    .Sum();

                var approvedFidForms = db.FID_Treasury
                    .Where(x => x.FormStatus == Common.FormStatus.Approved
                                && DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == account.Currency)
                    .Select(x => x.Id)
                    .ToList();

                var IF_DepositMaturity = db.FID_Treasury_Deposit
                    .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                && approvedFidForms.Contains(x.FormId))
                    .Select(x => x.PrincipalIntProfitReceivable)
                    .DefaultIfEmpty(0)
                    .Sum();
                var IF_MoneyMarket = db.FID_Treasury_MMI
                    .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                && approvedFidForms.Contains(x.FormId))
                    .Select(x => x.Proceeds)
                    .DefaultIfEmpty(0)
                    .Sum();

                var OF_RolloverNewPlacement = db.FID_Treasury_Deposit
                    .Where(x => x.CashflowType == Common.Cashflow.Outflow
                                && approvedFidForms.Contains(x.FormId))
                    .Select(x => x.PrincipalIntProfitReceivable)
                    .DefaultIfEmpty(0)
                    .Sum();
                var OF_MoneyMarket = db.FID_Treasury_MMI
                    .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                && approvedFidForms.Contains(x.FormId))
                    .Select(x => x.Proceeds)
                    .DefaultIfEmpty(0)
                    .Sum();

                var approvedIssdForms = db.FID_TS10
                    .Where(x => x.FormStatus == Common.FormStatus.Approved
                                && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == account.Currency)
                    .Select(x => x.Id)
                    .ToList();
                var IF_Others = db.FID_TS10_TradeItem
                    .Where(x => approvedIssdForms.Contains(x.FormId) && x.InflowTo == account.AccountName3)
                    .Select(x => x.AmountPlus + x.Maturity + x.Sales + x.FirstLeg)
                    .DefaultIfEmpty(0)
                    .Sum();
                var OF_Others = db.FID_TS10_TradeItem
                    .Where(x => approvedIssdForms.Contains(x.FormId) && x.OutflowFrom == account.AccountName3)
                    .Select(x => x.AmountPlus + x.Maturity + x.Sales + x.FirstLeg)
                    .DefaultIfEmpty(0)
                    .Sum();

                dataObj.Accounts.Add(new FCY_DealCutOffData_Account
                {
                    Account = account.AccountName1,
                    AccountNo = account.AccountNo,
                    Currency = account.Currency,
                    Ob = ob,
                    IF_DepositMaturity = IF_DepositMaturity,
                    IF_MoneyMarket = IF_MoneyMarket,
                    IF_Others = IF_Others,
                    OF_DepositPlacementRollover = OF_RolloverNewPlacement,
                    OF_MoneyMarket = OF_MoneyMarket,
                    OF_Others = OF_Others
                });
            }
            

            return dataObj;
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