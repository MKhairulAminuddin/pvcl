using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DevExpress.Spreadsheet;
using xDC.Infrastructure.Application;
using xDC_Web.Extension.MailMerge;
using xDC_Web.Models.MailMerge;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, Amsd")]
    public class AmsdController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewInflowFundsForm()
        {
            return View();
        }
        

        public ActionResult ViewInflowFundsForm(string id)
        {
            try
            {
                var formId = Convert.ToInt32(id);
                AmsdInflowFundFormPreviewModel model = new AmsdInflowFundFormPreviewModel();
                model.Id = formId;

                return View(model);
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
            
        }
        

        public ActionResult SpreadsheetMailMergePreview(AmsdInflowFundFormPreviewModel model)
        {
            try
            {
                var formId = Convert.ToInt32(model.Id);
                AmsdInflowFundFormPreviewModel formModel = new AmsdInflowFundFormPreviewModel();

                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        formModel.Id = getForm.Id;
                        formModel.Preparer = getForm.CreatedBy;
                        formModel.PreparedDate = getForm.CreatedDate.Value;
                        formModel.InflowFunds = new List<AmsdInflowFundItems>();

                        foreach (var item in getForm.AmsdInflowFunds)
                        {
                            formModel.InflowFunds.Add(new AmsdInflowFundItems()
                            {
                                Id = item.Id,
                                FundType = item.FundType,
                                Bank = item.Bank,
                                Amount = item.Amount.Value
                            });
                        }

                        SpreadsheetPreviewModel previewModel = new SpreadsheetPreviewModel();
                        previewModel.Workbook = CreateSpreadsheetMailMergeWorkbook(formModel);
                        return GenerateHtmlPreview(previewModel);
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
        }

        IWorkbook CreateSpreadsheetMailMergeWorkbook(AmsdInflowFundFormPreviewModel model)
        {
            AmsdInflowFundFormGenerator generator = new AmsdInflowFundFormGenerator();
            return generator.GenerateDocument(model);
        }

        protected ContentResult GenerateHtmlPreview(SpreadsheetPreviewModel previewModel)
        {
            ContentResult result = new ContentResult();
            result.ContentType = "text/html";
            result.Content = GenerateHtmlContentForWorkbook(previewModel);
            return result;
        }
        string GenerateHtmlContentForWorkbook(SpreadsheetPreviewModel previewModel)
        {
            MemoryStream stream = new MemoryStream();
            HtmlContentGenerator htmlGenerator = new HtmlContentGenerator(stream);
            htmlGenerator.Generate(previewModel.Workbook, previewModel.PreviewSheetIndex);
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}