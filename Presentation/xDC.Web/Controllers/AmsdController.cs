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
    [Authorize(Roles = "Administrator, Power User, AMSD")]
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

                    var isAmsdUser = User.IsInRole(Config.AclAmsd) || User.IsInRole(Config.AclAdministrator);

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

        [KashflowAuthorize(Roles = "AMSD")]
        public ActionResult NewInflowFundsForm()
        {
            var model = new ViewInflowFundStatusForm()
            {
                PreparedBy = User.Identity.Name,
                FormStatus = Common.FormStatusMapping(1)
            };

            return View(model);
        }

        [Authorize(Roles = "Power User, AMSD")]
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
                        var formObj = new ViewInflowFundStatusForm()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            FormStatus = getForm.FormStatus,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)

                        };
                        return View("NewInflowFundsForm", formObj);
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new HttpUnauthorizedResult();
            }
        }
        
        public ActionResult InflowFundsFormStatus(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

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
                            
                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };
                        return View(formObj);
                    }
                }
                return HttpNotFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }
        
        // Print Form
        [HttpPost]
        public ActionResult PrintInflowFund(string id)
        {
            try
            {
                var formId = Convert.ToInt32(id);

                var generatedDocumentFile = new InflowFundsFormDoc().GenerateExcelFile(formId);

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
                    var fileNameOnClient = id + ".xlsx";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileNameOnClient);
                    return File(fileStream, Common.ConvertIndexToContentType(4));
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
    }
}