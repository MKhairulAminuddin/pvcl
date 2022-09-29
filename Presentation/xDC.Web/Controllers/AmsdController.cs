using System;
using System.Linq;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Models;
using xDC_Web.Models.MailMerge;
using xDC_Web.ViewModels;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.InflowFundForm)]
    [RoutePrefix("amsd")]
    public class AmsdController : BaseController
    {
        [KflowAuthorize(Common.PermissionKey.InflowFundForm_View)]
        public ActionResult Index()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isAmsdUser = User.IsInRole(Config.Acl.Amsd) || User.IsInRole(Config.Acl.Administrator);

                    var model = new AmsdLandingPageVM()
                    {
                        EnableCreateForm = isAmsdUser
                    };

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return View("Error");
            }
        }


        #region Inflow Fund Form Page

        [KflowAuthorize(Common.PermissionKey.InflowFundForm_Creator)]
        [Route("InflowFund/New")]
        public ActionResult InflowFund_New()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    // only AMSD user can create new form
                    var isApprover = db.Config_Approver.Any(x => x.Username == User.Identity.Name);
                    if (!User.IsInRole(Config.Acl.Amsd) && !isApprover)
                    {
                        TempData["ErrorMessage"] = "You are not authorized to create new Inflow Fund form...";
                        return View("Error");
                    }

                    var model = new InflowFundStatusFormVM()
                    {
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = Common.FormStatus.Draft,
                        EnableDraftButton = true
                    };

                    return View("InflowFund/New", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return View("Error");
            }
        }

        [KflowAuthorize(Common.PermissionKey.InflowFundForm_Creator)]
        [Route("InflowFund/Edit/{id}")]
        public ActionResult InflowFund_Edit(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected) &&
                                        x.FormId == getForm.Id).OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

                        var formObj = new InflowFundStatusFormVM()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            FormStatus = getForm.FormStatus,

                            IsApproved = (getForm.FormStatus == Common.FormStatus.Approved),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            EnableResubmit = (getForm.FormStatus == Common.FormStatus.Approved || getForm.FormStatus == Common.FormStatus.Rejected) 
                                             && (!User.IsInRole(Config.Acl.PowerUser) 
                                                 && (getForm.ApprovedBy != User.Identity.Name)),
                            EnableSubmitForApproval = (getForm.FormStatus == Common.FormStatus.Draft || getForm.FormStatus == Common.FormStatus.Draft) 
                                                      && (!User.IsInRole(Config.Acl.PowerUser)),

                            EnableDraftButton = (getForm.FormStatus == Common.FormStatus.Draft) && (!User.IsInRole(Config.Acl.PowerUser)),
                            EnableSaveAdminChanges = User.IsInRole(Config.Acl.PowerUser) && (getForm.FormStatus == Common.FormStatus.Approved),
                            EnableApproveRejectBtn = (User.IsInRole(Config.Acl.Issd) && getForm.ApprovedBy == User.Identity.Name && getForm.FormStatus == Common.FormStatus.PendingApproval)

                        };
                        return View("InflowFund/Edit", formObj);
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

        [KflowAuthorize(Common.PermissionKey.InflowFundForm_View)]
        [Route("InflowFund/View/{formId}")]
        public ActionResult InflowFundsFormStatus(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var isApprovedOrRejected = (form.FormStatus == Common.FormStatus.Approved ||
                                                    form.FormStatus == Common.FormStatus.Rejected);
                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected) &&
                                        x.FormId == form.Id).OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

                        var formObj = new InflowFundStatusFormVM()
                        {
                            Id = form.Id,
                            FormStatus = form.FormStatus,

                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,

                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,
                            ApprovalNote = getFormWorkflow?.WorkflowNotes,
                            
                            IsAdminEdited = form.AdminEditted,
                            AdminEditedBy = form.AdminEdittedBy,
                            AdminEditedDate = form.AdminEdittedDate,
                            
                            EnableReassign = (form.FormStatus == Common.FormStatus.PendingApproval && form.ApprovedBy != User.Identity.Name),
                            IsApproved = (form.FormStatus == Common.FormStatus.Approved),
                            EnableResubmit = (form.FormStatus == Common.FormStatus.Approved || form.FormStatus == Common.FormStatus.Rejected)
                                             && (!User.IsInRole(Config.Acl.PowerUser)
                                                 && (form.ApprovedBy != User.Identity.Name)),
                            EnableSubmitForApproval = (form.FormStatus == Common.FormStatus.Draft || form.FormStatus == Common.FormStatus.Draft)
                                                      && (!User.IsInRole(Config.Acl.PowerUser)),
                            EnableDraftButton = (form.FormStatus == Common.FormStatus.Draft) && (!User.IsInRole(Config.Acl.PowerUser)),
                            EnableSaveAdminChanges = User.IsInRole(Config.Acl.PowerUser) && (form.FormStatus == Common.FormStatus.Approved),
                            EnableApproveRejectBtn = (User.IsInRole(Config.Acl.Amsd) && form.ApprovedBy == User.Identity.Name && form.FormStatus == Common.FormStatus.PendingApproval)
                        };

                        return View("InflowFund/View", formObj);
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


        #region Print Form

        [KflowAuthorize(Common.PermissionKey.InflowFundForm_Download)]
        [HttpPost]
        [Route("Print")]
        public ActionResult Print(string id, bool isExportAsExcel)
        {
            try
            {
                var formId = Convert.ToInt32(id);

                var generatedDocumentFile = new InflowFundsFormDoc().GenerateExcelFile(formId, isExportAsExcel);

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

        [KflowAuthorize(Common.PermissionKey.InflowFundForm_Download)]
        [HttpGet]
        [Route("Printed/{id}")]
        public ActionResult Printed(string id)
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