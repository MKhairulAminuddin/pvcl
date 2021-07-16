using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using xDC.Infrastructure.Application;
using xDC.Logging;

namespace xDC_Web.Extension.DocGenerator
{
    public class InflowFundsFormDoc : DocGeneratorBase
    {
        public string GenerateExcelFile(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var getInflowFunds = db.Amsd_InflowFunds.Where(x => x.FormId == formId).ToList();

                        getForm.Id = getForm.Id;
                        getForm.PreparedBy = getForm.PreparedBy;
                        getForm.PreparedDate = getForm.PreparedDate.Value;

                        DocumentFormat documentFormat = DocumentFormat.Xlsx;
                        IWorkbook workbook = new Workbook();
                        workbook.Options.Culture = new CultureInfo("en-US");
                        workbook.LoadDocument(MapPath("~/App_Data/Inflow Funds Template.xltx"));
                        workbook = GenerateDocument(workbook, getForm, getInflowFunds);

                        var docBytes = workbook.SaveDocument(documentFormat);
                        var ms = new MemoryStream(docBytes);
                        
                        var randomFileName = "AMSD Inflow Fund Form - " + DateTime.Now.ToString("yyyyMMddHHmmss");
                        SaveExcelFile(randomFileName, ms);

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

        private IWorkbook GenerateDocument(IWorkbook workbook, Form_Header formHeader, List<Amsd_InflowFunds> inflowFunds)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                sheet["E2"].Value = formHeader.PreparedBy;
                sheet["E3"].Value = formHeader.PreparedDate;

                int startingRownumber = 11;
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

                sheet["C11:C" + startingRownumber].NumberFormat =
                    "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                sheet["C" + startingRownumber].FormulaInvariant = "=SUM($C$11:$C$" + (startingRownumber - 1) + ")";

                sheet["A" + startingRownumber + ":C" + startingRownumber].Borders.TopBorder.Color = Color.Black;
                sheet["A" + startingRownumber + ":C" + startingRownumber].Borders.TopBorder.LineStyle = BorderLineStyle.Thick;


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