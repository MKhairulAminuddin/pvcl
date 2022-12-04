using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using xDC.Domain.Form;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Form;
using xDC.Services.Workflow;
using xDC.Utils;

namespace xDC.Services.FileGenerator
{
    public class GenFile_TreasuryForm : FileGenerator, IGenFile_TreasuryForm
    {
        #region Fields

        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#3498DB");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#E67E22");

        private readonly IXDcLogger _logger;
        private readonly IWorkflowService _wfService;

        #endregion

        #region Ctor

        public GenFile_TreasuryForm(IXDcLogger logger, IWorkflowService wfService)
        {
            _logger = logger;
            _wfService = wfService;
        }

        #endregion

        #region Methods

        public string GenId_TreasuryForm(int formId, string currentUser, bool isExportAsExcel)
        {    
            IWorkbook workbook = GenDoc_TreasuryForm(formId, currentUser);
            if (workbook == null) return null;

            return SaveAndGenDocId(workbook, Common.DownloadedFileName.FID_Treasury, isExportAsExcel);
        }

        #endregion

        #region Private Methods

        private IWorkbook GenDoc_TreasuryForm(int formId, string currentUser)
        {
            IWorkbook workbook = new Workbook();
            workbook.Options.Culture = new CultureInfo("en-US");
            workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_Treasury));

            try
            {
                workbook.BeginUpdate();

                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return null;

                    var deposits = db.FID_Treasury_Deposit.Where(x => x.FormId == formId).ToList();
                    var mmis = db.FID_Treasury_MMI.Where(x => x.FormId == formId).ToList();

                    var wfLatest = _wfService.Info(form.Id, form.FormType)
                        .Where(x => x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected)
                        .FirstOrDefault();



                    var sheet = workbook.Worksheets[0];

                    if (form.ValueDate != null)
                        sheet["B2"].Value = form.ValueDate.Value.ToString("dd/MM/yyyy");
                    sheet["B3"].Value = form.Currency;

                    sheet["N2"].Value = form.FormStatus;
                    sheet["N3"].Value = form.PreparedBy;
                    sheet["N4"].Value = form.PreparedDate?.ToString("dd/MM/yyyy hh:mm tt");
                    sheet["N5"].Value = form.ApprovedBy;
                    sheet["N6"].Value = form.ApprovedDate?.ToString("dd/MM/yyyy hh:mm tt");
                    sheet["N7"].Value = wfLatest?.WorkflowNotes;

                    int row = 9;

                    Content_MainTable(workbook, sheet,
                        deposits.Where(x => x.CashflowType == Cashflow.INFLOW.ToString()).ToList(),
                        mmis.Where(x => x.CashflowType == Cashflow.INFLOW.ToString()).ToList(), Cashflow.INFLOW.ToString(), row,
                        out row);

                    Content_MainTable(workbook, sheet,
                        deposits.Where(x => x.CashflowType == Cashflow.OUTFLOW.ToString()).ToList(),
                        mmis.Where(x => x.CashflowType == Cashflow.OUTFLOW.ToString()).ToList(), Cashflow.OUTFLOW.ToString(), row,
                        out row);

                    var footerRowNumber = row + 4;
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Merge();
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Value = "Generated on " + DateTime.Now.ToString("dd/MM/yyyy HH:ss") + " by " + HttpContext.Current.User.Identity.Name;
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Italic = true;
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Size = 10;
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Font.Color = Color.LightSlateGray;
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Alignment.Horizontal =
                        SpreadsheetHorizontalAlignment.Right;

                    workbook.Calculate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                sheet["A" + row + ":B" + row].FillColor = cashflowType == Cashflow.INFLOW.ToString() ? _inflowColor : _outFlowColor;
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
                table.HeaderRowRange.FillColor = cashflowType == Cashflow.INFLOW.ToString() ? _inflowColor : _outFlowColor;


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
                sheet["A" + row + ":B" + row].FillColor = cashflowType == Cashflow.INFLOW.ToString() ? _inflowColor : _outFlowColor;
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
                table.HeaderRowRange.FillColor = cashflowType == Cashflow.INFLOW.ToString() ? _inflowColor : _outFlowColor;

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


        #endregion
    }
}
