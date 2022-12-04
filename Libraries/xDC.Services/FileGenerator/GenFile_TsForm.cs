using DevExpress.Spreadsheet;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Form;
using xDC.Services.Workflow;
using xDC.Utils;

namespace xDC.Services.FileGenerator
{
    public class GenFile_TsForm: FileGenerator, IGenFile_TsForm
    {
        #region Fields

        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#E8F5E9");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#FFEBEE");
        private Color _otherColor = System.Drawing.ColorTranslator.FromHtml("#FFFDE7");

        private readonly IXDcLogger _logger;
        private readonly IWorkflowService _wfService;

        #endregion

        #region Ctor

        public GenFile_TsForm(IXDcLogger logger,  IWorkflowService wfService) : base(logger)
        {
            _logger = logger;
            _wfService = wfService;
        }

        #endregion

        #region Methods

        public string GenId_TsForm(int formId, string currentUser, bool isExportAsExcel)
        {
            IWorkbook workbook = GenDoc_Ts(formId, currentUser);
            if (workbook == null) return null;

            return SaveAndGenDocId(workbook, Common.DownloadedFileName.ISSD_TS_Consolidated, isExportAsExcel);
        }

        public string GenId_ConsolidatedTsForm(DateTime settlementDate, string currency, string currentUser, bool isExportAsExcel)
        {
            IWorkbook workbook = GenDoc_TsConsolidated(settlementDate, currency.ToUpper(), currentUser);
            if (workbook == null) return null;

            return SaveAndGenDocId(workbook, Common.DownloadedFileName.ISSD_TS_Consolidated, isExportAsExcel);
        }

        public string Gen_CnEmailFile(int formId, string senderEmail)
        {
            try
            {
                var fileName = "TradeSettlement_CN_TRX_" + DateTime.Now.ToString("yyyyMMddhhss");
                var mailMessage = new MailMessage();

                var recipients = ReceipientsFromConfig(Common.EmailNotiKey.ISSD_TS_CnEmail);
                foreach (var recipient in recipients)
                {
                    mailMessage.To.Add(recipient.Address);
                }

                var ccsEnabled = EnableNotification(Common.EmailNotiKey.Enable_ISSD_TS_CnEmail_Cc);
                var ccs = ReceipientsFromConfig(Common.EmailNotiKey.ISSD_TS_CnEmail_Cc);
                if (ccsEnabled && ccs.Any())
                {
                    foreach (var cc in ccs)
                    {
                        mailMessage.CC.Add(cc.Address);
                    }
                }

                mailMessage.From = new MailAddress(senderEmail);
                mailMessage.Headers.Add("X-Unsent", "1");
                mailMessage.Subject = Common.MailSubjectWithDate(Config.NotificationTsCnEmailSubject);

                mailMessage.Body = CnMailBody(formId);
                mailMessage.IsBodyHtml = true;

                SaveEmlFile(fileName, mailMessage);

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        #endregion

        #region Private Methods

        private IWorkbook GenDoc_Ts(int formId, string currentUser)
        {
            IWorkbook workbook = new Workbook();
            workbook.Options.Culture = new CultureInfo("en-US");
            workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.ISSD_TS));

            workbook.BeginUpdate();
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return null;

                    var trades = db.ISSD_TradeSettlement.Where(x => x.FormId == formId).ToList();



                    var sheet = workbook.Worksheets[0];

                    if (form.SettlementDate != null) 
                        sheet["B2"].Value = form.SettlementDate.Value.ToString("dd/MM/yyyy");
                    sheet["B3"].Value = form.Currency;
                    sheet["B4"].Value = form.FormType;

                    sheet["J2"].Value = form.PreparedBy;
                    sheet["J3"].Value = form.PreparedDate?.ToString("dd/MM/yyyy");

                    sheet["J4"].Value = form.ApprovedBy;
                    sheet["J5"].Value = form.ApprovedDate?.ToString("dd/MM/yyyy");

                    sheet["J6"].Value = form.FormStatus;

                    int tradeItemStartRow = 10;

                    Content_TradeItems(workbook, sheet, trades, tradeItemStartRow, out tradeItemStartRow);

                    var footerRowNumber = tradeItemStartRow + 4;
                    sheet["A" + footerRowNumber + ":J" + footerRowNumber].Merge();
                    sheet["A" + footerRowNumber + ":J" + footerRowNumber].Value = "Generated on " + DateTime.Now.ToString("dd/MM/yyyy HH:ss") + " by " + currentUser;
                    sheet["A" + footerRowNumber + ":J" + footerRowNumber].Font.Italic = true;
                    sheet["A" + footerRowNumber + ":J" + footerRowNumber].Font.Size = 10;
                    sheet["A" + footerRowNumber + ":J" + footerRowNumber].Font.Color = Color.LightSlateGray;
                    sheet["A" + footerRowNumber + ":J" + footerRowNumber].Alignment.Horizontal =
                        SpreadsheetHorizontalAlignment.Right;


                    workbook.Calculate();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex);
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

