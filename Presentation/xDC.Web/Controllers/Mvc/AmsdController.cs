using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.Form;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Models;
using xDC_Web.Models.MailMerge;
using xDC_Web.ViewModels;

namespace xDC_Web.Controllers.Mvc
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.AMSD)]
    [RoutePrefix("amsd")]
    public class AmsdController : BaseController
    {
        #region Fields

        private readonly IIfFormService _ifFormService;

        #endregion

        #region Ctor

        public AmsdController(IIfFormService ifFormService)
        {
            _ifFormService = ifFormService;
        }

        #endregion

        #region Inflow Fund Form Page

        [KflowAuthorize(Common.PermissionKey.AMSD_InflowFundForm_View)]
        public ActionResult Index()
        {
            var data = _ifFormService.GetLandingPage(User.Identity.Name);

            if (data == null)
            {
                TempData["ErrorMessage"] = "Internal Server Error.";
                return View("Error");
            }

            return View("InflowFund/Index", data);
        }

        [KflowAuthorize(Common.PermissionKey.AMSD_InflowFundForm_Edit)]
        [Route("InflowFund/New")]
        public ActionResult InflowFund_New()
        {
            var model = new InflowFundForm()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatus.Draft,
                EnableDraftButton = true
            };

            return View("InflowFund/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.AMSD_InflowFundForm_Edit)]
        [Route("InflowFund/Edit/{id}")]
        public ActionResult InflowFund_Edit(string id)
        {
            var formId = Convert.ToInt32(id);
            var form = _ifFormService.EditFormPage(formId, User.Identity.Name);

            if (form == null)
            {
                TempData["ErrorMessage"] = "Form Not found";
                return View("Error");
            }

            return View("InflowFund/Edit", form);
        }

        [KflowAuthorize(Common.PermissionKey.AMSD_InflowFundForm_View)]
        [Route("InflowFund/View/{formId}")]
        public ActionResult InflowFundsFormStatus(int formId)
        {
            var form = _ifFormService.ViewFormPage(formId, User.Identity.Name);

            if (form == null)
            {
                TempData["ErrorMessage"] = "Form Not found";
                return View("Error");
            }

            return View("InflowFund/View", form);
        }

        #endregion

        #region Print Form

        [KflowAuthorize(Common.PermissionKey.AMSD_InflowFundForm_Download)]
        [HttpPost]
        [Route("InflowFund/Print")]
        public ActionResult Print(string id, bool isExportAsExcel)
        {
            var formId = Convert.ToInt32(id);

            var generatedDocumentId = new InflowFundsFormDoc().GenerateExcelFile(formId, isExportAsExcel);

            if (string.IsNullOrEmpty(generatedDocumentId)) 
                return HttpNotFound();

            return Content(generatedDocumentId);
        }

        [KflowAuthorize(Common.PermissionKey.AMSD_InflowFundForm_Download)]
        [HttpGet]
        [Route("InflowFund/Printed/{id}")]
        public ActionResult Printed(string id)
        {
            var fileStream = new DocGeneratorBase().GetFile(id);

            if (fileStream == null)
            {
                TempData["ErrorMessage"] = "Generated file not found... sorry...";
                return View("Error");
            }

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

        #endregion
    }
}