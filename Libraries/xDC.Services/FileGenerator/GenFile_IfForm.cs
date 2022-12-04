using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using xDC.Domain.Form;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Form;
using xDC.Services.Workflow;
using xDC.Utils;

namespace xDC.Services.FileGenerator
{
    public class GenFile_IfForm: FileGenerator, IGenFile_IfForm
    {
        #region Fields

        private readonly IXDcLogger _logger;
        private readonly IWorkflowService _wfService;

        #endregion

        #region Ctor

        public GenFile_IfForm(IWorkflowService wfService, IXDcLogger logger)
        {
            _wfService = wfService;
            _logger = logger;
        }

        #endregion

        public string GenId_IfForm(int formId, string currentUser, bool isExportAsExcel)
        {
            IWorkbook workbook = GenDoc_IfForm(formId, currentUser);
            if (workbook == null) return null;

            return SaveAndGenDocId(workbook, Common.DownloadedFileName.AMSD_IF, isExportAsExcel);
        }

        private IWorkbook GenDoc_IfForm(int formId, string currentUser)
        {
            IWorkbook workbook = new Workbook();
            workbook.Options.Culture = new CultureInfo("en-US");
            workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.AMSD_IF));

            workbook.BeginUpdate();
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return null;

                    var formItems = db.AMSD_IF_Item.Where(x => x.FormId == form.Id).ToList();

                    var formWf = _wfService.Info(form.Id, form.FormType)
                            .Where(x => (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected))
                            .FirstOrDefault();


                    int startingRownumber = 6;
                    var sheet = workbook.Worksheets[0];

                    sheet["E2"].Value = form.PreparedBy;
                    if (form.PreparedDate != null)
                    {
                        sheet["E3"].Value = form.PreparedDate.Value.ToString("dd/MM/yyyy HH:ss");
                    }

                    sheet["E5"].Value = form.ApprovedBy;
                    if (form.ApprovedDate != null)
                    {
                        sheet["E6"].Value = form.ApprovedDate.Value.ToString("dd/MM/yyyy HH:ss");

                        sheet["D7:E7"].CopyFrom(sheet["D6:E6"]);
                        sheet["D7"].Value = "Notes";
                        sheet["E7"].Value = formWf?.WorkflowNotes;
                        startingRownumber += 1;
                    }

                    if (form.AdminEdittedDate != null)
                    {
                        sheet["D9:E10"].CopyFrom(sheet["D5:E6"]);
                        sheet["D9"].Value = "Admin Edit";
                        sheet["E9"].Value = form.AdminEdittedBy;
                        sheet["D10"].Value = "Admin Edit Date";
                        sheet["E10"].Value = form.AdminEdittedDate.Value.ToString("dd/MM/yyyy HH:ss");

                        startingRownumber += 3;
                    }

                    startingRownumber += 1;
                    sheet["A8:C8"].MoveTo(sheet["A" + startingRownumber + ":C" + startingRownumber]);
                    startingRownumber += 2;
                    sheet["A10:C10"].MoveTo(sheet["A" + startingRownumber + ":C" + startingRownumber]);

                    startingRownumber += 1;
                    int actualDataRowNumber = startingRownumber;
                    foreach (var item in formItems)
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
                    sheet["A" + footerRowNumber + ":E" + footerRowNumber].Value = "Generated on " + DateTime.Now.ToString("dd/MM/yyyy HH:ss") + " by " + currentUser;
                    sheet["A" + footerRowNumber + ":E" + footerRowNumber].Font.Italic = true;
                    sheet["A" + footerRowNumber + ":E" + footerRowNumber].Font.Size = 10;
                    sheet["A" + footerRowNumber + ":E" + footerRowNumber].Font.Color = Color.LightSlateGray;
                    sheet["A" + footerRowNumber + ":E" + footerRowNumber].Alignment.Horizontal =
                        SpreadsheetHorizontalAlignment.Right;


                    workbook.Calculate();
                }

            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

    }
}
