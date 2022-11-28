using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Form;
using xDC.Utils;
using DevExpress.Spreadsheet;

namespace xDC.Services.FileGenerator
{
    public class FileGenerator: IFileGenerator
    {
        public string GenId_ConsolidatedTsForm(DateTime settlementDate, string currency, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.ISSD_FormHeader.Where(x =>
                            DbFunctions.TruncateTime(x.SettlementDate) == settlementDate.Date
                            && x.Currency == currency
                            && x.FormStatus == Common.FormStatus.Approved).ToList();

                    if (getForm.Any())
                    {
                        var associatedFormIdParts = getForm.Select(x => x.Id).ToList();

                        var getTrades = new List<ISSD_TradeSettlement>();
                        foreach (var formId in associatedFormIdParts)
                        {
                            getTrades.AddRange(db.ISSD_TradeSettlement.Where(x => x.FormId == formId).ToList());
                        }

                        var worflows = new List<Form_Workflow>();
                        foreach (var formId in associatedFormIdParts)
                        {
                            var workflow = db.Form_Workflow
                                .Where(x => x.WorkflowStatus == Common.FormStatus.Approved
                                            && x.FormId == formId)
                                .OrderByDescending(x => x.RecordedDate)
                                .FirstOrDefault();

                            if (workflow != null)
                            {
                                worflows.Add(workflow);
                            }
                        }

                        var getOpeningBalance = new List<TsOpeningBalance>();
                        var firstForm = getForm.FirstOrDefault();
                        if (firstForm != null)
                        {
                            getOpeningBalance = TsFormService.GetOpeningBalance(db, firstForm.SettlementDate.Value, firstForm.Currency);
                        }

                        IWorkbook workbook = GenerateDocumentConsolidated(settlementDate, currency, worflows, getTrades, getOpeningBalance);
                        var randomFileName = Common.DownloadedFileName.ISSD_TS_Consolidated + DateTime.Now.ToString("yyyyMMddHHmmss");

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
    }
}
