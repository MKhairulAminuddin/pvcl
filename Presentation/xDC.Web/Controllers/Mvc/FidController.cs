using System;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.Form;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels.Fid.Treasury;

namespace xDC_Web.Controllers.Mvc
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.FID)]
    [RoutePrefix("fid")]
    public class FidController : Controller
    {
        #region Fields

        private readonly ITreasuryFormService _tFormService = Startup.Container.GetInstance<ITreasuryFormService>();
        private readonly IFcaTaggingFormService _fcaTaggingService = Startup.Container.GetInstance<IFcaTaggingFormService>();

        #endregion


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
                var response = _fcaTaggingService.Page_FcaTaggingForm(settlementDateEpoch, currency, out bool reqStatus);

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
                var response = _fcaTaggingService.Page_FcaTaggingForm(settlementDateEpoch, currency, out bool reqStatus);

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
            var formModel = _tFormService.GetEditPageData(id, User.Identity.Name);
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

        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_View)]
        [Route("Treasury/View/{id}")]
        public ActionResult TreasuryView(int id)
        {
            var data = _tFormService.GetViewPageData(id, User.Identity.Name);

            if (data != null)
            {
                return View("Treasury/View", data);
            }
            else
            {
                TempData["ErrorMessage"] = "Form Not found";
                return View("Error");
            }
        }

        [Route("Treasury/Download/{id}")]
        [KflowAuthorize(Common.PermissionKey.FID_TreasuryForm_Download)]
        public ActionResult ViewPrinted(string id)
        {
            var generatedFileId = HttpUtility.HtmlDecode(id);
            var fileStream = _tFormService.GetGeneratedForm(generatedFileId);

            if (fileStream == null)
            {
                TempData["ErrorMessage"] = "Generated file not found... sorry...";
                return View("Error");
            }

            var newFileName = Common.GetFileName(fileStream);
            Response.AddHeader("Content-Disposition", "attachment; filename=" + newFileName);

            if (Common.GetFileExt(fileStream) == ".xlsx")
            {
                return File(fileStream, Common.ConvertIndexToContentType(4));
            }
            else
            {
                return File(fileStream, Common.ConvertIndexToContentType(11));
            }
        }

        #endregion

    }
}