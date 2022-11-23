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
using xDC.Services.Form;
using xDC.Utils;

namespace xDC_Web.Extension.DocGenerator
{
    public class TreasuryFormDoc : DocGeneratorBase
    {
        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#3498DB");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#E67E22");

        public string GenerateFile(int formId, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var deposits = db.FID_Treasury_Deposit.Where(x => x.FormId == formId).ToList();
                        var mmis = db.FID_Treasury_MMI.Where(x => x.FormId == formId).ToList();
                        
                        var workflow = db.Form_Workflow
                            .Where(x => x.FormId == form.Id && (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected))
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();
                        
                        IWorkbook workbook = GenerateDocument(form, deposits, mmis, workflow);
                        var randomFileName = Common.DownloadedFileName.FID_Treasury + DateTime.Now.ToString("yyyyMMddHHmmss");

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
        
        private IWorkbook GenerateDocument(FID_Treasury form, List<FID_Treasury_Deposit> depo, List<FID_Treasury_MMI> mmi, Form_Workflow workflow)
        {
            IWorkbook workbook = new Workbook();
            workbook.Options.Culture = new CultureInfo("en-US");
            workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_Treasury));

            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];
                
                if (form.ValueDate != null)
                    sheet["B2"].Value = form.ValueDate.Value.ToString("dd/MM/yyyy");
                sheet["B3"].Value = form.Currency;

                sheet["N2"].Value = form.FormStatus;
                sheet["N3"].Value = form.PreparedBy;
                sheet["N4"].Value = form.PreparedDate?.ToString("dd/MM/yyyy hh:mm tt");
                sheet["N5"].Value = form.ApprovedBy;
                sheet["N6"].Value = form.ApprovedDate?.ToString("dd/MM/yyyy hh:mm tt");
                sheet["N7"].Value = workflow?.WorkflowNotes;
                
                int row = 9;

                Content_MainTable(workbook, sheet,
                    depo.Where(x => x.CashflowType == Common.Cashflow.Inflow).ToList(),
                    mmi.Where(x => x.CashflowType == Common.Cashflow.Inflow).ToList(), Common.Cashflow.Inflow, row,
                    out row);

                Content_MainTable(workbook, sheet,
                    depo.Where(x => x.CashflowType == Common.Cashflow.Outflow).ToList(),
                    mmi.Where(x => x.CashflowType == Common.Cashflow.Outflow).ToList(), Common.Cashflow.Outflow, row,
                    out row);
                
                var footerRowNumber = row + 4;
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
        
