using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.ISSD_TS;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Extension.MailMerge;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.DealCutOff;
using xDC_Web.ViewModels.Fid.Treasury;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("fid")]
    public class FidController : Controller
    {
        // GET: Fid
        public ActionResult Index()
        {
            return View();
        }

        #region 10 AM Cut Off

        [Route("Summary")]
        public ActionResult Summary()
        {
            return View("TenAmCutOff/Summary");
        }

        [Route("FcaTagging")]
        public ActionResult FcaTagging()
        {
            return View("TenAmCutOff/FcaTagging");
        }

        [Route("FcaTagging/Edit/{formId}")]
        public ActionResult FcaTaggingEdit(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_TS10.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var model = new EditFcaAccountAssignmentVM
                        {
                            Currency = form.Currency,
                            OpeningBalance = new List<TS_OpeningBalance>()
                        };
                        
                        if (form.SettlementDate != null)
                        {
                            model.SettlementDate = form.SettlementDate.Value;

                            var ob = FcaTaggingSvc.GetOpeningBalance(db, form.SettlementDate.Value, form.Currency);
                            model.OpeningBalance.AddRange(ob);
                            var totalOb = model.OpeningBalance.Sum(x => x.Amount);

                            var totalFlow = FcaTaggingSvc.GetTotalFlow(db, form.Id, form.SettlementDate.Value, form.Currency);

                            model.ClosingBalance = totalOb + totalFlow.Inflow - totalFlow.Outflow;
                        }

                        return View("TenAmCutOff/FcaTaggingEdit", model);

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Form Not found";
                        return View("Error");
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        #endregion

        #region Treasury

        public ActionResult Treasury()
        {
            return View("Treasury/Index");
        }

        [Route("Treasury/New")]
        public ActionResult TreasuryNew()
        {
            var model = new TreasuryFormVM
            {
                FormStatus = Common.FormStatus.Draft,
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
            };
            return View("Treasury/New", model);
        }

        [Route("Treasury/Edit/{id}")]
        public ActionResult TreasuryEdit(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var model = new TreasuryFormVM
                        {
                            Currency = form.Currency,
                            TradeDate = form.TradeDate,
                            FormStatus = form.FormStatus,
                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,
                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,

                            EnableSubmitForApproval = (form.FormStatus != Common.FormStatus.PendingApproval && form.ApprovedBy == null),
                            EnableSaveAsDraftBtn = (form.FormStatus == Common.FormStatus.Draft && form.ApprovedBy == null)
                        };
                        return View("Treasury/Edit", model);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Form Not found";
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
            
        }

        [Route("Treasury/View/{id}")]
        public ActionResult TreasuryView(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var wf = db.Form_Workflow
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault(x =>
                                x.FormId == form.Id && x.FormType == Common.FormType.FID_TREASURY &&
                                (x.WorkflowStatus == Common.FormStatus.Approved ||
                                 x.WorkflowStatus == Common.FormStatus.Rejected));

                        var formVm = new TreasuryFormVM()
                        {
                            Id = form.Id,
                            FormStatus = form.FormStatus,
                            TradeDate = form.TradeDate,
                            Currency = form.Currency,
                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,
                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,
                            ApprovalNotes = wf?.WorkflowNotes,

                            EnableApproveBtn = form.ApprovedBy == User.Identity.Name && form.FormStatus == Common.FormStatus.PendingApproval,
                            EnableRejectBtn = form.ApprovedBy == User.Identity.Name && form.FormStatus == Common.FormStatus.PendingApproval,
                        };
                        return View("Treasury/View", formVm);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Form Not found";
                        return View("Error");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                TempData["ErrorMessage"] = "Hmmm... Something went wrong here...";
                return View("Error");
            }
            
        }

        #endregion

        #region Deal Cut Off

        public ActionResult DealCutOffMyr()
        {
            DealCutOffMyrTemplateModel model = new DealCutOffMyrTemplateModel();
            model.PreviewModel.Workbook = new DealCutOffForm_MYR().GenerateWorkbook(DateTime.Now);

            return View("DealCutOff/Myr", model);
        }

        [HttpPost]
        [Route("DealCutOffMyr/Print")]
        public ActionResult DealCutOffMyr_Print(string TradeDate, bool isExportAsExcel)
        {
            try
            {
                DateTime? selectedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(TradeDate))
                {
                    selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
                }

                var generatedDocumentFile = new DealCutOffForm_MYR().GenerateFile(selectedDate, isExportAsExcel);

                if (!string.IsNullOrEmpty(generatedDocumentFile))
                {
                    return Content(generatedDocumentFile);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }

        [Route("DealCutOffMyr/Printed/{id}")]
        public ActionResult DealCutOffMyr_ViewPrinted(string id)
        {
            try
            {
                var fileStream = new DocGeneratorBase().GetFile(id);

                if (fileStream != null)
                {
                    var fileName = Common.GetFileName(fileStream);
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

                    if (Common.GetFileExt(fileStream) == ".xlsx")
                    {
                        return File(fileStream, Common.ConvertIndexToContentType(4));
                    }
                    else
                    {
                        return File(fileStream, Common.ConvertIndexToContentType(11));
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "Generated file not found... sorry...";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        public ActionResult DealCutOffMyrPreview(string TradeDate)
        {
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(TradeDate))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
            }

            var previewModel = new SpreadsheetPreviewModel
            {
                Workbook = new DealCutOffForm_MYR().GenerateWorkbook(selectedDate)
            };
            return GenerateHtmlPreview(previewModel);
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

        #endregion

        #region Print Form

        [HttpPost]
        [Route("Print")]
        public ActionResult Print(string id, bool isExportAsExcel)
        {
            try
            {
                var formId = Convert.ToInt32(id);

                var generatedDocumentFile = new TreasuryFormDoc().GenerateFile(formId, isExportAsExcel);

                if (!string.IsNullOrEmpty(generatedDocumentFile))
                {
                    return Content(generatedDocumentFile);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }

        [Route("Printed/{id}")]
        public ActionResult ViewPrinted(string id)
        {
            try
            {
                var fileStream = new DocGeneratorBase().GetFile(id);

                if (fileStream != null)
                {
                    var fileName = Common.GetFileName(fileStream);
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

                    if (Common.GetFileExt(fileStream) == ".xlsx")
                    {
                        return File(fileStream, Common.ConvertIndexToContentType(4));
                    }
                    else
                    {
                        return File(fileStream, Common.ConvertIndexToContentType(11));
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "Generated file not found... sorry...";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        #endregion
    }
}