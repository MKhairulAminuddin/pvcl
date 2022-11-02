using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.ISSD_TS;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Extension.MailMerge;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.DealCutOff;
using xDC_Web.ViewModels.Fid.Treasury;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.FID)]
    [RoutePrefix("fid")]
    public class FidController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        #region FCA Tagging Form

        [KflowAuthorize(Common.PermissionKey.FID_FcaTaggingForm)]
        [Route("FcaTagging")]
        public ActionResult FcaTagging()
        {
            return View("FcaTagging/Index");
        }

        [KflowAuthorize(Common.PermissionKey.FID_FcaTaggingForm)]
        [Route("FcaTagging/View/{settlementDateEpoch}/{currency}")]
        public ActionResult FcaTaggingView(long settlementDateEpoch, string currency)
        {
            try
            {
                var response = new FcaTaggingFormService().Page_FcaTaggingForm(settlementDateEpoch, currency, out bool reqStatus);

                if (reqStatus)
                {
                    return View("FcaTagging/View", response);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid data - settlement date";
                    return View("Error");
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        [KflowAuthorize(Common.PermissionKey.FID_FcaTaggingForm_Edit)]
        [Route("FcaTagging/Edit/{settlementDateEpoch}/{currency}")]
        public ActionResult FcaTaggingEdit(long settlementDateEpoch, string currency)
        {
            try
            {
                var response = new FcaTaggingFormService().Page_FcaTaggingForm(settlementDateEpoch, currency, out bool reqStatus);

                if (reqStatus)
                {
                    return View("FcaTagging/Edit", response);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid data - settlement date";
                    return View("Error");
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        #endregion

        #region Treasury Form

        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_View)]
        public ActionResult Treasury()
        {
            return View("Treasury/Index");
        }

        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_Edit)]
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

        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_Edit)]
        [Route("Treasury/Edit/{id}")]
        public ActionResult TreasuryEdit(int id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formModel = TreasuryFormService.GetEditPageData(id, User.Identity.Name);
                    if (formModel != null)
                    {
                        return View("Treasury/Edit", formModel);
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

        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_View)]
        [Route("Treasury/View/{id}")]
        public ActionResult TreasuryView(int id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formModel = TreasuryFormService.GetViewPageData(id, User.Identity.Name);

                    if (formModel != null)
                    {
                        return View("Treasury/View", formModel);
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

        [HttpPost]
        [Route("Treasury/Print")]
        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_Download)]
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

        [Route("Treasury/Printed/{id}")]
        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_Download)]
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