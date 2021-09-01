﻿using System;
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
    [Authorize(Roles = "Administrator, Power User, AMSD")]
    [RoutePrefix("amsd")]
    public class AmsdController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                var model = new AmsdViewModel();

                using (var db = new kashflowDBEntities())
                {
                    var isApprover = db.Config_Approver.Any(x => x.Username == User.Identity.Name);
                    var isAmsdUser = User.IsInRole(Config.Acl.Amsd) || User.IsInRole(Config.Acl.Administrator);

                    model.isAllowedToCreateForm = (!isApprover && isAmsdUser);

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

                    var model = new ViewInflowFundStatusForm()
                    {
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = Common.FormStatus.Draft,
                        IsDraftEnabled = true
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

        [Route("InflowFund/Edit")]
        public ActionResult EditInflowFundsForm(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        if (!User.IsInRole(Config.Acl.PowerUser) &&
                            (getForm.FormStatus == Common.FormStatus.PendingApproval || getForm.FormStatus == Common.FormStatus.Approved))
                        {
                            TempData["ErrorMessage"] = "Hey! You cannot amend form that in Pending Approval or Approved status!";
                            return View("Error");
                        }

                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == "Approved" || x.WorkflowStatus == "Rejected") &&
                                        x.FormId == getForm.Id).OrderByDescending(x => x.EndDate)
                            .FirstOrDefault();

                        var formObj = new ViewInflowFundStatusForm()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            FormStatus = getForm.FormStatus,

                            IsApproved = (getForm.FormStatus == Common.FormStatus.Approved),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsDraftEnabled = (getForm.FormStatus == Common.FormStatus.Draft),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.Acl.PowerUser),

                            ApprovalOrRejectionNotes = getFormWorkflow?.WorkflowNotes

                        };
                        return View("InflowFund/New", formObj);
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

        [Route("InflowFund/View")]
        public ActionResult InflowFundsFormStatus(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var isApprovedOrRejected = (getForm.FormStatus == Common.FormStatus.Approved ||
                                                    getForm.FormStatus == Common.FormStatus.Rejected);
                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == "Approved" || x.WorkflowStatus == "Rejected") &&
                                        x.FormId == getForm.Id).OrderByDescending(x => x.EndDate)
                            .FirstOrDefault();

                        var formObj = new ViewInflowFundStatusForm()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,
                            FormStatus = getForm.FormStatus,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.Acl.PowerUser),

                            IsApprovedOrRejected = isApprovedOrRejected,
                            ApprovalOrRejectionNotes = getFormWorkflow?.WorkflowNotes
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

        [HttpPost]
        public ActionResult PrintInflowFund(string id, bool isExportAsExcel)
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

        public ActionResult GetPrintInflowFund(string id)
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