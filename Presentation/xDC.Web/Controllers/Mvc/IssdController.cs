using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Services;
using xDC.Services.Form;
using xDC.Services.Membership;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels.Fid;

namespace xDC_Web.Controllers.Mvc
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.ISSD)]
    [RoutePrefix("issd")]
    public class IssdController : BaseController
    {
        #region Fields

        private readonly ITsFormService _tsFormService = Startup.Container.GetInstance<ITsFormService>();
        private readonly IUserManagementService _userService = Startup.Container.GetInstance<IUserManagementService>();
        private readonly IFcaTaggingFormService _fcaTaggingFormService = Startup.Container.GetInstance<IFcaTaggingFormService>();


        #endregion


        #region Trade Settlement Form

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement")]
        public ActionResult TsLandingPage()
        {
            var landingPageData = _tsFormService.GetLandingPageData(User.Identity.Name);

            if (landingPageData == null)
            {
                TempData["ErrorMessage"] = "Internal Server Error";
                return View("Error");
            }

            return View("TradeSettlement/Index", landingPageData);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/ConsolidatedView/")]
        public ActionResult TsForm_ConsolidatedView(string settlementDateEpoch, string currency)
        {
            var consolidateFormView = _tsFormService.ViewConsolidatedForm(settlementDateEpoch, currency);
            if (consolidateFormView == null) return View("Error");

            return View("TradeSettlement/View", consolidateFormView);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/View/{formId}")]
        public ActionResult TsForm_View(int formId)
        {
            var form = _tsFormService.ViewForm(formId, User.Identity.Name);
            if (form == null) return View("Error");

            switch (form.FormType)
            {
                case Common.FormType.ISSD_TS_A:
                    return View("TradeSettlement/PartA/View", form);
                case Common.FormType.ISSD_TS_B:
                    return View("TradeSettlement/PartB/View", form);
                case Common.FormType.ISSD_TS_C:
                    return View("TradeSettlement/PartC/View", form);
                case Common.FormType.ISSD_TS_D:
                    return View("TradeSettlement/PartD/View", form);
                case Common.FormType.ISSD_TS_E:
                    return View("TradeSettlement/PartE/View", form);
                case Common.FormType.ISSD_TS_F:
                    return View("TradeSettlement/PartF/View", form);
                case Common.FormType.ISSD_TS_G:
                    return View("TradeSettlement/PartG/View", form);
                case Common.FormType.ISSD_TS_H:
                    return View("TradeSettlement/PartH/View", form);
                default:
                    TempData["ErrorMessage"] = "Form Not found";
                    return View("Error");
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/Edit/{formId}")]
        public ActionResult TsForm_EditView(int formId)
        {
            var form = _tsFormService.ViewEditForm(formId, User.Identity.Name);
            if (form == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            switch (form.FormType)
            {
                case Common.FormType.ISSD_TS_A:
                    return View("TradeSettlement/PartA/Edit", form);
                case Common.FormType.ISSD_TS_B:
                    return View("TradeSettlement/PartB/Edit", form);
                case Common.FormType.ISSD_TS_C:
                    return View("TradeSettlement/PartC/Edit", form);
                case Common.FormType.ISSD_TS_D:
                    return View("TradeSettlement/PartD/Edit", form);
                case Common.FormType.ISSD_TS_E:
                    return View("TradeSettlement/PartE/Edit", form);
                case Common.FormType.ISSD_TS_F:
                    return View("TradeSettlement/PartF/Edit", form);
                case Common.FormType.ISSD_TS_G:
                    return View("TradeSettlement/PartG/Edit", form);
                case Common.FormType.ISSD_TS_H:
                    return View("TradeSettlement/PartH/Edit", form);
                default:
                    TempData["ErrorMessage"] = "Form Not found";
                    return View("Error");
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/New/{formTypeId}")]
        public ActionResult TsForm_NewView(int formTypeId)
        {
            var formType = Common.FormTypeMapping(formTypeId);
            var form = _tsFormService.ViewNewForm(formType, User.Identity.Name);
            if (form == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            switch (form.FormType)
            {
                case Common.FormType.ISSD_TS_A:
                    return View("TradeSettlement/PartA/New", form);
                case Common.FormType.ISSD_TS_B:
                    return View("TradeSettlement/PartB/New", form);
                case Common.FormType.ISSD_TS_C:
                    return View("TradeSettlement/PartC/New", form);
                case Common.FormType.ISSD_TS_D:
                    return View("TradeSettlement/PartD/New", form);
                case Common.FormType.ISSD_TS_E:
                    return View("TradeSettlement/PartE/New", form);
                case Common.FormType.ISSD_TS_F:
                    return View("TradeSettlement/PartF/New", form);
                case Common.FormType.ISSD_TS_G:
                    return View("TradeSettlement/PartG/New", form);
                case Common.FormType.ISSD_TS_H:
                    return View("TradeSettlement/PartH/New", form);
                default:
                    TempData["ErrorMessage"] = "Form Not found";
                    return View("Error");
            }
        }

        #region View Generated File

        [Route("TradeSettlement/Download/{id}")]
        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Download)]
        public ActionResult ViewPrinted(string id)
        {
            var generatedFileId = HttpUtility.HtmlDecode(id);
            var fileStream = _tsFormService.GetGeneratedForm(generatedFileId);

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

        #region Generate CN Email

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [HttpPost]
        [Route("TradeSettlement/GenerateCnEmail")]
        public ActionResult GenerateCnEmail(int formId)
        {
            var userEmail = string.Empty;

            var findUser = _userService.GetUser(User.Identity.Name);
            if (findUser != null)
            {
                userEmail = findUser.Email;
            }

            var referenceId = _tsFormService.GenCnMailId(formId, userEmail);

            return Content(referenceId);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/RetrieveCnEmail")]
        public ActionResult RetrieveCnEmail(string referenceId)
        {
            // stream out the contents - don't need to dispose because File() does it for you
            var fileStream = _tsFormService.GetGeneratedForm(referenceId);

            if (fileStream != null)
            {
                var responseHeaderValue = $"attachment; filename=Query Email - {DateTime.Now:ddMMyyyyhhmm}.eml";
                Response.AddHeader("Content-Disposition", responseHeaderValue);
                return File(fileStream, "application/vnd.ms-outlook");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }


        #endregion

        #endregion


        #region FCA Tagging

        [KflowAuthorize(Common.PermissionKey.ISSD_FcaTaggingForm)]
        [Route("FcaTagging")]
        public ActionResult FcaTagging()
        {
            return View("FcaTagging/Index");
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_FcaTaggingForm_Edit)]
        [Route("FcaTagging/Edit/{settlementDateEpoch}/{currency}")]
        public ActionResult FcaTaggingEdit(long settlementDateEpoch, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var model = new EditFcaAccountAssignmentVM
                    {
                        Currency = currency,
                        OpeningBalance = new List<TsOpeningBalance>()
                    };

                    if (settlementDate != null)
                    {
                        model.SettlementDate = settlementDate.Value;

                        var ob = FcaTaggingSvc.GetOpeningBalance(db, settlementDate.Value, currency);
                        model.OpeningBalance.AddRange(ob);
                        var totalOb = model.OpeningBalance.Sum(x => x.Amount);

                        var totalInflow = FcaTaggingSvc.TotalInflow(db, settlementDate.Value, currency);
                        var totalOutflow = FcaTaggingSvc.TotalOutflow(db, settlementDate.Value, currency);

                        model.ClosingBalance = totalOb + totalInflow - totalOutflow;
                        return View("FcaTagging/Edit", model);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid data - settlement date";
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

        [KflowAuthorize(Common.PermissionKey.ISSD_FcaTaggingForm)]
        [Route("FcaTagging/View/{settlementDateEpoch}/{currency}")]
        public ActionResult FcaTaggingView(long settlementDateEpoch, string currency)
        {
            try
            {
                var response = _fcaTaggingFormService.Page_FcaTaggingForm(settlementDateEpoch, currency, out bool reqStatus);

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

        #endregion

    }
}