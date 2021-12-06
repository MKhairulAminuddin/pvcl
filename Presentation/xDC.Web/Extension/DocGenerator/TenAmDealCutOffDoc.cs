using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using DevExpress.Spreadsheet;
using DevExtreme.AspNet.Data;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.ViewModels.Fid;

namespace xDC_Web.Extension.DocGenerator
{
    public class TenAmDealCutOffDoc : DocGeneratorBase
    {
        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#3498DB");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#E67E22");
        private Color _highlightColor = System.Drawing.ColorTranslator.FromHtml("#FFFDE7");

        public IWorkbook GenerateWorkbook(DateTime selectedDate, bool viewApproved)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var dataObj = ConstructData(selectedDate, viewApproved);

                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_TenAmDealCutOff));
                    workbook = GenerateDocument(workbook, dataObj, selectedDate, viewApproved);

                    return workbook;

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public string GenerateFile(DateTime selectedDate, bool isExportAsExcel, bool viewApproved)
        {
            try
            {
                IWorkbook workbook = GenerateWorkbook(selectedDate, viewApproved);
                var randomFileName = Common.DownloadedFileName.FID_TenAmDealCutOff + DateTime.Now.ToString("yyyyMMddHHmmss");

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

        private IWorkbook GenerateDocument(IWorkbook workbook, List<TenAmCutOffItemVM> items, DateTime selectedDate, bool viewApproved)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                sheet["B2"].Value =  selectedDate.ToString("dd/MM/yyyy");

                var startIndex = 6;
                var currentIndex = startIndex;
                var startGroupIndex = startIndex;

                foreach (var currency in items.GroupBy(x => x.Currency).OrderBy(x => x.Key).Select(x => x.Key))
                {
                    foreach (var item in items.Where(x => x.Currency == currency))
                    {
                        if (currentIndex != 6)
                        {
                            sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                            sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                        }

                        sheet["A" + currentIndex].Value = item.Currency;
                        sheet["B" + currentIndex].Value = item.Account;
                        sheet["C" + currentIndex].Value = item.OpeningBalance;
                        sheet["D" + currentIndex].Value = item.TotalInflow;
                        sheet["E" + currentIndex].Value = item.TotalOutflow;
                        sheet["F" + currentIndex].Value = item.Net;
                        sheet["G" + currentIndex].Value = item.ClosingBalance;

                        currentIndex++;
                    }

                    sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                    sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                    sheet["B" + currentIndex].Value = "TOTAL";
                    sheet["C" + currentIndex].Formula = "=SUM($C$" + startGroupIndex + ":$C$" + (currentIndex - 1) + ")";
                    sheet["D" + currentIndex].Formula = "=SUM($D$" + startGroupIndex + ":$D$" + (currentIndex - 1) + ")";
                    sheet["E" + currentIndex].Formula = "=SUM($E$" + startGroupIndex + ":$E$" + (currentIndex - 1) + ")";
                    sheet["F" + currentIndex].Formula = "=SUM($F$" + startGroupIndex + ":$F$" + (currentIndex - 1) + ")";
                    sheet["G" + currentIndex].Formula = "=SUM($G$" + startGroupIndex + ":$G$" + (currentIndex - 1) + ")";

                    sheet["B" + currentIndex + ":G" + currentIndex].Font.Bold = true;
                    sheet["B" + currentIndex + ":G" + currentIndex].FillColor = _highlightColor;

                    sheet["A" + startGroupIndex + ":A" + currentIndex].Merge();
                    sheet["A" + startGroupIndex + ":A" + currentIndex].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);

                    currentIndex++;
                    startGroupIndex = currentIndex;
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

        private List<TenAmCutOffItemVM> ConstructData(DateTime selectedDate, bool viewApproved)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<TenAmCutOffItemVM>();
                    var resultRaw = new List<TenAmCutOffItemVM>();

                    var configAccount = db.Config_FcaBankAccount.Where(x => x.Currency != "MYR").ToList();
                    configAccount.Add(new Config_FcaBankAccount
                    {
                        AccountName1 = "RENTAS",
                        AccountName2 = "RENTAS",
                        AccountName3 = "RENTAS",
                        Currency = "MYR"
                    });
                    configAccount.Add(new Config_FcaBankAccount
                    {
                        AccountName1 = "MMA",
                        AccountName2 = "MMA",
                        AccountName3 = "MMA",
                        Currency = "MYR"
                    });

                    #region 1 - ISSD Trade Settlement TRX

                    foreach (var account in configAccount)
                    {
                        var item = new TenAmCutOffItemVM
                        {
                            Account = account.AccountName1,
                            Currency = account.Currency
                        };
                        result.Add(item);

                        var tsFormIds = (viewApproved)
                            ? db.ISSD_FormHeader
                                .Where(x => x.FormStatus == Common.FormStatus.Approved
                                            && x.SettlementDate != null
                                            && x.Currency == item.Currency
                                            && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate))
                                .Select(x => x.Id)
                                .ToList()
                            : db.ISSD_FormHeader
                                .Where(x => x.FormStatus != Common.FormStatus.Rejected
                                            && x.SettlementDate != null
                                            && x.Currency == item.Currency
                                            && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate))
                                .Select(x => x.Id)
                                .ToList();

                        if (tsFormIds.Any())
                        {
                            var tradeItemInflow = db.ISSD_TradeSettlement
                                .Where(x => tsFormIds.Contains(x.FormId)
                                            && x.InflowTo != null
                                            && x.InflowTo == account.AccountName3
                                            && x.InflowAmount > 0)
                                .Select(l => l.InflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();

                            item.TotalInflow = tradeItemInflow;

                            // get total inflow based on assigned outflow account
                            var tradeItemOutflow = db.ISSD_TradeSettlement
                                .Where(x => tsFormIds.Contains(x.FormId)
                                            && x.OutflowFrom != null
                                            && x.OutflowFrom == account.AccountName3
                                            && x.OutflowAmount > 0)
                                .Select(l => l.OutflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();

                            item.TotalOutflow = tradeItemOutflow;
                        }

                        resultRaw.Add(item);
                    }

                    #endregion

                    #region 2 - FID Treasury TRX (MYR)

                    var fidTreasuryMyr = new TenAmCutOffItemVM
                    {
                        Account = "RENTAS",
                        Currency = "MYR"
                    };

                    var treasuryMyrFormIds = (viewApproved)
                        ? db.FID_Treasury
                            .Where(x => x.FormStatus == Common.FormStatus.Approved
                                        && x.TradeDate != null
                                        && x.Currency == "MYR"
                                        && DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate))
                            .Select(x => x.Id)
                            .ToList()
                        : db.FID_Treasury
                            .Where(x => x.FormStatus != Common.FormStatus.Rejected
                                        && x.TradeDate != null
                                        && x.Currency == "MYR"
                                        && DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate))
                            .Select(x => x.Id)
                            .ToList();

                    if (treasuryMyrFormIds.Any())
                    {
                        var inflowDeposit = db.FID_Treasury_Deposit
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.PrincipalIntProfitReceivable > 0
                                        && x.CashflowType == Common.Cashflow.Inflow)
                            .Select(l => l.PrincipalIntProfitReceivable)
                            .DefaultIfEmpty(0)
                            .Sum();

                        var inflowMmi = db.FID_Treasury_MMI
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.CashflowType == Common.Cashflow.Inflow
                                        && x.Proceeds > 0)
                            .Select(l => l.Proceeds)
                            .DefaultIfEmpty(0)
                            .Sum();

                        var outflowDeposit = db.FID_Treasury_Deposit
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.PrincipalIntProfitReceivable > 0
                                        && x.CashflowType == Common.Cashflow.Outflow)
                            .Select(l => l.PrincipalIntProfitReceivable)
                            .DefaultIfEmpty(0)
                            .Sum();

                        var outflowMmi = db.FID_Treasury_MMI
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.CashflowType == Common.Cashflow.Outflow
                                        && x.Proceeds > 0)
                            .Select(l => l.Proceeds)
                            .DefaultIfEmpty(0)
                            .Sum();

                        fidTreasuryMyr.TotalInflow += inflowDeposit;
                        fidTreasuryMyr.TotalInflow += inflowMmi;
                        fidTreasuryMyr.TotalOutflow += outflowDeposit;
                        fidTreasuryMyr.TotalOutflow += outflowMmi;
                    }

                    resultRaw.Add(fidTreasuryMyr);

                    #endregion

                    #region 3 - FID Treasury TRX (FCY)

                    foreach (var account in configAccount.Where(x => x.Currency != "MYR"))
                    {
                        var fidTreasuryFcy = new TenAmCutOffItemVM
                        {
                            Account = account.AccountName1,
                            Currency = account.Currency
                        };

                        var treasuryFcyFormIds = (viewApproved)
                            ? db.FID_Treasury
                                .Where(x => x.FormStatus == Common.FormStatus.Approved
                                            && x.TradeDate != null
                                            && x.Currency == account.Currency
                                            && DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate))
                                .Select(x => x.Id)
                                .ToList()
                            : db.FID_Treasury
                                .Where(x => x.FormStatus != Common.FormStatus.Rejected
                                            && x.TradeDate != null
                                            && x.Currency == account.Currency
                                            && DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate))
                                .Select(x => x.Id)
                                .ToList();

                        if (treasuryFcyFormIds.Any())
                        {
                            var inflowDeposit = db.FID_Treasury_Deposit
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.PrincipalIntProfitReceivable > 0
                                            && x.CashflowType == Common.Cashflow.Inflow
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.PrincipalIntProfitReceivable)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var inflowMmi = db.FID_Treasury_MMI
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.CashflowType == Common.Cashflow.Inflow
                                            && x.Proceeds > 0
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.Proceeds)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var outflowDeposit = db.FID_Treasury_Deposit
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.PrincipalIntProfitReceivable > 0
                                            && x.CashflowType == Common.Cashflow.Outflow
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.PrincipalIntProfitReceivable)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var outflowMmi = db.FID_Treasury_MMI
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.CashflowType == Common.Cashflow.Outflow
                                            && x.Proceeds > 0
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.Proceeds)
                                .DefaultIfEmpty(0)
                                .Sum();

                            fidTreasuryFcy.TotalInflow += inflowDeposit;
                            fidTreasuryFcy.TotalInflow += inflowMmi;
                            fidTreasuryFcy.TotalOutflow += outflowDeposit;
                            fidTreasuryFcy.TotalOutflow += outflowMmi;
                        }

                        resultRaw.Add(fidTreasuryFcy);
                    }


                    #endregion

                    #region 4 - AMSD Inflow Fund

                    var amsdFormIds = (viewApproved)
                        ? db.AMSD_IF
                            .Where(x => DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(selectedDate)
                                        && x.FormStatus == Common.FormStatus.Approved)
                            .Select(x => x.Id).ToList()
                        : db.AMSD_IF
                            .Where(x => DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(selectedDate)
                                        && x.FormStatus != Common.FormStatus.Rejected)
                            .Select(x => x.Id)
                            .ToList();

                    if (amsdFormIds.Any())
                    {
                        var inflowFunds = db.AMSD_IF_Item
                            .Where(x => amsdFormIds.Contains(x.FormId))
                            .GroupBy(x => new
                            {
                                x.Bank
                            })
                            .Select(x => new
                            {
                                Bank = x.Key.Bank,
                                Amount = x.Sum(y => y.Amount)
                            });

                        foreach (var fund in inflowFunds)
                        {
                            var inflowFundsFromAmsd = new TenAmCutOffItemVM()
                            {
                                Account = fund.Bank,
                                Currency = "MYR",
                                TotalInflow = fund.Amount,
                                Net = fund.Amount
                            };
                            resultRaw.Add(inflowFundsFromAmsd);
                        }
                    }
                    #endregion

                    #region 5 - Opening Balance

                    result = resultRaw
                        .GroupBy(x => new
                        {
                            x.Account,
                            x.Currency
                        })
                        .Select(x => new TenAmCutOffItemVM
                        {
                            Currency = x.Key.Currency,
                            Account = x.Key.Account,
                            OpeningBalance = 0,
                            TotalInflow = x.Sum(y => y.TotalInflow),
                            TotalOutflow = x.Sum(y => y.TotalOutflow)
                        })
                        .ToList();

                    foreach (var item in result)
                    {
                        var accountName2 = configAccount.Where(x => x.AccountName1 == item.Account)
                            .Select(x => x.AccountName2).Distinct().FirstOrDefault();

                        if (accountName2 != null)
                        {
                            // plug in opening balance
                            var ob = FcaTaggingSvc.GetOpeningBalance(db, selectedDate, item.Currency, accountName2);

                            item.OpeningBalance = ob;
                        }
                    }

                    #endregion

                    result = result.GroupBy(x => new
                    {
                        x.Account,
                        x.Currency
                    })
                        .Select(x => new TenAmCutOffItemVM
                        {
                            Currency = x.Key.Currency,
                            Account = x.Key.Account,
                            OpeningBalance = x.Sum(y => y.OpeningBalance),
                            TotalInflow = x.Sum(y => y.TotalInflow),
                            TotalOutflow = x.Sum(y => y.TotalOutflow),
                            Net = x.Sum(y => y.OpeningBalance) + x.Sum(y => y.TotalInflow) - x.Sum(y => y.TotalOutflow)
                        })
                        .ToList();

                    #region Editable Closing Balance

                    var closingBalances = db.TenAmDealCutOff_ClosingBalance.Where(x => DbFunctions.TruncateTime(x.Date) == DbFunctions.TruncateTime(selectedDate)).ToList();

                    if (closingBalances.Any())
                    {
                        foreach (var item in closingBalances)
                        {
                            var getResult = result.FirstOrDefault(x => x.Account == item.Account && x.Currency == item.Currency);
                            if (getResult != null)
                            {
                                getResult.ClosingBalance = item.ClosingBalance;
                            }
                        }
                    }

                    #endregion

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }
        
    }
}