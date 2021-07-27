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
                        var randomFileName = "IISD Trade Settlement - " + DateTime.Now.ToString("yyyyMMddHHmmss");

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

                int startingRownumber = 16;
                // equity
                var equity = trades.Where(x => x.InstrumentType == "Equity").ToList();
                if (equity.Any())
                {
                    sheet["A"+ startingRownumber + ":E" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A"+ startingRownumber + ":E" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Equity";
                    sheet["B" + startingRownumber].Value = "Stock Code/ ISIN";
                    sheet["C" + startingRownumber].Value = "Maturity (+)";
                    sheet["D" + startingRownumber].Value = "Sales (+)";
                    sheet["E" + startingRownumber].Value = "Purchase (-)";

                   ++startingRownumber;

                    foreach (var item in equity)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.InstrumentCode;
                        sheet["C" + startingRownumber].Value = item.Maturity;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["D" + startingRownumber].Value = item.Sales;
                        sheet["D" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["E" + startingRownumber].Value = item.Purchase;
                        sheet["E" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // bond
                var bond = trades.Where(x => x.InstrumentType == "Bond").ToList();
                if (bond.Any())
                {
                    sheet["A" + startingRownumber + ":E" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":E" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Bond";
                    sheet["B" + startingRownumber].Value = "Stock Code/ ISIN";
                    sheet["C" + startingRownumber].Value = "Maturity (+)";
                    sheet["D" + startingRownumber].Value = "Sales (+)";
                    sheet["E" + startingRownumber].Value = "Purchase (-)";

                    ++startingRownumber;

                    foreach (var item in bond)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.InstrumentCode;
                        sheet["C" + startingRownumber].Value = item.Maturity;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["D" + startingRownumber].Value = item.Sales;
                        sheet["D" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["E" + startingRownumber].Value = item.Purchase;
                        sheet["E" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // CP
                var cp = trades.Where(x => x.InstrumentType == "CP").ToList();
                if (cp.Any())
                {
                    sheet["A" + startingRownumber + ":E" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":E" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "CP";
                    sheet["B" + startingRownumber].Value = "Stock Code/ ISIN";
                    sheet["C" + startingRownumber].Value = "Maturity (+)";
                    sheet["D" + startingRownumber].Value = "Sales (+)";
                    sheet["E" + startingRownumber].Value = "Purchase (-)";

                    ++startingRownumber;

                    foreach (var item in cp)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.InstrumentCode;
                        sheet["C" + startingRownumber].Value = item.Maturity;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["D" + startingRownumber].Value = item.Sales;
                        sheet["D" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["E" + startingRownumber].Value = item.Purchase;
                        sheet["E" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // Notes & Papers
                var notes = trades.Where(x => x.InstrumentType == "Notes & Papers").ToList();
                if (notes.Any())
                {
                    sheet["A" + startingRownumber + ":E" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":E" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Notes & Papers";
                    sheet["B" + startingRownumber].Value = "Stock Code/ ISIN";
                    sheet["C" + startingRownumber].Value = "Maturity (+)";
                    sheet["D" + startingRownumber].Value = "Sales (+)";
                    sheet["E" + startingRownumber].Value = "Purchase (-)";

                    ++startingRownumber;

                    foreach (var item in notes)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.InstrumentCode;
                        sheet["C" + startingRownumber].Value = item.Maturity;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["D" + startingRownumber].Value = item.Sales;
                        sheet["D" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["E" + startingRownumber].Value = item.Purchase;
                        sheet["E" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // REPO
                var repo = trades.Where(x => x.InstrumentType == "REPO").ToList();
                if (repo.Any())
                {
                    sheet["A" + startingRownumber + ":D" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":D" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Repo";
                    sheet["B" + startingRownumber].Value = "Stock Code/ ISIN";
                    sheet["C" + startingRownumber].Value = "1st Leg (+)";
                    sheet["D" + startingRownumber].Value = "2nd Leg (-)";

                    ++startingRownumber;

                    foreach (var item in repo)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.InstrumentCode;
                        sheet["C" + startingRownumber].Value = item.FirstLeg;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["D" + startingRownumber].Value = item.SecondLeg;
                        sheet["D" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // Coupon Received
                var coupon = trades.Where(x => x.InstrumentType == "Coupon Received").ToList();
                if (coupon.Any())
                {
                    sheet["A" + startingRownumber + ":C" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":C" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Coupon Received";
                    sheet["B" + startingRownumber].Value = "Stock Code/ ISIN";
                    sheet["C" + startingRownumber].Value = "Amount (+)";

                    ++startingRownumber;

                    foreach (var item in coupon)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.InstrumentCode;
                        sheet["C" + startingRownumber].Value = item.AmountPlus;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // Fees
                var fees = trades.Where(x => x.InstrumentType == "Fees").ToList();
                if (fees.Any())
                {
                    sheet["A" + startingRownumber + ":B" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":B" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Fees";
                    sheet["B" + startingRownumber].Value = "Amount (+)";

                    ++startingRownumber;

                    foreach (var item in fees)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.AmountPlus;
                        sheet["B" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // Payment/ Received (MTM)
                var mtm = trades.Where(x => x.InstrumentType == "Payment/ Received (MTM)").ToList();
                if (mtm.Any())
                {
                    sheet["A" + startingRownumber + ":C" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":C" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Coupon Received";
                    sheet["B" + startingRownumber].Value = "Amount (+)";
                    sheet["C" + startingRownumber].Value = "Amount (-)";

                    ++startingRownumber;

                    foreach (var item in mtm)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.AmountPlus;
                        sheet["B" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["C" + startingRownumber].Value = item.AmountMinus;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // FX Settlement
                var fx = trades.Where(x => x.InstrumentType == "FX Settlement").ToList();
                if (fx.Any())
                {
                    sheet["A" + startingRownumber + ":C" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":C" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "FX Settlement";
                    sheet["B" + startingRownumber].Value = "Amount (+)";
                    sheet["C" + startingRownumber].Value = "Amount (-)";

                    ++startingRownumber;

                    foreach (var item in fx)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.AmountPlus;
                        sheet["B" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["C" + startingRownumber].Value = item.AmountMinus;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // Contribution credited
                var contribution = trades.Where(x => x.InstrumentType == "Contribution Credited").ToList();
                if (contribution.Any())
                {
                    sheet["A" + startingRownumber + ":B" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":B" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Contribution credited";
                    sheet["B" + startingRownumber].Value = "Amount (+)";

                    ++startingRownumber;

                    foreach (var item in fx)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.AmountPlus;
                        sheet["B" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // ALTID Distribution & Drawdown
                var altid = trades.Where(x => x.InstrumentType == "ALTID Distribution & Drawdown").ToList();
                if (altid.Any())
                {
                    sheet["A" + startingRownumber + ":C" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":C" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "ALTID Distribution & Drawdown";
                    sheet["B" + startingRownumber].Value = "Amount (+)";
                    sheet["C" + startingRownumber].Value = "Amount (-)";

                    ++startingRownumber;

                    foreach (var item in altid)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.AmountPlus;
                        sheet["B" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["C" + startingRownumber].Value = item.AmountMinus;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }

                // Others
                var others = trades.Where(x => x.InstrumentType == "Others").ToList();
                if (others.Any())
                {
                    sheet["A" + startingRownumber + ":C" + startingRownumber].FillColor = Color.AliceBlue;
                    sheet["A" + startingRownumber + ":C" + startingRownumber].Font.Bold = true;

                    sheet["A" + startingRownumber].Value = "Others";
                    sheet["B" + startingRownumber].Value = "Amount (+)";
                    sheet["C" + startingRownumber].Value = "Amount (-)";

                    ++startingRownumber;

                    foreach (var item in others)
                    {
                        sheet["A" + startingRownumber].Value = item.InstrumentType;
                        sheet["B" + startingRownumber].Value = item.AmountPlus;
                        sheet["B" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                        sheet["C" + startingRownumber].Value = item.AmountMinus;
                        sheet["C" + startingRownumber].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                        ++startingRownumber;
                    }

                    ++startingRownumber;
                }
                
                var footerRowNumber = startingRownumber + 4;
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