﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Logging;
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
            try
            {
                var landingPageData = _tsFormService.GetLandingPageData(User.Identity.Name);

                if (landingPageData == null)
                {
                    TempData["ErrorMessage"] = "Internal Server Error";
                    return View("Error");
                }

                return View("TradeSettlement/Index", landingPageData);
            }
            catch (Exception ex)
            {
                return xDcErrorPage(ex);
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/ConsolidatedView")]
        public ActionResult TsForm_ConsolidatedView(long settlementDateEpoch, string currency)
        {
            try
            {
                var consolidateFormView = _tsFormService.ViewConsolidatedForm(settlementDateEpoch, currency);
                if (consolidateFormView == null) return View("Error");

                return View("TradeSettlement/View", consolidateFormView);
            }
            catch (Exception ex)
            {
                return xDcErrorPage(ex);
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/View/{formId}")]
        public ActionResult TsForm_View(int formId)
        {
            try
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
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return xDcErrorPage(ex);
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit, Common.PermissionKey.ISSD_TradeSettlementForm_Admin_Edit)]
        [Route("TradeSettlement/Edit/{formId}")]
        public ActionResult TsForm_EditView(int formId)
        {
            try
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
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return xDcErrorPage(ex);
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/New/{formTypeId}")]
        public ActionResult TsForm_NewView(int formTypeId)
        {
            try
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
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return xDcErrorPage(ex);
            }
            
        }

        #region View Generated File

        [Route("TradeSettlement/Download/{id}")]
        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Download)]
        public ActionResult ViewPrinted(string id)
        {
            try
            {
                var outputFile = _tsFormService.GetGeneratedForm(id);

                if (outputFile != null)
                {
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + outputFile.FileName);

                    if (outputFile.FileExt == ".xlsx")
                    {
                        return File(outputFile.FileBytes, Common.ConvertIndexToContentType(4));
                    }
                    else
                    {
                        return File(outputFile.FileBytes, Common.ConvertIndexToContentType(11));
                    }

                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return xDcErrorPage(ex, "Generated file not found... sorry...");
            }
        }

        #endregion

        #region Generate CN Email


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/RetrieveCnEmail/{referenceId}")]
        public ActionResult RetrieveCnEmail(string referenceId)
        {
            // stream out the contents - don't need to dispose because File() does it for you
            var file = _tsFormService.GetGeneratedForm(referenceId, true);

            if (file != null)
            {
                Response.AddHeader("Content-Disposition", $"attachment; filename=Query Email - {DateTime.Now:ddMMyyyyhhmm}.eml");
                return File(file.FileBytes, Common.ConvertIndexToContentType(13));
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