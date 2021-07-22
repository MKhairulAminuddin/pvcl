using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Models;
using xDC_Web.ViewModels;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, IISD")]
    public class IisdController : BaseController
    {
        public ActionResult Index()
        {
            using (var db = new kashflowDBEntities())
            {
                var isApprover = db.Config_Approver.Any(x => x.Username == User.Identity.Name);
                var isIisdUser = User.IsInRole(Config.AclIisd);

                var model = new IisdLandingPageViewModel()
                {
                    IsAllowedToCreateForm = (!isApprover && isIisdUser)
                };
                
                return View(model);
            }

            
        }

        [Authorize(Roles = "IISD")]
        public ActionResult NewTradeSettlement()
        {
            var model = new TradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(1),
                IsDraftEnabled = true
            };

            return View(model);
        }

        public ActionResult TradeSettlementFormStatus(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new TradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            FormDate = getForm.FormDate,
                            FormCurrency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsDraftEnabled = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };
                        return View(formObj);
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

        public ActionResult EditTradeSettlementForm(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        if (!User.IsInRole(Config.AclAdministrator) &&
                            (getForm.FormStatus == Common.FormStatusMapping(2) || getForm.FormStatus == Common.FormStatusMapping(3)))
                        {
                            TempData["ErrorMessage"] = "Current form status prohibited you from editing it.";
                            return View("Error");
                        }

                        var formObj = new TradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            FormStatus = getForm.FormStatus,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsDraftEnabled = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)

                        };

                        return View("NewTradeSettlement", formObj);
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

        #region Print Form

        [HttpPost]
        public ActionResult PrintTradeSettlement(string id, bool isExportAsExcel)
        {
            try
            {
                var formId = Convert.ToInt32(id);

                var generatedDocumentFile = new TradeSettlementFormDoc().GenerateFile(formId, isExportAsExcel);

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

        public ActionResult GetPrintTradeSettlement(string id)
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