        private IWorkbook GenDoc_TsConsolidated(DateTime settlementDate, string currency, string currentUser)
        {
            IWorkbook workbook = new Workbook();
            workbook.Options.Culture = new CultureInfo("en-US");
            workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.ISSD_TS_Consolidated));

            workbook.BeginUpdate();
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var forms = db.ISSD_FormHeader.Where(x =>
                            DbFunctions.TruncateTime(x.SettlementDate) == settlementDate.Date
                            && x.Currency == currency
                            && x.FormStatus == Common.FormStatus.Approved).ToList();
                    if (!forms.Any()) return null;


                    var trades = new List<ISSD_TradeSettlement>();
                    foreach (var form in forms)
                    {
                        trades.AddRange(db.ISSD_TradeSettlement.Where(x => x.FormId == form.Id).ToList());
                    }

                    var wfHistories = new List<Form_Workflow>();
                    foreach (var form in forms)
                    {
                        var workflow = _wfService.Info(form.Id, form.FormType).Where(x => x.WorkflowStatus == Common.FormStatus.Approved).FirstOrDefault();
                        if (workflow != null) wfHistories.Add(workflow);
                    }

                    var openingBalances = new List<TsOpeningBalance>();
                    openingBalances = GetOpeningBalance(settlementDate, currency);



                    var sheet = workbook.Worksheets[0];

                    sheet["B2"].Value = settlementDate.ToString("dd/MM/yyyy");
                    sheet["B3"].Value = currency;

                    int tradeItemStartRow = 7;
                    // Opening Balance
                    if (openingBalances.Any())
                    {
                        sheet["A5"].Value = "Opening Balance:";
                        sheet["A5"].Font.Bold = true;
                        sheet["A5:B5"].Merge();

                        var headerStartRow = tradeItemStartRow;

                        foreach (var item in openingBalances)
                        {
                            sheet["A" + tradeItemStartRow].Value = item.Account;
                            sheet["B" + tradeItemStartRow].Value = item.Amount;
                            sheet["B" + tradeItemStartRow].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                            sheet["A" + tradeItemStartRow].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                            sheet["B" + tradeItemStartRow].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                            sheet["B" + tradeItemStartRow].FillColor = _otherColor;

                            tradeItemStartRow++;
                        }

                        tradeItemStartRow += 2;
                    }

                    Content_TradeItems(workbook, sheet, trades, tradeItemStartRow, out tradeItemStartRow);

                    Content_WorkflowItems(sheet, wfHistories, tradeItemStartRow, out tradeItemStartRow);

                    var footerRowNumber = tradeItemStartRow + 4;
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Merge();
                    sheet["A" + footerRowNumber + ":G" + footerRowNumber].Value = "Generated on " +
                                                                                  DateTime.Now.ToString(
                                                                                      "dd/MM/yyyy HH:ss") + " by " +
                                                                                  currentUser;
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
                _logger.LogError(ex);
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

