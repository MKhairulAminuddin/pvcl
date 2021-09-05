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
    public class InflowFundsFormDoc : DocGeneratorBase
    {
        public string GenerateExcelFile(int formId, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var getInflowFunds = db.AMSD_IF_Item.Where(x => x.FormId == formId).ToList();

                        getForm.Id = getForm.Id;
                        getForm.PreparedBy = getForm.PreparedBy;
                        getForm.PreparedDate = getForm.PreparedDate.Value;

                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected) && x.FormId == getForm.Id)
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

                        IWorkbook workbook = new Workbook();
                        workbook.Options.Culture = new CultureInfo("en-US");
                        workbook.LoadDocument(MapPath("~/App_Data/Inflow Funds Template.xltx"));
                        workbook = GenerateDocument(workbook, getForm, getFormWorkflow, getInflowFunds);
                        var randomFileName = "AMSD Inflow Fund Form - " + DateTime.Now.ToString("yyyyMMddHHmmss");

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

        private IWorkbook GenerateDocument(IWorkbook workbook, AMSD_IF formHeader, Form_Workflow formWorkflow, List<AMSD_IF_Item> inflowFunds)
        {
            workbook.BeginUpdate();
            try
            {
                int startingRownumber = 6;
                var sheet = workbook.Worksheets[0];
                
                sheet["E2"].Value = formHeader.PreparedBy;
                if (formHeader.PreparedDate != null)
                {
                    sheet["E3"].Value = formHeader.PreparedDate.Value.ToString("dd/MM/yyyy HH:ss");
                }

                sheet["E5"].Value = formHeader.ApprovedBy;
                if (formHeader.ApprovedDate != null)
                {
                    sheet["E6"].Value = formHeader.ApprovedDate.Value.ToString("dd/MM/yyyy HH:ss");

                    sheet["D7:E7"].CopyFrom(sheet["D6:E6"]);
                    sheet["D7"].Value = "Notes";
                    sheet["E7"].Value = formWorkflow.WorkflowNotes;
                    startingRownumber += 1;
                }

                if (formHeader.AdminEdittedDate != null)
                {
                    sheet["D9:E10"].CopyFrom(sheet["D5:E6"]);
                    sheet["D9"].Value = "Admin Edit";
                    sheet["E9"].Value = formHeader.AdminEdittedBy;
                    sheet["D10"].Value = "Admin Edit Date";
                    sheet["E10"].Value = formHeader.AdminEdittedDate.Value.ToString("dd/MM/yyyy HH:ss");
                    
                    startingRownumber += 3;
                }

                startingRownumber += 1;
                sheet["A8:C8"].MoveTo(sheet["A" + startingRownumber + ":C" + startingRownumber]);
                startingRownumber += 2;
                sheet["A10:C10"].MoveTo(sheet["A" + startingRownumber + ":C" + startingRownumber]);
                
                startingRownumber += 1;
                int actualDataRowNumber = startingRownumber;
                foreach (var item in inflowFunds)
                {
                    sheet["A" + startingRownumber].Value = item.FundType;
                    sheet["B" + startingRownumber].Value = item.Bank;
                    sheet["C" + startingRownumber].Value = item.Amount;

                    ++startingRownumber;
                }
                sheet["A" + startingRownumber + ":B" + startingRownumber].Merge();
                sheet["A" + startingRownumber].Value = "Total";
                sheet["A" + startingRownumber].Font.Bold = true;

                sheet["C" + actualDataRowNumber + ":C" + startingRownumber].NumberFormat =
                    "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                sheet["C" + startingRownumber].FormulaInvariant = "=SUM($C$" + actualDataRowNumber + ":$C$" + (startingRownumber - 1) + ")";

                sheet["A" + startingRownumber + ":C" + startingRownumber].Borders.TopBorder.Color = Color.Black;
                sheet["A" + startingRownumber + ":C" + startingRownumber].Borders.TopBorder.LineStyle = BorderLineStyle.Thick;


                var footerRowNumber = startingRownumber + 4;
                sheet["A" + footerRowNumber + ":E" + footerRowNumber].Merge();
                sheet["A" + footerRowNumber + ":E" + footerRowNumber].Value = "Generated on " + DateTime.Now.ToString("dd/MM/yyyy HH:ss") + " by "+ HttpContext.Current.User.Identity.Name;
                sheet["A" + footerRowNumber + ":E" + footerRowNumber].Font.Italic = true;
                sheet["A" + footerRowNumber + ":E" + footerRowNumber].Font.Size = 10;
                sheet["A" + footerRowNumber + ":E" + footerRowNumber].Font.Color = Color.LightSlateGray;
                sheet["A" + footerRowNumber + ":E" + footerRowNumber].Alignment.Horizontal =
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