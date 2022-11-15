using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Extension.MailGenerator;
using xDC_Web.Models;
using xDC_Web.ViewModels;
using xDC_Web.ViewModels.Fid;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.ISSD)]
    [RoutePrefix("issd")]
    public class IssdController : BaseController
    {
        #region Trade Settlement Form

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement")]
        public ActionResult TsLandingPage()
        {
            var landingPageData = TsFormService.GetLandingPageData(User.Identity.Name);

            if (landingPageData != null)
            {
                return View("TradeSettlement/Index", landingPageData);
            }
            else
            {
                TempData["ErrorMessage"] = "Internal Server Error";
                return View("Error");
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/View/")]
        public ActionResult TradeSettlementView(string settlementDateEpoch, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(Convert.ToInt64(settlementDateEpoch));
                    var settlementDateOnly = settlementDate.Value.Date;

                    var form = db.ISSD_FormHeader.Where(x =>
                        DbFunctions.TruncateTime(x.SettlementDate) == settlementDateOnly && x.Currency == currency);

                    if (form.Any())
                    {
                        var vm = new TradeSettlementFormVM()
                        {
                            FormStatus = form.First().FormStatus,
                            SettlementDate = form.First().SettlementDate,
                            Currency = form.First().Currency,
                            OpeningBalance = new List<TsOpeningBalance>()
                        };

                        var ob = TsFormService.GetOpeningBalance(db, settlementDateOnly, currency);
                        vm.OpeningBalance.AddRange(ob);
                        var totalOb = vm.OpeningBalance.Sum(x => x.Amount);

                        var totalFlow = TsFormService.GetTotalFlow(db, form.Select(x => x.Id).ToList(), settlementDateOnly, currency);

                        vm.ClosingBalance = totalOb + totalFlow.Inflow - totalFlow.Outflow;

                        return View("TradeSettlement/View", vm);
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

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/View/{id}")]
        public ActionResult ViewPart(string id)
        {
            return TS_View(id);
        }

        #region TS Part A (Equity)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartA/New")]
        public ActionResult NewPartA()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartA/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartA/View/{id}")]
        public ActionResult ViewPartA(string id)
        {
            return TS_View(id);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartA/Edit/{formId}")]
        public ActionResult EditPartA(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part B (BOND, CP, Notes/Papers, Coupon)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartB/New")]
        public ActionResult NewPartB()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartB/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartB/View/{id}")]
        public ActionResult ViewPartB(string id)
        {
            return TS_View(id);
        }


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartB/Edit/{formId}")]
        public ActionResult EditPartB(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part C (REPO)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartC/New")]
        public ActionResult NewPartC()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartC/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartC/View/{id}")]
        public ActionResult ViewPartC(string id)
        {
            return TS_View(id);
        }


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartC/Edit/{formId}")]
        public ActionResult EditPartC(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part D (MTM, FX)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartD/New")]
        public ActionResult NewPartD()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartD/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartD/View/{id}")]
        public ActionResult ViewPartD(string id)
        {
            return TS_View(id);
        }


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartD/Edit/{formId}")]
        public ActionResult EditPartD(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part E (ALTID)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartE/New")]
        public ActionResult NewPartE()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartE/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartE/View/{id}")]
        public ActionResult ViewPartE(string id)
        {
            return TS_View(id);
        }


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartE/Edit/{formId}")]
        public ActionResult EditPartE(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part F (Fees)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartF/New")]
        public ActionResult NewPartF()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartF/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartF/View/{id}")]
        public ActionResult ViewPartF(string id)
        {
            return TS_View(id);
        }


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartF/Edit/{formId}")]
        public ActionResult EditPartF(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part G (Contribution)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartG/New")]
        public ActionResult TS_PartG_New()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartG/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartG/View/{id}")]
        public ActionResult TS_PartG_View(string id)
        {
            return TS_View(id);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartG/Edit/{formId}")]
        public ActionResult TS_PartG_Edit(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region TS Part H (Others)

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartH/New")]
        public ActionResult TS_PartH_New()
        {
            var model = GenerateNewModel();
            return View("TradeSettlement/PartH/New", model);
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_View)]
        [Route("TradeSettlement/PartH/View/{id}")]
        public ActionResult TS_PartH_View(string id)
        {
            return TS_View(id);
        }


        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/PartH/Edit/{formId}")]
        public ActionResult TS_PartH_Edit(string formId)
        {
            return TS_Edit(formId);
        }

        #endregion

        #region View Generated File

        [Route("ViewPrinted/{id}")]
        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Download)]
        public ActionResult ViewPrinted(string id)
        {
            try
            {
                var genFileName = HttpUtility.HtmlDecode(id);

                var fileStream = new DocGeneratorBase().GetFile(genFileName);

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

        #region Generate CN Email

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [HttpPost]
        [Route("TradeSettlement/GenerateCnEmail")]
        public ActionResult GenerateCnEmail(int formId)
        {
            try
            {
                var userEmail = string.Empty;

                var findUser = new AuthService().GetUser(User.Identity.Name);
                if (findUser != null)
                {
                    userEmail = findUser.Email;
                }

                var referenceId = new CnEmailGenerator().GenerateMailFile(formId, userEmail);

                return Content(referenceId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Error in composing the Email Query. Please check with developer. (STEP 1)");
            }
        }

        [KflowAuthorize(Common.PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [Route("TradeSettlement/RetrieveCnEmail")]
        public ActionResult RetrieveCnEmail(string referenceId)
        {
            try
            {
                // stream out the contents - don't need to dispose because File() does it for you
                var fileStream = new CnEmailGenerator().GetFile(referenceId);

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
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }


        #endregion

        #region Private Methods

        private TradeSettlementFormVM GenerateViewModel(ISSD_FormHeader form, Form_Workflow wf)
        {
            return new TradeSettlementFormVM()
            {
                Id = form.Id,
                FormStatus = form.FormStatus,
                SettlementDate = form.SettlementDate,
                Currency = form.Currency,

                PreparedBy = form.PreparedBy,
                PreparedDate = form.PreparedDate,

                IsApproved = (form.FormStatus == Common.FormStatus.Approved),
                ApprovedBy = form.ApprovedBy,
                ApprovedDate = form.ApprovedDate,
                ApprovalNote = wf?.WorkflowNotes,

                IsAdminEdited = form.AdminEditted,
                AdminEditedBy = form.AdminEdittedBy,
                AdminEditedDate = form.AdminEdittedDate,

                EnableApproveRejectBtn = (User.IsInRole(Config.Acl.Issd) && form.ApprovedBy == User.Identity.Name && form.FormStatus == Common.FormStatus.PendingApproval),
                EnableReassign = (form.FormStatus == Common.FormStatus.PendingApproval && form.ApprovedBy != User.Identity.Name)
            };
        }

        private TradeSettlementFormVM GenerateEditModel(ISSD_FormHeader form, Form_Workflow wf)
        {
            return new TradeSettlementFormVM()
            {
                Id = form.Id,
                FormStatus = form.FormStatus,
                SettlementDate = form.SettlementDate,
                Currency = form.Currency,

                PreparedBy = form.PreparedBy,
                PreparedDate = form.PreparedDate,

                IsApproved = (form.FormStatus == Common.FormStatus.Approved),
                ApprovedBy = form.ApprovedBy,
                ApprovedDate = form.ApprovedDate,
                ApprovalNote = wf?.WorkflowNotes,

                IsAdminEdited = form.AdminEditted,
                AdminEditedBy = form.AdminEdittedBy,
                AdminEditedDate = form.AdminEdittedDate,

                EnableResubmit = (form.FormStatus == Common.FormStatus.Approved || form.FormStatus == Common.FormStatus.Rejected) && (!User.IsInRole(Config.Acl.PowerUser) && (form.ApprovedBy != User.Identity.Name)),
                EnableSubmitForApproval = (form.FormStatus == Common.FormStatus.Draft || form.FormStatus == Common.FormStatus.Draft) && (!User.IsInRole(Config.Acl.PowerUser)),

                EnableDraftButton = (form.FormStatus == Common.FormStatus.Draft) && (!User.IsInRole(Config.Acl.PowerUser)),
                EnableSaveAdminChanges = User.IsInRole(Config.Acl.PowerUser) && (form.FormStatus == Common.FormStatus.Approved),
                EnableApproveRejectBtn = (User.IsInRole(Config.Acl.Issd) && form.ApprovedBy == User.Identity.Name && form.FormStatus == Common.FormStatus.PendingApproval)
            };
        }

        private TradeSettlementFormVM GenerateNewModel()
        {
            return new TradeSettlementFormVM()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatus.Draft,
                EnableDraftButton = true
            };
        }

        private ActionResult TS_View(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var wf = WorkflowService.GetLatestWorkflow(db, form.Id, form.FormType);
                        var model = GenerateViewModel(form, wf);

                        switch (form.FormType)
                        {
                            case Common.FormType.ISSD_TS_A:
                                return View("TradeSettlement/PartA/View", model);
                            case Common.FormType.ISSD_TS_B:
                                return View("TradeSettlement/PartB/View", model);
                            case Common.FormType.ISSD_TS_C:
                                return View("TradeSettlement/PartC/View", model);
                            case Common.FormType.ISSD_TS_D:
                                return View("TradeSettlement/PartD/View", model);
                            case Common.FormType.ISSD_TS_E:
                                return View("TradeSettlement/PartE/View", model);
                            case Common.FormType.ISSD_TS_F:
                                return View("TradeSettlement/PartF/View", model);
                            case Common.FormType.ISSD_TS_G:
                                return View("TradeSettlement/PartG/View", model);
                            case Common.FormType.ISSD_TS_H:
                                return View("TradeSettlement/PartH/View", model);
                            default:
                                TempData["ErrorMessage"] = "Form Not found";
                                return View("Error");
                        }
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

        private ActionResult TS_Edit(string formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdParsed = Convert.ToInt32(formId);
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formIdParsed);

                    if (form != null)
                    {
                        if (TsFormService.EditFormRules(form.FormStatus, form.ApprovedBy, User.Identity.Name, out var errorMessage))
                        {
                            TempData["ErrorMessage"] = errorMessage;
                            return View("Error");
                        }

                        var wf = WorkflowService.GetLatestWorkflow(db, form.Id, form.FormType);
                        var model = GenerateEditModel(form, wf);

                        switch (form.FormType)
                        {
                            case Common.FormType.ISSD_TS_A:
                                return View("TradeSettlement/PartA/Edit", model);
                            case Common.FormType.ISSD_TS_B:
                                return View("TradeSettlement/PartB/Edit", model);
                            case Common.FormType.ISSD_TS_C:
                                return View("TradeSettlement/PartC/Edit", model);
                            case Common.FormType.ISSD_TS_D:
                                return View("TradeSettlement/PartD/Edit", model);
                            case Common.FormType.ISSD_TS_E:
                                return View("TradeSettlement/PartE/Edit", model);
                            case Common.FormType.ISSD_TS_F:
                                return View("TradeSettlement/PartF/Edit", model);
                            case Common.FormType.ISSD_TS_G:
                                return View("TradeSettlement/PartG/Edit", model);
                            case Common.FormType.ISSD_TS_H:
                                return View("TradeSettlement/PartH/Edit", model);
                            default:
                                TempData["ErrorMessage"] = "Form Not found";
                                return View("Error");
                        }
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

        #endregion

    }
}