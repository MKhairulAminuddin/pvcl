using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC_Web.Extension.DocGenerator
{
    public class TradeSettlementFormDoc : DocGeneratorBase
    {
        public string GenerateFile(int formId, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

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

                        IWorkbook workbook = new Workbook();
                        workbook.Options.Culture = new CultureInfo("en-US");
                        workbook.LoadDocument(MapPath("~/App_Data/Trade Settlement Template.xltx"));
                        workbook = GenerateDocument(workbook, getForm, getFormWorkflow, getTrades);
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

        private IWorkbook GenerateDocument(IWorkbook workbook, Form_Header formHeader, Form_Workflow formWorkflow, List<ISSD_TradeSettlement> trades)
        {
            workbook.BeginUpdate();
            try
            {
                
                var sheet = workbook.Worksheets[0];
                
                sheet["G2"].Value = formHeader.PreparedBy;
                if (formHeader.PreparedDate != null)
                {
                    sheet["G3"].Value = formHeader.PreparedDate.Value.ToString("dd/MM/yyyy HH:ss");
                }

                sheet["G5"].Value = formHeader.ApprovedBy;
                if (formHeader.ApprovedDate != null)
                {
                    sheet["G6"].Value = formHeader.ApprovedDate.Value.ToString("dd/MM/yyyy HH:ss");
                    sheet["G7"].Value = formWorkflow.WorkflowNotes;
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

                if (formHeader.FormDate != null)
                    sheet["B6"].Value = formHeader.FormDate.Value.ToString("dd/MM/yyyy HH:ss");

                sheet["B7"].Value = formHeader.Currency;

                // Opening Balance
                //sheet["B10"].Value = formHeader.Currency;
                //sheet["B11"].Value = formHeader.Currency;

                int tradeItemStartRow = 15;
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

                    foreach (var item in equity)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":E" + tradeItemStartRow],
                        true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];
                    
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
                    
                    foreach (var item in bond)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":E" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

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
                    
                    foreach (var item in cp)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":E" + tradeItemStartRow],
                        true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

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
                    
                    foreach (var item in notes)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.Maturity;
                        sheet["D" + tradeItemStartRow].Value = item.Sales;
                        sheet["E" + tradeItemStartRow].Value = item.Purchase;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":E" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

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
                    
                    foreach (var item in repo)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.FirstLeg;
                        sheet["D" + tradeItemStartRow].Value = item.SecondLeg;
                    }
                    
                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":D" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[3].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[3].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[3].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

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
                    
                    foreach (var item in coupon)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.StockCode;
                        sheet["C" + tradeItemStartRow].Value = item.AmountPlus;
                    }


                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    tradeItemStartRow += 3;
                }

                // Fees
                var fees = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(7)).ToList();
                if (fees.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Fees";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    
                    foreach (var item in fees)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":B" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    tradeItemStartRow += 3;
                }

                // Payment/ Received (MTM)
                var mtm = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(8)).ToList();
                if (mtm.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Coupon Received";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                    
                    foreach (var item in mtm)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

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
                    
                    foreach (var item in fx)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                    }
                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    tradeItemStartRow += 3;
                }

                // Contribution credited
                var contribution = trades.Where(x => x.InstrumentType == Common.TradeSettlementMapping(10)).ToList();
                if (contribution.Any())
                {
                    var headerStartRow = tradeItemStartRow;

                    sheet["A" + tradeItemStartRow].Value = "Contribution credited";
                    sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                    
                    foreach (var item in contribution)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":B" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

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
                    
                    foreach (var item in altid)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                    }

                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

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
                    
                    foreach (var item in others)
                    {
                        ++tradeItemStartRow;
                        sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                        sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                        sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                    }


                    // Insert a table in the worksheet.
                    Table table = sheet.Tables.Add(sheet["A" + headerStartRow + ":C" + tradeItemStartRow], true);
                    table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleLight9];

                    table.Columns[1].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    table.Columns[2].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

                    table.Columns[1].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    table.ShowTotals = true;
                    table.Columns[1].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[2].TotalRowFunction = TotalRowFunction.Sum;
                    table.Columns[1].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    table.Columns[2].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

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
        
        
    }
}