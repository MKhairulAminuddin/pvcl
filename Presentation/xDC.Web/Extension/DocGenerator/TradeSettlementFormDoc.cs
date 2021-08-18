using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;

namespace xDC_Web.Extension.DocGenerator
{
    public class TradeSettlementFormDoc : DocGeneratorBase
    {
        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#E8F5E9");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#FFEBEE");

        public string GenerateFile(int formId, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var getTrades = db.ISSD_TradeSettlement.Where(x => x.FormId == formId).ToList();

                        getForm.Id = getForm.Id;
                        getForm.PreparedBy = getForm.PreparedBy;
                        getForm.PreparedDate = getForm.PreparedDate.Value;

                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == "Approved" || x.WorkflowStatus == "Rejected") &&
                                        x.FormId == getForm.Id).OrderByDescending(x => x.EndDate)
                            .FirstOrDefault();

                        var openingBalance = db.ISSD_Balance.Where(x => x.FormId == formId).ToList();

                        IWorkbook workbook = new Workbook();
                        workbook.Options.Culture = new CultureInfo("en-US");
                        workbook.LoadDocument(MapPath("~/App_Data/Trade Settlement Template.xltx"));
                        workbook = GenerateDocument(workbook, getForm, getFormWorkflow, getTrades, openingBalance);
                        var randomFileName = "ISSD Trade Settlement - " + DateTime.Now.ToString("yyyyMMddHHmmss");

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
                    else
                    {
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public string GenerateFileConsolidated(DateTime settlementDate, string currency, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.ISSD_FormHeader.Where(x =>
                            DbFunctions.TruncateTime(x.SettlementDate) == settlementDate.Date 
                            && x.Currency == currency 
                            && x.FormStatus == "Approved").ToList();

                    if (getForm.Any())
                    {
                        var associatedFormIdParts = getForm.Select(x => x.Id).ToList();

                        var getTrades = new List<ISSD_TradeSettlement>();
                        foreach (var formId in associatedFormIdParts)
                        {
                            getTrades.AddRange(TradeSettlementService.GetTradeSettlement(db, formId));
                        }

                        var worflows = new List<Form_Workflow>();
                        foreach (var formId in associatedFormIdParts)
                        {
                            worflows.AddRange(TradeSettlementService.GetWorkflow(db, formId));
                        }

                        var getEquityFormId = getForm.FirstOrDefault(x => x.FormType == "Trade Settlement (Part A)");
                        var getOpeningBalance = new List<ISSD_Balance>();
                        if (getEquityFormId != null)
                        {
                            getOpeningBalance = db.ISSD_Balance.Where(x => x.FormId == getEquityFormId.Id).ToList();
                        }
                        
                        IWorkbook workbook = new Workbook();
                        workbook.Options.Culture = new CultureInfo("en-US");
                        workbook.LoadDocument(MapPath("~/App_Data/Trade Settlement Template - Consolidated.xltx"));
                        workbook = GenerateDocumentConsolidated(workbook, settlementDate, currency, worflows, getTrades, getOpeningBalance);
                        var randomFileName = "ISSD Trade Settlement - " + DateTime.Now.ToString("yyyyMMddHHmmss");

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
                    else
                    {
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        private IWorkbook GenerateDocument(IWorkbook workbook, ISSD_FormHeader formHeader, Form_Workflow formWorkflow, List<ISSD_TradeSettlement> trades, List<ISSD_Balance> ob = null)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                if (formHeader.SettlementDate != null)
                    sheet["B2"].Value = formHeader.SettlementDate.Value.ToString("dd/MM/yyyy");
                sheet["B3"].Value = formHeader.Currency;

                sheet["H2"].Value = formHeader.PreparedBy;
                if (formHeader.PreparedDate != null)
                {
                    sheet["H3"].Value = formHeader.PreparedDate.Value.ToString("dd/MM/yyyy HH:ss");
                }

                sheet["H5"].Value = formHeader.ApprovedBy;
                if (formHeader.ApprovedDate != null)
                {
                    sheet["H6"].Value = formHeader.ApprovedDate.Value.ToString("dd/MM/yyyy HH:ss");
                    if (formWorkflow != null)
                    {
                        sheet["H7"].Value = formWorkflow.WorkflowNotes;
                    }
                }

                /*if (formHeader.AdminEdittedDate != null)
                {
                    sheet["D9:E10"].CopyFrom(sheet["D5:E6"]);
                    sheet["D9"].Value = "Admin Edit";
                    sheet["E9"].Value = formHeader.AdminEdittedBy;
                    sheet["D10"].Value = "Admin Edit Date";
                    sheet["E10"].Value = formHeader.AdminEdittedDate.Value.ToString("dd/MM/yyyy HH:ss");
                    
                    startingRownumber += 3;
                }*/
                
                int tradeItemStartRow = 7;
                // Opening Balance
                var openingBalance = ob.Where(x => x.FormId == formHeader.Id).ToList();
                if (openingBalance.Any())
                {
                    sheet["A5"].Value = "Opening Balance:";
                    sheet["A5"].Font.Bold = true;
                    sheet["A5:B5"].Merge();

                    var headerStartRow = tradeItemStartRow;
                    
                    foreach (var item in openingBalance)
                    {
                        sheet["A" + tradeItemStartRow].Value = item.BalanceCategory;
                        sheet["B" + tradeItemStartRow].Value = item.Amount;
                        sheet["B" + tradeItemStartRow].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        tradeItemStartRow++;
                    }
                    
                    tradeItemStartRow += 2;
                }

                sheet["A" + tradeItemStartRow].Value = "Daily Trade Settlement:";
                sheet["A" + tradeItemStartRow].Font.Bold = true;
                sheet["A" + tradeItemStartRow + ":B" + tradeItemStartRow].Merge();
                tradeItemStartRow += 2;

                // equity
                var equity = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(1)).ToList();
                if (equity.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Equity";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in equity)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow],
                        true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // bond
                var bond = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(2)).ToList();
                if (bond.Any())
                {
                    var headerStartRow = tradeItemStartRow;
                    
                    sheet["A" + tradeItemStartRow].Value = "Bond";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in bond)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // CP
                var cp = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(3)).ToList();
                if (cp.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "CP";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in cp)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow],
                        true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    
                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Notes & Papers
                var notes = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(4)).ToList();
                if (notes.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Notes & Papers";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in notes)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // REPO
                var repo = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(5)).ToList();
                if (repo.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Repo";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "1st Leg (+)";
                    sheet["D" + tradeItemStartRow].Value = "2nd Leg (-)";
                    sheet["E" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in repo)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.FirstLeg;
                        sheet["D" + tradeItemStartRow].Value = item.SecondLeg;
                        sheet["E" + tradeItemStartRow].Value = item.Remarks;
                    }
                    
                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":E" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[4].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Coupon Received
                var coupon = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(6)).ToList();
                if (coupon.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Coupon Received";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in coupon)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }


                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Fees
                var fees = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(7)).ToList();
                if (fees.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Fees";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in fees)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Payment/ Received (MTM)
                var mtm = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(8)).ToList();
                if (mtm.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Payment/ Receipt (MTM)";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in mtm)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // FX Settlement
                var fx = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(9)).ToList();
                if (fx.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "FX Settlement";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in fx)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }
                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Contribution credited
                var contribution = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(10)).ToList();
                if (contribution.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Contribution credited";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in contribution)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // ALTID Distribution & Drawdown
                var altid = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(11)).ToList();
                if (altid.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "ALTID Distribution & Drawdown";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in altid)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.AmountMinus;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Others
                var others = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(12)).ToList();
                if (others.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Others";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";
                    
                    foreach (var item in others)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }
                    
                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }
                
                var footerRowNumber = tradeItemStartRow + 4;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Merge();
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Value = "Generated on " + DateTime.Now.ToString("dd/MM/yyyy HH:ss") + " by "+ HttpContext.Current.User.Identity.Name;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Italic = true;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Size = 10;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Color = Color.LightSlateGray;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Alignment.Horizontal =
                    SpreadsheetHorizontalAlignment.Right;


                workbook.Calculate();
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

        private IWorkbook GenerateDocumentConsolidated(IWorkbook workbook, DateTime settlementDate, string currency, List<Form_Workflow> formWorkflow, List<ISSD_TradeSettlement> trades, List<ISSD_Balance> ob)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                sheet["B2"].Value = settlementDate.ToString("dd/MM/yyyy");
                sheet["B3"].Value = currency;

                int tradeItemStartRow = 7;
                // Opening Balance
                if (ob.Any())
                {
                    sheet["A5"].Value = "Opening Balance:";
                    sheet["A5"].Font.Bold = true;
                    sheet["A5:B5"].Merge();

                    var headerStartRow = tradeItemStartRow;

                    foreach (var item in ob)
                    {
                        sheet["A" + tradeItemStartRow].Value = item.BalanceCategory;
                        sheet["B" + tradeItemStartRow].Value = item.Amount;
                        sheet["B" + tradeItemStartRow].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        tradeItemStartRow++;
                    }

                    tradeItemStartRow += 2;
                }

                sheet["A" + tradeItemStartRow].Value = "Daily Trade Settlement:";
                sheet["A" + tradeItemStartRow].Font.Bold = true;
                sheet["A" + tradeItemStartRow + ":B" + tradeItemStartRow].Merge();
                tradeItemStartRow += 2;

                // equity
                var equity = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(1)).ToList();
                if (equity.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Equity";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in equity)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow],
                        true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    
                    table.Columns[2].DataRange.FillColor = _inflowColor;
                    table.Columns[3].DataRange.FillColor = _inflowColor;
                    table.Columns[4].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // bond
                var bond = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(2)).ToList();
                if (bond.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Bond";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in bond)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].DataRange.FillColor = _inflowColor;
                    table.Columns[3].DataRange.FillColor = _inflowColor;
                    table.Columns[4].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // CP
                var cp = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(3)).ToList();
                if (cp.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "CP";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in cp)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow],
                        true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].DataRange.FillColor = _inflowColor;
                    table.Columns[3].DataRange.FillColor = _inflowColor;
                    table.Columns[4].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Notes & Papers
                var notes = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(4)).ToList();
                if (notes.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Notes & Papers";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Maturity (+)";
                    sheet["D" + tradeItemStartRow].Value = "Sales (+)";
                    sheet["E" + tradeItemStartRow].Value = "Purchase (-)";
                    sheet["F" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in notes)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                        sheet["F" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":F" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[4].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].DataRange.FillColor = _inflowColor;
                    table.Columns[3].DataRange.FillColor = _inflowColor;
                    table.Columns[4].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[4].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[4].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[5].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // REPO
                var repo = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(5)).ToList();
                if (repo.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Repo";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "1st Leg (+)";
                    sheet["D" + tradeItemStartRow].Value = "2nd Leg (-)";
                    sheet["E" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in repo)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.FirstLeg;
                        sheet["D" + tradeItemStartRow].Value = item.SecondLeg;
                        sheet["E" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":E" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].DataRange.FillColor = _inflowColor;
                    table.Columns[3].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[4].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Coupon Received
                var coupon = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(6)).ToList();
                if (coupon.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Coupon Received";
                    sheet["B" + tradeItemStartRow].Value = "Stock Code/ ISIN";
                    sheet["C" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in coupon)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }


                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].DataRange.FillColor = _inflowColor;

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Fees
                var fees = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(7)).ToList();
                if (fees.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Fees";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in fees)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[1].DataRange.FillColor = _inflowColor;

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Payment/ Received (MTM)
                var mtm = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(8)).ToList();
                if (mtm.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Payment/ Receipt (MTM)";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in mtm)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[1].DataRange.FillColor = _inflowColor;
                    table.Columns[2].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // FX Settlement
                var fx = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(9)).ToList();
                if (fx.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "FX Settlement";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in fx)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[1].DataRange.FillColor = _inflowColor;
                    table.Columns[2].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Contribution credited
                var contribution = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(10)).ToList();
                if (contribution.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Contribution credited";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in contribution)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[1].DataRange.FillColor = _inflowColor;

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[2].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // ALTID Distribution & Drawdown
                var altid = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(11)).ToList();
                if (altid.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "ALTID Distribution & Drawdown";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in altid)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.AmountMinus;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[1].DataRange.FillColor = _inflowColor;
                    table.Columns[2].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }

                // Others
                var others = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(12)).ToList();
                if (others.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Others";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    sheet["D" + tradeItemStartRow].Value = "Remarks";

                    foreach (var item in others)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                        sheet["D" + tradeItemStartRow].Value = item.Remarks;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                    table.HeaderRowRange.Font.Color = Color.White;
                    table.ShowTableStyleRowStripes = false;

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[1].DataRange.FillColor = _inflowColor;
                    table.Columns[2].DataRange.FillColor = _outFlowColor;

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.Columns[3].TotalRowFunction = TotalRowFunction.None;

                    tradeItemStartRow += 3;
                }


                var workflowRowNumber = tradeItemStartRow + 2;

                sheet["A" + workflowRowNumber].Value = "Workflow Approval :";
                sheet["A" + workflowRowNumber].Font.Bold = true;
                workflowRowNumber += 2;

                var workflowPartA = formWorkflow.FirstOrDefault(x => x.FormType == "Trade Settlement (Part A)" && x.WorkflowStatus == "Approved");
                if (workflowPartA != null)
                {
                    sheet["A" + workflowRowNumber].Value = "Form Type";
                    sheet["B" + workflowRowNumber].Value = "Trade Settlement (Part A)";
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approver";
                    sheet["B" + workflowRowNumber].Value = workflowPartA.RequestBy;
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approved Date";
                    sheet["B" + workflowRowNumber].Value = workflowPartA.EndDate.Value.ToString("dd/MM/yyyy HH:mm");
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 2;
                }
                var workflowPartB = formWorkflow.FirstOrDefault(x => x.FormType == "Trade Settlement (Part B)" && x.WorkflowStatus == "Approved");
                if (workflowPartB != null)
                {
                    sheet["A" + workflowRowNumber].Value = "Form Type";
                    sheet["B" + workflowRowNumber].Value = "Trade Settlement (Part B)";
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approver";
                    sheet["B" + workflowRowNumber].Value = workflowPartB.RequestBy;
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approved Date";
                    sheet["B" + workflowRowNumber].Value = workflowPartB.EndDate.Value.ToString("dd/MM/yyyy HH:mm");
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 2;
                }
                var workflowPartC = formWorkflow.FirstOrDefault(x => x.FormType == "Trade Settlement (Part C)" && x.WorkflowStatus == "Approved");
                if (workflowPartC != null)
                {
                    sheet["A" + workflowRowNumber].Value = "Form Type";
                    sheet["B" + workflowRowNumber].Value = "Trade Settlement (Part C)";
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approver";
                    sheet["B" + workflowRowNumber].Value = workflowPartC.RequestBy;
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approved Date";
                    sheet["B" + workflowRowNumber].Value = workflowPartC.EndDate.Value.ToString("dd/MM/yyyy HH:mm");
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 2;
                }
                var workflowPartD = formWorkflow.FirstOrDefault(x => x.FormType == "Trade Settlement (Part D)" && x.WorkflowStatus == "Approved");
                if (workflowPartD != null)
                {
                    sheet["A" + workflowRowNumber].Value = "Form Type";
                    sheet["B" + workflowRowNumber].Value = "Trade Settlement (Part D)";
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approver";
                    sheet["B" + workflowRowNumber].Value = workflowPartD.RequestBy;
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approved Date";
                    sheet["B" + workflowRowNumber].Value = workflowPartD.EndDate.Value.ToString("dd/MM/yyyy HH:mm");
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 2;
                }
                var workflowPartE = formWorkflow.FirstOrDefault(x => x.FormType == "Trade Settlement (Part E)" && x.WorkflowStatus == "Approved");
                if (workflowPartE != null)
                {
                    sheet["A" + workflowRowNumber].Value = "Form Type";
                    sheet["B" + workflowRowNumber].Value = "Trade Settlement (Part E)";
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approver";
                    sheet["B" + workflowRowNumber].Value = workflowPartE.RequestBy;
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 1;
                    sheet["A" + workflowRowNumber].Value = "Approved Date";
                    sheet["B" + workflowRowNumber].Value = workflowPartE.EndDate.Value.ToString("dd/MM/yyyy HH:mm");
                    sheet["B" + workflowRowNumber].FillColor = Color.LightGoldenrodYellow;
                    workflowRowNumber += 2;
                }

                var footerRowNumber = workflowRowNumber + 4;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Merge();
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Value = "Generated on " +
                                                                              DateTime.Now.ToString(
                                                                                  "dd/MM/yyyy HH:ss") + " by " +
                                                                              HttpContext.Current.User.Identity.Name;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Italic = true;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Size = 10;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Color = Color.LightSlateGray;
                sheet["A" + footerRowNumber + ":G" + footerRowNumber].Alignment.Horizontal =
                    SpreadsheetHorizontalAlignment.Right;


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
        

    }
}