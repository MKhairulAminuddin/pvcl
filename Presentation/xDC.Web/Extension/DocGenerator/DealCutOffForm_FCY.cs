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

        public IWorkbook GenerateWorkbook(DateTime? selectedDate)
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
                var sheetNames = new List<string>()
                {
                    "Maybank MFCA", "CITI MFCA", "Hong Leong Bank MFCA", "JP Morgan MFCA", "CIMB FCA"
                };

                #region Sheet 1

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

                workbook.Calculate();

                #endregion

                #region Sheet 2 - "Maybank MFCA"

                foreach (var sheetName in sheetNames)
                {
                    var currentSheet = workbook.Worksheets[sheetName];

                    currentSheet["B2"].Value = string.Format("FOREIGN CURRENCY TRANSACTIONS VIA {0} FOR VALUE DATE {1:dd/MM/yyyy}",
                        sheetName, dataItem.SelectedDate.Value);
                    
                    var accounts = dataItem.Accounts.Where(x => x.Account == sheetName).ToList();
                    var IF_Deposit_items = new List<FCY_Item>();
                    var IF_MM_items = new List<FCY_Item>();
                    var IF_Others_items = new List<FCY_ItemOthers>();
                    var OF_Deposit_items = new List<FCY_Item>();
                    var OF_MM_items = new List<FCY_Item>();
                    var OF_Others_items = new List<FCY_ItemOthers>();
                    foreach (var account in accounts)
                    {
                        IF_Deposit_items.AddRange(account.Details_IF_Deposit_Maturity);
                        IF_MM_items.AddRange(account.Details_IF_MM);
                        IF_Others_items.AddRange(account.Details_IF_Others);

                        OF_Deposit_items.AddRange(account.Details_OF_Deposit_Maturity);
                        OF_MM_items.AddRange(account.Details_OF_MM);
                        OF_Others_items.AddRange(account.Details_OF_Others);
                    }

                    var IF_Deposit_items_startIndex = 7;
                    var IF_Deposit_items_endIndex = 10;

                    DetailsTab_Table1(IF_Deposit_items, IF_Deposit_items_startIndex, ref IF_Deposit_items_endIndex, ref currentSheet, false, true);

                    var IF_MM_items_startIndex = IF_Deposit_items_endIndex + 2;
                    var IF_MM_items_endIndex = IF_Deposit_items_endIndex + 5;
                    
                    DetailsTab_Table1(IF_MM_items, IF_MM_items_startIndex, ref IF_MM_items_endIndex, ref currentSheet);

                    var IF_Others_items_startIndex = IF_MM_items_endIndex + 2;
                    var IF_Others_items_endIndex = IF_MM_items_endIndex + 5;

                    DetailsTab_Table2(IF_Others_items, IF_Others_items_startIndex, ref IF_Others_items_endIndex, ref currentSheet);

                    // OF

                    var OF_Deposit_items_startIndex = IF_Others_items_endIndex + 6;
                    var OF_Deposit_items_endIndex = IF_Others_items_endIndex + 9;

                    DetailsTab_Table1(OF_Deposit_items, OF_Deposit_items_startIndex, ref OF_Deposit_items_endIndex, ref currentSheet, true, true);

                    var OF_MM_items_startIndex = OF_Deposit_items_endIndex + 2;
                    var OF_MM_items_endIndex = OF_Deposit_items_endIndex + 5;

                    DetailsTab_Table1(OF_MM_items, OF_MM_items_startIndex, ref OF_MM_items_endIndex, ref currentSheet, true);

                    var OF_Others_items_startIndex = OF_MM_items_endIndex + 2;
                    var OF_Others_items_endIndex = OF_MM_items_endIndex + 5;

                    DetailsTab_Table2(OF_Others_items, OF_Others_items_startIndex, ref OF_Others_items_endIndex, ref currentSheet);


                    workbook.Calculate();
                }
                
                #endregion
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

        private FCY_DealCutOffData ConstructData(kashflowDBEntities db, DateTime? selectedDate)
        {
                var accounts = db.Config_FcaBankAccount.Where(x => x.Currency != "MYR").Select(x => x.AccountName1).Distinct().ToList();
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

                #region Sheet Others - Details

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
                                .ToList();

                            if (IF_DepositMaturity.Any())
                            {
                                item.Details_IF_Deposit_Maturity = new List<FCY_Item>();

                                foreach (var i in IF_DepositMaturity)
                                {
                                    item.Details_IF_Deposit_Maturity.Add(new FCY_Item
                                    {
                                        Item = i.Bank,
                                        Currency = item.Currency,
                                        TradeDate = selectedDate.Value,
                                        MaturityDate = i.MaturityDate.Value,
                                        ValueDate = i.ValueDate.Value,
                                        Amount = i.Principal,
                                        Tenor = i.Tenor.Value,
                                        Rate = i.RatePercent,
                                        Interest = i.IntProfitReceivable,
                                        Principal_Interest = i.PrincipalIntProfitReceivable,
                                        InstrumentType = i.AssetType,
                                        Fca = i.FcaAccount,
                                        ContactPerson = i.ContactPerson
                                    });
                                }
                            }

                            var IF_MoneyMarket = db.FID_Treasury_MMI
                                .Where(x => x.CashflowType == Common.Cashflow.Inflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .ToList();

                            if (IF_MoneyMarket.Any())
                            {
                                item.Details_IF_MM = new List<FCY_Item>();

                                foreach (var i in IF_MoneyMarket)
                                {
                                    item.Details_IF_MM.Add(new FCY_Item
                                    {
                                        Item = i.CounterParty,
                                        Currency = item.Currency,
                                        TradeDate = selectedDate.Value,
                                        MaturityDate = i.MaturityDate.Value,
                                        ValueDate = i.ValueDate.Value,
                                        Amount = i.Proceeds,
                                        Tenor = i.HoldingDayTenor.Value,
                                        Rate = i.SellPurchaseRateYield,
                                        Interest = i.IntDividendReceivable,
                                        Principal_Interest = i.IntDividendReceivable,
                                        InstrumentType = i.ProductType,
                                        Fca = i.FcaAccount,
                                        ContactPerson = i.Dealer
                                    });
                                }
                            }

                            var OF_RolloverNewPlacement = db.FID_Treasury_Deposit
                                .Where(x => x.CashflowType == Common.Cashflow.Outflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .ToList();

                            if (OF_RolloverNewPlacement.Any())
                            {
                                item.Details_OF_Deposit_Maturity = new List<FCY_Item>();

                                foreach (var i in OF_RolloverNewPlacement)
                                {
                                    item.Details_OF_Deposit_Maturity.Add(new FCY_Item
                                    {
                                        Item = i.Bank,
                                        Currency = item.Currency,
                                        TradeDate = selectedDate.Value,
                                        MaturityDate = i.MaturityDate.Value,
                                        ValueDate = i.ValueDate.Value,
                                        Amount = i.Principal,
                                        Tenor = i.Tenor.Value,
                                        Rate = i.RatePercent,
                                        Interest = i.IntProfitReceivable,
                                        Principal_Interest = i.PrincipalIntProfitReceivable,
                                        InstrumentType = i.AssetType,
                                        Fca = i.FcaAccount,
                                        ContactPerson = i.ContactPerson,
                                        Dealer = i.Dealer
                                    });
                                }
                            }

                            var OF_MoneyMarket = db.FID_Treasury_MMI
                                .Where(x => x.CashflowType == Common.Cashflow.Outflow
                                            && approvedFidForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.FcaAccount))
                                .ToList();

                            if (OF_MoneyMarket.Any())
                            {
                                item.Details_OF_MM = new List<FCY_Item>();

                                foreach (var i in OF_MoneyMarket)
                                {
                                    item.Details_OF_MM.Add(new FCY_Item
                                    {
                                        Item = i.CounterParty,
                                        Currency = item.Currency,
                                        TradeDate = selectedDate.Value,
                                        MaturityDate = i.MaturityDate.Value,
                                        ValueDate = i.ValueDate.Value,
                                        Amount = i.Proceeds,
                                        Tenor = i.HoldingDayTenor.Value,
                                        Rate = i.SellPurchaseRateYield,
                                        Interest = i.IntDividendReceivable,
                                        Principal_Interest = i.IntDividendReceivable,
                                        InstrumentType = i.ProductType,
                                        Fca = i.FcaAccount,
                                        Dealer = i.Dealer
                                    });
                                }
                            }
                        }

                        #region Others Portion

                        var approvedTsForms = db.ISSD_FormHeader
                            .Where(x => x.FormStatus == Common.FormStatus.Approved
                                        && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                        && x.Currency == item.Currency)
                            .Select(x => x.Id)
                            .ToList();

                        if (approvedTsForms.Any())
                        {
                            var IF_Others = db.ISSD_TradeSettlement
                                .Where(x => x.InflowAmount > 0
                                            && approvedTsForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.InflowTo))
                                .ToList();

                            if (IF_Others.Any())
                            {
                                item.Details_IF_Others = new List<FCY_ItemOthers>();

                                foreach (var i in IF_Others)
                                {
                                    item.Details_IF_Others.Add(new FCY_ItemOthers
                                    {
                                        Item = i.InstrumentCode,
                                        Currency = item.Currency,
                                        Notes = i.InstrumentType,
                                        Fca = i.InflowTo,
                                        Amount = i.InflowAmount
                                    });
                                }
                            }

                            var OF_Others = db.ISSD_TradeSettlement
                                .Where(x => x.OutflowAmount > 0
                                            && approvedTsForms.Contains(x.FormId)
                                            && availableAccount3.Contains(x.OutflowFrom))
                                .ToList();

                            if (OF_Others.Any())
                            {
                                item.Details_OF_Others = new List<FCY_ItemOthers>();

                                foreach (var i in OF_Others)
                                {
                                    item.Details_OF_Others.Add(new FCY_ItemOthers
                                    {
                                        Item = i.InstrumentCode,
                                        Currency = item.Currency,
                                        Notes = i.InstrumentType,
                                        Fca = i.OutflowFrom,
                                        Amount = i.OutflowAmount
                                    });
                                }
                            }
                        }

                        #endregion
                    }
                }

                #endregion




                return dataObj;
        }

        private void DetailsTab_Table1(List<FCY_Item> items, int startIndex, ref int endIndex, ref Worksheet sheet, bool withDealer = false, bool isDepositTable = false)
        {
            if (items.Any())
            {
                var currentIndex = startIndex += 2;

                foreach (var item in items)
                {
                    if (currentIndex != startIndex)
                    {
                        sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                        sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                    }

                    sheet["C" + currentIndex].Value = item.Item;
                    sheet["D" + currentIndex].Value = item.Currency;
                    sheet["E" + currentIndex].Value = item.Notes;
                    sheet["F" + currentIndex].Value = item.TradeDate;
                    sheet["G" + currentIndex].Value = item.MaturityDate;
                    sheet["H" + currentIndex].Value = item.ValueDate;
                    sheet["I" + currentIndex].Value = item.Amount;
                    sheet["J" + currentIndex].Value = item.Tenor;
                    sheet["K" + currentIndex].Value = item.Rate;
                    sheet["L" + currentIndex].Value = item.Interest;
                    sheet["M" + currentIndex].Value = item.Principal_Interest;
                    sheet["N" + currentIndex].Value = item.InstrumentType;
                    sheet["O" + currentIndex].Value = item.Fca;
                    sheet["P" + currentIndex].Value = item.ContactPerson;

                    if (withDealer)
                    {
                        sheet["Q" + currentIndex].Value = item.Dealer;
                    }

                    currentIndex++;
                }
                
                sheet["I" + currentIndex].Formula = "=SUM($I$" + startIndex + ":$I$" + (currentIndex - 1) + ")";
                if (isDepositTable)
                {
                    sheet["L" + currentIndex].Formula = "=SUM($L$" + startIndex + ":$L$" + (currentIndex - 1) + ")";
                }
                sheet["M" + currentIndex].Formula = "=SUM($M$" + startIndex + ":$M$" + (currentIndex - 1) + ")";
                
                endIndex = currentIndex;
            }
        }

        private void DetailsTab_Table2(List<FCY_ItemOthers> items, int startIndex, ref int endIndex, ref Worksheet sheet)
        {
            if (items.Any())
            {
                var currentIndex = startIndex += 2;

                foreach (var item in items)
                {
                    if (currentIndex != startIndex)
                    {
                        sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                        sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                    }

                    sheet["C" + currentIndex].Value = item.Item;
                    sheet["D" + currentIndex].Value = item.Currency;
                    sheet["E" + currentIndex].Value = item.Notes;
                    sheet["F" + currentIndex].Value = item.Amount;
                    sheet["G" + currentIndex].Value = item.Fca;

                    currentIndex++;
                }

                sheet["F" + currentIndex].Formula = "=SUM($F$" + startIndex + ":$F$" + (currentIndex - 1) + ")";
                
                endIndex = currentIndex;
            }
        }
    }

    public class FCY_DealCutOffData
    {
        public DateTime? SelectedDate { get; set; }
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

        // details part
        public List<FCY_Item> Details_IF_Deposit_Maturity { get; set; }
        public List<FCY_Item> Details_IF_MM { get; set; }
        public List<FCY_ItemOthers> Details_IF_Others { get; set; }

        public List<FCY_Item> Details_OF_Deposit_Maturity { get; set; }
        public List<FCY_Item> Details_OF_MM { get; set; }
        public List<FCY_ItemOthers> Details_OF_Others { get; set; }

        public FCY_DealCutOffData_Account()
        {
            this.Details_IF_Deposit_Maturity = new List<FCY_Item>();
            this.Details_IF_MM = new List<FCY_Item>();
            this.Details_IF_Others = new List<FCY_ItemOthers>();
            this.Details_OF_Deposit_Maturity = new List<FCY_Item>();
            this.Details_OF_MM = new List<FCY_Item>();
            this.Details_OF_Others = new List<FCY_ItemOthers>();
        }
    }

    public class FCY_Item
    {
        public string Item { get; set; }
        public string Currency { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public DateTime ValueDate { get; set; }
        public double Amount { get; set; }
        public int Tenor { get; set; }
        public double Rate { get; set; }
        public double Interest { get; set; }
        public double Principal_Interest { get; set; }
        public string InstrumentType { get; set; }
        public string Notes { get; set; }
        public string Fca { get; set; }
        public string ContactPerson { get; set; }
        public string Dealer { get; set; }
    }

    public class FCY_ItemOthers
    {
        public string Item { get; set; }
        public string Currency { get; set; }
        public string Notes { get; set; }
        public string Fca { get; set; }
        public double Amount { get; set; }
    }
}