using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using DevExpress.Spreadsheet;
using xDC.Infrastructure.Application;
using xDC.Services;
using xDC.Utils;
using xDC_Web.Extension.MailMerge;
using xDC_Web.Models;
using xDC_Web.Models.MailMerge;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, Amsd")]
    public class AmsdController : BaseController
    {
        public ActionResult Index()
        {
            var x = new MailService();
            x.SendSubmitForApprovalEmail(4);
            return View();
        }

        public ActionResult NewInflowFundsForm()
        {
            var model = new ViewInflowFundStatusForm()
            {
                PreparedBy = User.Identity.Name,
                FormStatus = Common.FormStatusMapping(1)
            };

            return View(model);
        }

        public ActionResult EditInflowFundsForm(string id)
        {
            using (var db = new kashflowDBEntities())
            {
                var formId = Convert.ToInt32(id);
                var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                if (getForm != null)
                {
                    var formObj = new ViewInflowFundStatusForm()
                    {
                        Id = getForm.Id,
                        PreparedBy = getForm.PreparedBy,
                        PreparedDate = getForm.PreparedDate,
                        ApprovedBy = getForm.ApprovedBy,
                        ApprovedDate = getForm.ApprovedDate,
                        FormStatus = getForm.FormStatus,

                        ApprovePermission = false // to ganti with workflow checking
                    };
                    return View("NewInflowFundsForm", formObj);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            
        }

        public ActionResult InflowFundsFormStatus(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm!= null)
                    {
                        var formObj = new ViewInflowFundStatusForm()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,
                            FormStatus = getForm.FormStatus,

                            ApprovePermission = false // to ganti with workflow checking
                        };
                        return View(formObj);
                    }
                }
                return HttpNotFound();
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
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
                        formModel.Preparer = getForm.PreparedBy;
                        formModel.PreparedDate = getForm.PreparedDate.Value;
                        formModel.InflowFunds = new List<AmsdInflowFundItems>();

                        var getInflowFunds = db.Amsd_InflowFunds.Where(x => x.FormId == getForm.Id);

                        foreach (var item in getInflowFunds)
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

        public ActionResult SpreadsheetDocumentTemplateExportTo(AmsdInflowFundFormPreviewModel model)
        {
            AmsdInflowFundFormGenerator generator = new AmsdInflowFundFormGenerator();
            IWorkbook workbook = generator.GenerateDocument(model);
            

            MemoryStream stream = new MemoryStream();
            DocumentFormat documentFormat = Common.ConvertIndexToFormat(model.ExportToDocumentType);
            if (documentFormat == DocumentFormat.Undefined)
                workbook.ExportToPdf(stream);
            else
                workbook.SaveDocument(stream, documentFormat);

            string contentType = Common.ConvertIndexToContentType(model.ExportToDocumentType);
            string fileExtension = Common.ConvertIndexToFileExtension(model.ExportToDocumentType);


            return CreateFileStreamResult(stream, contentType, fileExtension);
        }



        [HttpPost]
        // called first
        public ActionResult PrintInflowFund(string id)
        {
            try
            {
                var formId = Convert.ToInt32(id);
                AmsdInflowFundFormPreviewModel formModel = new AmsdInflowFundFormPreviewModel();

                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        formModel.Id = getForm.Id;
                        formModel.Preparer = getForm.PreparedBy;
                        formModel.PreparedDate = getForm.PreparedDate.Value;
                        formModel.InflowFunds = new List<AmsdInflowFundItems>();

                        var getInflowFunds = db.Amsd_InflowFunds.Where(x => x.FormId == getForm.Id);

                        foreach (var item in getInflowFunds)
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

                        DocumentFormat documentFormat = DocumentFormat.Xlsx;
                        IWorkbook workbook = previewModel.Workbook;
                        var docBytes = workbook.SaveDocument(documentFormat);
                        var ms = new MemoryStream(docBytes);

                        var randomFileName = Guid.NewGuid().ToString();
                        SaveFile(randomFileName, ms);

                        return Content(id);
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


        public ActionResult GetPrintInflowFund(string id)
        {
            try
            {
                // stream out the contents - don't need to dispose because File() does it for you
                var fileStream = GetFile(id);

                if (fileStream != null)
                {
                    Response.AddHeader("Content-Disposition", "attachment; filename=test.xlsx");
                    return File(fileStream, Common.ConvertIndexToContentType(4));
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
        }
        

        public static FileStream GetFile(string filename)
        {
            var tempFolder = "D:\\";

            tempFolder = Path.Combine(tempFolder, "xDCPrintedFiles");

            tempFolder = Path.Combine(tempFolder, filename);

            // tempFolder should contain 1 eml file
            var filePath = Directory.GetFiles(tempFolder).SingleOrDefault();

            if (!string.IsNullOrEmpty(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Open);
                return fs;
            }
            else
            {
                return null;
            }

        }

        public static void SaveFile(string filename, MemoryStream ms)
        {
            try
            {
                var tempFolder = "D:\\";
                tempFolder = Path.Combine(tempFolder, "xDCPrintedFiles");

                // create a temp folder to hold just this .eml file so that we can find it easily.
                tempFolder = Path.Combine(tempFolder, filename + ".xlsx");

                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                FileStream file = new FileStream(tempFolder, FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();
                ms.Close();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            

            
        }
    }
}