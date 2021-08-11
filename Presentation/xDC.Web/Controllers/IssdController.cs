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
    [Authorize(Roles = "Administrator, Power User, ISSD")]
    [RoutePrefix("issd")]
    public class IssdController : BaseController
    {
        public ActionResult Index()
        {
            using (var db = new kashflowDBEntities())
            {
                var isApprover = db.Config_Approver.Any(x => x.Username == User.Identity.Name);
                var isIisdUser = User.IsInRole(Config.AclIssd);

                var model = new ISSDLandingPageViewModel()
                {
                    IsAllowedToCreateForm = (!isApprover && isIisdUser)
                };
                
                return View(model);
            }
        }
        
        [Route("TradeSettlement")]
        public ActionResult TradeSettlement()
        {
            using (var db = new kashflowDBEntities())
            {
                var isApprover = db.Config_Approver.Any(x => x.Username == User.Identity.Name);
                var isIssdUser = User.IsInRole(Config.AclIssd);

                var model = new ISSDLandingPageViewModel()
                {
                    IsAllowedToCreateForm = (!isApprover && isIssdUser)
                };

                return View("TradeSettlement/Index", model);
            }


        }
        
        #region Trade Settlement Form Part A

        [Authorize(Roles = "ISSD")]
        [Route("TradeSettlement/PartA/New")]
        public ActionResult NewPartA()
        {
            var model = new ViewTradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(0),
                EnableDraftButton = true
            };

            return View("TradeSettlement/PartA/New", model);
        }

        [Route("TradeSettlement/PartA/View/{id}")]
        public ActionResult ViewPartA(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new ViewTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };

                        return View("TradeSettlement/PartA/View", formObj);
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


        [Authorize(Roles = "Power User, ISSD")]
        [Route("TradeSettlement/PartA/Edit/{formId}")]
        public ActionResult EditPartA(string formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdParsed = Convert.ToInt32(formId);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formIdParsed);

                    if (IsMeApprover(db))
                    {
                        TempData["ErrorMessage"] = "Unauthorized!";
                        return View("Error");
                    }

                    if (getForm != null)
                    {
                        var formObj = new EditTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,


                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0)),
                            EnableSaveAdminChanges = (User.IsInRole(Config.AclPowerUser)),

                        };

                        return View("TradeSettlement/PartA/Edit", formObj);
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

        #region Trade Settlement Form Part B

        [Authorize(Roles = "ISSD")]
        [Route("TradeSettlement/PartB/New")]
        public ActionResult NewPartB()
        {
            var model = new ViewTradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(0),
                EnableDraftButton = true
            };

            return View("TradeSettlement/PartB/New", model);
        }

        [Route("TradeSettlement/PartB/View/{id}")]
        public ActionResult ViewPartB(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new ViewTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };

                        return View("TradeSettlement/PartB/View", formObj);
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


        [Authorize(Roles = "Power User, ISSD")]
        [Route("TradeSettlement/PartB/Edit/{formId}")]
        public ActionResult EditPartB(string formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdParsed = Convert.ToInt32(formId);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formIdParsed);

                    if (IsMeApprover(db))
                    {
                        TempData["ErrorMessage"] = "Unauthorized!";
                        return View("Error");
                    }

                    if (getForm != null)
                    {
                        var formObj = new EditTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,


                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0)),
                            EnableSaveAdminChanges = (User.IsInRole(Config.AclPowerUser)),

                        };

                        return View("TradeSettlement/PartB/Edit", formObj);
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

        #region Trade Settlement Form Part C

        [Authorize(Roles = "ISSD")]
        [Route("TradeSettlement/PartC/New")]
        public ActionResult NewPartC()
        {
            var model = new ViewTradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(0),
                EnableDraftButton = true
            };

            return View("TradeSettlement/PartC/New", model);
        }

        [Route("TradeSettlement/PartC/View/{id}")]
        public ActionResult ViewPartC(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new ViewTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };

                        return View("TradeSettlement/PartC/View", formObj);
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


        [Authorize(Roles = "Power User, ISSD")]
        [Route("TradeSettlement/PartC/Edit/{formId}")]
        public ActionResult EditPartC(string formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdParsed = Convert.ToInt32(formId);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formIdParsed);

                    if (IsMeApprover(db))
                    {
                        TempData["ErrorMessage"] = "Unauthorized!";
                        return View("Error");
                    }

                    if (getForm != null)
                    {
                        var formObj = new EditTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,


                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0)),
                            EnableSaveAdminChanges = (User.IsInRole(Config.AclPowerUser)),

                        };

                        return View("TradeSettlement/PartC/Edit", formObj);
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

        #region Trade Settlement Form Part D

        [Authorize(Roles = "ISSD")]
        [Route("TradeSettlement/PartD/New")]
        public ActionResult NewPartD()
        {
            var model = new ViewTradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(0),
                EnableDraftButton = true
            };

            return View("TradeSettlement/PartD/New", model);
        }

        [Route("TradeSettlement/PartD/View/{id}")]
        public ActionResult ViewPartD(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new ViewTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };

                        return View("TradeSettlement/PartD/View", formObj);
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


        [Authorize(Roles = "Power User, ISSD")]
        [Route("TradeSettlement/PartD/Edit/{formId}")]
        public ActionResult EditPartD(string formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdParsed = Convert.ToInt32(formId);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formIdParsed);

                    if (IsMeApprover(db))
                    {
                        TempData["ErrorMessage"] = "Unauthorized!";
                        return View("Error");
                    }

                    if (getForm != null)
                    {
                        var formObj = new EditTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,


                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0)),
                            EnableSaveAdminChanges = (User.IsInRole(Config.AclPowerUser)),

                        };

                        return View("TradeSettlement/PartD/Edit", formObj);
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

        #region Trade Settlement Form Part E

        [Authorize(Roles = "ISSD")]
        [Route("TradeSettlement/PartE/New")]
        public ActionResult NewPartE()
        {
            var model = new ViewTradeSettlementFormViewModel()
            {
                PreparedBy = User.Identity.Name,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatusMapping(0),
                EnableDraftButton = true
            };

            return View("TradeSettlement/PartE/New", model);
        }

        [Route("TradeSettlement/PartE/View/{id}")]
        public ActionResult ViewPartE(string id)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var formObj = new ViewTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,

                            IsApproved = (getForm.FormStatus == Common.FormStatusMapping(3)),
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0) ||
                                              getForm.FormStatus == Common.FormStatusMapping(1)),

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,

                            ApprovePermission = getForm.ApprovedBy == User.Identity.Name,
                            AdminEditPermission = User.IsInRole(Config.AclPowerUser)
                        };

                        return View("TradeSettlement/PartE/View", formObj);
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


        [Authorize(Roles = "Power User, ISSD")]
        [Route("TradeSettlement/PartE/Edit/{formId}")]
        public ActionResult EditPartE(string formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdParsed = Convert.ToInt32(formId);
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formIdParsed);

                    if (IsMeApprover(db))
                    {
                        TempData["ErrorMessage"] = "Unauthorized!";
                        return View("Error");
                    }

                    if (getForm != null)
                    {
                        var formObj = new EditTradeSettlementFormViewModel()
                        {
                            Id = getForm.Id,
                            FormStatus = getForm.FormStatus,
                            SettlementDate = getForm.SettlementDate,
                            Currency = getForm.Currency,

                            PreparedBy = getForm.PreparedBy,
                            PreparedDate = getForm.PreparedDate,
                            ApprovedBy = getForm.ApprovedBy,
                            ApprovedDate = getForm.ApprovedDate,

                            IsAdminEdited = getForm.AdminEditted,
                            AdminEditedBy = getForm.AdminEdittedBy,
                            AdminEditedDate = getForm.AdminEdittedDate,


                            EnableDraftButton = (getForm.FormStatus == Common.FormStatusMapping(0)),
                            EnableSaveAdminChanges = (User.IsInRole(Config.AclPowerUser)),

                        };

                        return View("TradeSettlement/PartE/Edit", formObj);
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
        [Route("Print")]
        public ActionResult Print(string id, bool isExportAsExcel)
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

        [Route("ViewPrinted/{id}")]
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