        private void Content_MainTable(IWorkbook workbook, Worksheet sheet, List<FID_Treasury_Deposit> depositItems, List<FID_Treasury_MMI> mmiItems, string cashflowType, int row, out int currentRowIndex)
        {
            var depo = depositItems.Where(x => x.CashflowType == cashflowType).ToList();
            var mmi = mmiItems.Where(x => x.CashflowType == cashflowType).ToList();

            if (depo.Any() || mmi.Any())
            {
                sheet["A" + row].Value = "FUND " + cashflowType + " :";
                sheet["A" + row].Font.Bold = true;
                sheet["A" + row + ":B" + row].Merge();
                row += 2;
            }

            if (depo.Any())
            {
                sheet["A" + row].Value = "Deposit";
                sheet["A" + row].Font.Bold = true;
                sheet["A" + row].Font.Color = Color.White;
                sheet["A" + row + ":B" + row].Merge();
                sheet["A" + row + ":B" + row].FillColor = cashflowType == Common.Cashflow.Inflow ? _inflowColor : _outFlowColor;
                row += 2;

                var rowNumberCount = 0;
                var headerStartRow = row;

                sheet["A" + row].Value = "No.";
                sheet["B" + row].Value = "Dealer";
                sheet["C" + row].Value = "Bank";
                sheet["D" + row].Value = "Trade Date";
                sheet["E" + row].Value = "Value Date";
                sheet["F" + row].Value = "Maturity Date (T)";
                sheet["G" + row].Value = "Principal";
                sheet["H" + row].Value = "Tenor (day)";
                sheet["I" + row].Value = "Rate (%)";
                sheet["J" + row].Value = "Interest/ Profit Receivable";
                sheet["K" + row].Value = "Principal + Interest/ Profit Receivable";
                sheet["L" + row].Value = "Asset Type";
                sheet["M" + row].Value = "REPO Tag";
                sheet["N" + row].Value = "Contact Person";
                sheet["O" + row].Value = "Notes";

                foreach (var item in depo)
                {
                    ++row;
                    ++rowNumberCount;
                    sheet["A" + row].Formula = "=ROW(A" + rowNumberCount + ")";
                    sheet["B" + row].Value = item.Dealer;
                    sheet["C" + row].Value = item.Bank;
                    sheet["D" + row].Value = item.TradeDate?.ToString("dd/MM/yyyy");
                    sheet["E" + row].Value = item.ValueDate?.ToString("dd/MM/yyyy");
                    sheet["F" + row].Value = item.MaturityDate?.ToString("dd/MM/yyyy");
                    sheet["G" + row].Value = Math.Truncate(100 * item.Principal) / 100;
                    sheet["H" + row].Value = item.Tenor;
                    sheet["I" + row].Value = item.RatePercent;
                    sheet["J" + row].Value = Math.Truncate(100 * item.IntProfitReceivable) / 100;
                    sheet["K" + row].Value = Math.Truncate(100 * item.PrincipalIntProfitReceivable) / 100;
                    sheet["L" + row].Value = item.AssetType;
                    sheet["M" + row].Value = item.RepoTag;
                    sheet["N" + row].Value = item.ContactPerson;
                    sheet["O" + row].Value = item.Notes;

                    sheet["G" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    sheet["H" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    sheet["I" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    sheet["B" + row].Alignment.WrapText = true;
                    sheet["C" + row].Alignment.WrapText = true;
                }

                // Insert a table in the worksheet.
                var tableRange = "A" + headerStartRow + ":O" + row;
                var table = sheet.Tables.Add(sheet[tableRange], true);
                table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                table.HeaderRowRange.Font.Color = Color.White;
                table.AutoFilter.Disable();
                table.ShowTableStyleRowStripes = false;
                table.ShowTotals = true;
                table.HeaderRowRange.FillColor = cashflowType == Common.Cashflow.Inflow ? _inflowColor : _outFlowColor;

                
                table.Columns[6].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[6].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[9].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[9].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[10].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[10].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                
                table.DataRange.Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
                table.HeaderRowRange.Alignment.WrapText = true;

                row += 4;
            }
            
            if (mmi.Any())
            {
                sheet["A" + row].Value = "Money Market Instruments";
                sheet["A" + row].Font.Bold = true;
                sheet["A" + row].Font.Color = Color.White;
                sheet["A" + row + ":B" + row].Merge();
                sheet["A" + row + ":B" + row].FillColor = cashflowType == Common.Cashflow.Inflow ? _inflowColor : _outFlowColor;
                row += 2;

                var rowNumberCount = 0;
                var headerStartRow = row;

                sheet["A" + row].Value = "No.";
                sheet["B" + row].Value = "Issuer";
                sheet["C" + row].Value = "Product Type";
                sheet["D" + row].Value = "Counterparty";
                sheet["E" + row].Value = "Trade Date";
                sheet["F" + row].Value = "Value Date";
                sheet["G" + row].Value = "Maturity Date";
                sheet["H" + row].Value = "Nominal";
                sheet["I" + row].Value = "Tenor (days)";
                sheet["J" + row].Value = "Sell Rate / Yield (%)";
                sheet["K" + row].Value = "Price (RM)";
                sheet["L" + row].Value = "Purchase Proceeds";
                sheet["M" + row].Value = "Interest/ Dividend";
                sheet["N" + row].Value = "Proceeds (RM)";
                sheet["O" + row].Value = "Certificate No. / Stock Code";

                foreach (var item in mmi)
                {
                    ++row;
                    ++rowNumberCount;
                    sheet["A" + row].Formula = "=ROW(A" + rowNumberCount + ")";
                    sheet["B" + row].Value = item.Issuer;
                    sheet["C" + row].Value = item.ProductType;
                    sheet["D" + row].Value = item.CounterParty;
                    sheet["E" + row].Value = item.TradeDate?.ToString("dd/MM/yyyy");
                    sheet["F" + row].Value = item.ValueDate?.ToString("dd/MM/yyyy");
                    sheet["G" + row].Value = item.MaturityDate?.ToString("dd/MM/yyyy");
                    sheet["H" + row].Value = item.Nominal;
                    sheet["I" + row].Value = item.HoldingDayTenor;
                    sheet["J" + row].Value = item.SellPurchaseRateYield;
                    sheet["K" + row].Value = item.Price;
                    sheet["L" + row].Value = item.PurchaseProceeds;
                    sheet["M" + row].Value = item.IntDividendReceivable;
                    sheet["N" + row].Value = item.Proceeds;
                    sheet["O" + row].Value = item.CertNoStockCode;

                    sheet["H" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    sheet["I" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    sheet["J" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    sheet["K" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                    sheet["L" + row].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                    sheet["B" + row].Alignment.WrapText = true;
                    sheet["C" + row].Alignment.WrapText = true;
                    sheet["D" + row].Alignment.WrapText = true;
                }

                // Insert a table in the worksheet.
                var tableRange = "A" + headerStartRow + ":O" + row;
                var table = sheet.Tables.Add(sheet[tableRange], true);
                table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
                table.HeaderRowRange.Font.Color = Color.White;
                table.AutoFilter.Disable();
                table.ShowTableStyleRowStripes = false;
                table.ShowTotals = true;
                table.HeaderRowRange.FillColor = cashflowType == Common.Cashflow.Inflow ? _inflowColor : _outFlowColor;

                table.Columns[7].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[7].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[11].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[11].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[12].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[12].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                table.DataRange.Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
                table.HeaderRowRange.Alignment.WrapText = true;

                row += 4;
            }
            
            currentRowIndex = row;
        }
        
    }
}