        private void Content_TradeItems(IWorkbook workbook, Worksheet sheet, List<ISSD_TradeSettlement> trades, int tradeItemStartRow, out int currentRowIndex)
        {
            // equity
            var equity = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Equity).ToList();
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
                var tableRange = "A" + headerStartRow + ":F" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 2, 3 }, new List<int> { 4 }, new List<int> { 5 });

                tradeItemStartRow += 3;
            }

            // bond
            var bond = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Bond).ToList();
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
                var tableRange = "A" + headerStartRow + ":F" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 2, 3 }, new List<int> { 4 }, new List<int> { 5 });

                tradeItemStartRow += 3;
            }

            // CP
            var cp = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Cp).ToList();
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
                var tableRange = "A" + headerStartRow + ":F" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 2, 3 }, new List<int> { 4 }, new List<int> { 5 });

                tradeItemStartRow += 3;
            }

            // Notes & Papers
            var notes = trades.Where(x => x.InstrumentType == Common.TsItemCategory.NotesPapers).ToList();
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
                var tableRange = "A" + headerStartRow + ":F" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 2, 3 }, new List<int> { 4 }, new List<int> { 5 });

                tradeItemStartRow += 3;
            }

            // REPO
            var repo = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Repo).ToList();
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
                var tableRange = "A" + headerStartRow + ":E" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 2 }, new List<int> { 3 }, new List<int> { 4 });

                tradeItemStartRow += 3;
            }

            // Coupon Received
            var coupon = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Coupon).ToList();
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
                var tableRange = "A" + headerStartRow + ":D" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 2 }, new List<int> { }, new List<int> { 3 });

                tradeItemStartRow += 3;
            }

            // Fees
            var fees = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Fees).ToList();
            if (fees.Any())
            {
                var headerStartRow = tradeItemStartRow;

                sheet["A" + tradeItemStartRow].Value = "Fees";
                sheet["B" + tradeItemStartRow].Value = "Amount (+)";
                sheet["C" + tradeItemStartRow].Value = "Amount (-)";
                sheet["D" + tradeItemStartRow].Value = "Remarks";

                foreach (var item in fees)
                {
                    ++tradeItemStartRow;
                    sheet["A" + tradeItemStartRow].Value = item.InstrumentCode;
                    sheet["B" + tradeItemStartRow].Value = item.AmountPlus;
                    sheet["C" + tradeItemStartRow].Value = item.AmountMinus;
                    sheet["D" + tradeItemStartRow].Value = item.Remarks;
                }

                // Insert a table in the worksheet.
                var tableRange = "A" + headerStartRow + ":D" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 1 }, new List<int> { 2 }, new List<int> { 3 });

                tradeItemStartRow += 3;
            }

            // Payment/ Received (MTM)
            var mtm = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Mtm).ToList();
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
                var tableRange = "A" + headerStartRow + ":D" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 1 }, new List<int> { 2 }, new List<int> { 3 });

                tradeItemStartRow += 3;
            }

            // FX Settlement
            var fx = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Fx).ToList();
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
                var tableRange = "A" + headerStartRow + ":D" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 1 }, new List<int> { 2 }, new List<int> { 3 });

                tradeItemStartRow += 3;
            }

            // Contribution credited
            var contribution = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Cn).ToList();
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
                var tableRange = "A" + headerStartRow + ":C" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 1 }, new List<int> { }, new List<int> { 2 });

                tradeItemStartRow += 3;
            }

            // ALTID Distribution & Drawdown
            var altid = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Altid).ToList();
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
                var tableRange = "A" + headerStartRow + ":D" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 1 }, new List<int> { 2 }, new List<int> { 3 });

                tradeItemStartRow += 3;
            }

            // Others
            var others = trades.Where(x => x.InstrumentType == Common.TsItemCategory.Others).ToList();
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
                var tableRange = "A" + headerStartRow + ":D" + tradeItemStartRow;
                Table table = CommonTradeTableStyle(workbook, sheet, tableRange, new List<int> { 1 }, new List<int> { 2 }, new List<int> { 3 });

                tradeItemStartRow += 3;
            }

            currentRowIndex = tradeItemStartRow;
        }

        private Table CommonTradeTableStyle(IWorkbook workbook, Worksheet sheet, string sheetRange, List<int> inflowColumnIndex, List<int> outflowColumnIndex, List<int> nonTotalColumnIndex)
        {
            Table table = sheet.Tables.Add(sheet[sheetRange], true);
            table.Style = workbook.TableStyles[BuiltInTableStyleId.TableStyleMedium15];
            table.HeaderRowRange.Font.Color = Color.White;
            table.AutoFilter.Disable();
            table.ShowTableStyleRowStripes = false;
            table.ShowTotals = true;

            foreach (var i in inflowColumnIndex)
            {
                table.Columns[i].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                table.Columns[i].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[i].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[i].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[i].DataRange.FillColor = _inflowColor;
            }

            foreach (var i in outflowColumnIndex)
            {
                table.Columns[i].Range.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                table.Columns[i].DataRange.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[i].TotalRowFunction = TotalRowFunction.Sum;
                table.Columns[i].Total.NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                table.Columns[i].DataRange.FillColor = _outFlowColor;
            }

            foreach (var i in nonTotalColumnIndex)
            {
                table.Columns[i].TotalRowFunction = TotalRowFunction.None;
            }

            return table;
        }

        private void Content_WorkflowItems(Worksheet sheet, List<Form_Workflow> formWorkflow, int currentRow, out int currentRowIndex)
        {
            var workflowRowNumber = currentRow + 2;

            if (formWorkflow.Any())
            {
                sheet["A" + workflowRowNumber].Value = "Workflow Approval :";
                sheet["A" + workflowRowNumber].Font.Bold = true;
                workflowRowNumber += 2;
            }

            var workflowPartA = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_A);
            if (workflowPartA != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartA.FormType, workflowPartA.RequestBy, workflowPartA.WorkflowStatus, workflowPartA.RecordedDate, out workflowRowNumber);
            }
            var workflowPartB = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_B);
            if (workflowPartB != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartB.FormType, workflowPartB.RequestBy, workflowPartB.WorkflowStatus, workflowPartB.RecordedDate, out workflowRowNumber);
            }
            var workflowPartC = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_C);
            if (workflowPartC != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartC.FormType, workflowPartC.RequestBy, workflowPartC.WorkflowStatus, workflowPartC.RecordedDate, out workflowRowNumber);
            }
            var workflowPartD = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_D);
            if (workflowPartD != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartD.FormType, workflowPartD.RequestBy, workflowPartD.WorkflowStatus, workflowPartD.RecordedDate, out workflowRowNumber);
            }
            var workflowPartE = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_E);
            if (workflowPartE != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartE.FormType, workflowPartE.RequestBy, workflowPartE.WorkflowStatus, workflowPartE.RecordedDate, out workflowRowNumber);
            }
            var workflowPartF = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_F);
            if (workflowPartF != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartF.FormType, workflowPartF.RequestBy, workflowPartF.WorkflowStatus, workflowPartF.RecordedDate, out workflowRowNumber);
            }
            var workflowPartG = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_G);
            if (workflowPartG != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartG.FormType, workflowPartG.RequestBy, workflowPartG.WorkflowStatus, workflowPartG.RecordedDate, out workflowRowNumber);
            }
            var workflowPartH = formWorkflow.FirstOrDefault(x => x.FormType == Common.FormType.ISSD_TS_H);
            if (workflowPartH != null)
            {
                GenerateWorkflowBox(sheet, workflowRowNumber, workflowPartH.FormType, workflowPartH.RequestBy, workflowPartH.WorkflowStatus, workflowPartH.RecordedDate, out workflowRowNumber);
            }

            currentRowIndex = workflowRowNumber;
        }

        private void GenerateWorkflowBox(Worksheet sheet, int currentRowNumber, string formType, string approvedBy, string wfStatus, DateTime? recordedDate, out int nextRowNumber)
        {
            sheet["A" + currentRowNumber].Value = "Form Type";
            sheet["B" + currentRowNumber].Value = formType;
            sheet["B" + currentRowNumber].FillColor = _otherColor;
            sheet["A" + currentRowNumber + ":B" + currentRowNumber].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
            currentRowNumber += 1;
            sheet["A" + currentRowNumber].Value = "Approver";
            sheet["B" + currentRowNumber].Value = approvedBy;
            sheet["B" + currentRowNumber].FillColor = _otherColor;
            sheet["A" + currentRowNumber + ":B" + currentRowNumber].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
            currentRowNumber += 1;
            sheet["A" + currentRowNumber].Value = "Status";
            sheet["B" + currentRowNumber].Value = wfStatus;
            sheet["B" + currentRowNumber].FillColor = _otherColor;
            sheet["A" + currentRowNumber + ":B" + currentRowNumber].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
            currentRowNumber += 1;
            sheet["A" + currentRowNumber].Value = "Approved Date";
            sheet["B" + currentRowNumber].Value = (wfStatus == Common.FormStatus.Approved || wfStatus == Common.FormStatus.Rejected) ? recordedDate?.ToString("dd/MM/yyyy hh:mm tt") : null;
            sheet["B" + currentRowNumber].FillColor = _otherColor;
            sheet["A" + currentRowNumber + ":B" + currentRowNumber].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
            currentRowNumber += 2;

            nextRowNumber = currentRowNumber;
        }

        private string CnMailBody(int formId)
        {
            try
            {
                var mailBody = string.Empty;

                using (var db = new kashflowDBEntities())
                {
                    var cnForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (cnForm != null)
                    {
                        var cnItems = db.ISSD_TradeSettlement.Where(x => x.FormId == cnForm.Id);

                        var pageUrl = string.Format("{0}" + Common.Email_FormUrlMap(cnForm.FormType) + "{1}", Config.EmailApplicationUrl, formId);

                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append("<p>Dear All, </p>");
                        bodyBuilder.AppendLine(string.Format("<p>Contribution Credited item in  <a href='" + pageUrl +
                                                             "'>#" + cnForm.Id + "</a> form has been " +
                                                             cnForm.FormStatus + " by " + cnForm.ApprovedBy));

                        using (var table = new Common.Table(bodyBuilder))
                        {
                            using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                            {
                                row.AddCell("#");
                                row.AddCell("Contribution Credited");
                                row.AddCell("Amount (+)");
                                row.AddCell("Remarks");
                            }

                            var count = 0;
                            foreach (var item in cnItems)
                            {
                                count += 1;

                                using (var row = table.AddRow())
                                {
                                    row.AddCell(count.ToString());
                                    row.AddCell(item.InstrumentCode);
                                    row.AddCell(item.AmountPlus.ToString("N"));
                                    row.AddCell(item.Remarks);
                                }
                            }
                        }

                        mailBody = bodyBuilder.ToString();
                        return mailBody;
                    }
                    else
                    {
                        _logger.LogError("CN Form not found. ID:" + formId);
                        return mailBody;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return string.Empty;
            }
        }
        private void SaveEmlFile(string filename, MailMessage message)
        {
            using (var client = new SmtpClient())
            {
                // create a temp folder to hold just this .eml file so that we can find it easily.
                var tempFolder = Path.Combine(Config.TempFolderPath, filename);

                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                client.UseDefaultCredentials = true;
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = tempFolder;
                client.Send(message);
            }
        }

        private bool EnableNotification(string configKey)
        {
            using (var db = new kashflowDBEntities())
            {
                var configValue = db.Config_Application.FirstOrDefault(x => x.Key == configKey);
                if (configValue != null)
                {
                    var conversionStatus = bool.TryParse(configValue.Value, out bool enableNotify);

                    if (conversionStatus)
                    {
                        return enableNotify;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private List<MailboxAddress> ReceipientsFromConfig(string configKey)
        {
            using (var db = new kashflowDBEntities())
            {
                var configValue = db.Config_Application.FirstOrDefault(x => x.Key == configKey);
                if (configValue != null)
                {
                    var emailAddresses = configValue.Value.Split(',').ToList();
                    var mailboxAddress = new List<MailboxAddress>();
                    foreach (var email in emailAddresses)
                    {
                        mailboxAddress.Add(MailboxAddress.Parse(email));
                    }
                    return mailboxAddress;
                }
                else
                {
                    return null;
                }
            }
        }

        private List<TsOpeningBalance> GetOpeningBalance(DateTime settlementDate, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<TsOpeningBalance>();

                    var ob = db.EDW_BankBalance
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                    && x.Currency == currency)
                        .GroupBy(x => new { x.SettlementDate, x.InstrumentType })
                        .Select(x => new
                        {
                            account = x.Key.InstrumentType,
                            total = x.Sum(y => y.Amount ?? 0)
                        });

                    foreach (var item in ob)
                    {
                        result.Add(new TsOpeningBalance()
                        {
                            Account = item.account,
                            Amount = item.total
                        });
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        #endregion
    }
}
