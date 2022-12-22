using DevExpress.Xpo.Logger;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using xDC.Domain.Form;
using xDC.Domain.Web;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Domain.Web.Application;
using xDC.Domain.WebApi.Forms.InflowFund;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Audit;
using xDC.Services.FileGenerator;
using xDC.Services.Membership;
using xDC.Services.Notification;
using xDC.Services.Workflow;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.Form
{
    public class IfFormService : FormService, IIfFormService
    {

        #region Fields

        private readonly IAuditService _auditService;
        private readonly IEmailNotification _emailNotification;
        private readonly IRoleManagementService _roleService;
        private readonly IGenFile_IfForm _genFile;

        #endregion

        #region Ctor

        public IfFormService(IWorkflowService wfService, INotificationService notifyService, IAuditService auditService, IRoleManagementService roleService, IEmailNotification emailNotification, IGenFile_IfForm genFile)
            : base(wfService, notifyService, auditService, roleService)
        {
            _auditService = auditService;
            _emailNotification = emailNotification;
            _roleService = roleService;
            _genFile = genFile;
        }

        #endregion

        #region Pages

        public LandingPage GetLandingPage(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var enableCreateForm = _roleService.IsUserHaveAccess(currentUser, PermissionKey.AMSD_InflowFundForm_Edit);
                    var today = DateTime.Now;

                    var model = new LandingPage()
                    {
                        CountTodaySubmission = db.AMSD_IF.Count(x => x.FormType == FormType.AMSD_IF
                                            && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                            && x.FormStatus != FormStatus.Draft),
                        CountTodayPendingApproval = db.AMSD_IF
                                                        .Count(x => x.FormType == FormType.AMSD_IF
                                                                && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == FormStatus.PendingApproval),
                        CountTodayApproved = db.AMSD_IF
                                                        .Count(x => x.FormType == FormType.AMSD_IF
                                                                && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == FormStatus.Approved),
                        CountTodayRejected = db.AMSD_IF
                                                        .Count(x => x.FormType == FormType.AMSD_IF
                                                                && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == FormStatus.Rejected),
                        EnableCreateForm = enableCreateForm
                    };

                    return model;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public InflowFundForm ViewFormPage(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var isApprovedOrRejected = form.FormStatus == FormStatus.Approved || form.FormStatus == FormStatus.Rejected;
                        var latestWorkflow = db.Form_Workflow
                            .Where(x => x.Id == form.Id && x.FormType == form.FormType && (x.WorkflowStatus.Contains(FormStatus.Approved) || x.WorkflowStatus.Contains(FormStatus.Rejected)))
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

                        var formView = new InflowFundForm()
                        {
                            Id = form.Id,
                            FormStatus = form.FormStatus,

                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,

                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,
                            ApprovalNote = latestWorkflow?.WorkflowNotes,

                            IsAdminEdited = form.AdminEditted,
                            AdminEditedBy = form.AdminEdittedBy,
                            AdminEditedDate = form.AdminEdittedDate,

                            IsApproved = form.FormStatus == FormStatus.Approved,

                            EnableReassign = this.EnableReassignApprover(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.AMSD_InflowFundForm_Edit),
                            EnableResubmit = this.EnableResubmission(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.AMSD_InflowFundForm_Edit),
                            
                            EnableSubmitForApproval = this.EnableApprovalSubmission(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.AMSD_InflowFundForm_Edit),
                            
                            EnableDraftButton = form.FormStatus == FormStatus.Draft,
                            EnableEditDraftBtn = form.FormStatus == FormStatus.Draft && form.PreparedBy == currentUser,
                            EnableSaveAdminChanges = _roleService.IsUserHaveAccess(currentUser, PermissionKey.AMSD_InflowFundForm_Admin_Edit) && form.FormStatus == FormStatus.Approved,

                            EnableApproveRejectBtn = form.ApprovedBy == currentUser && form.FormStatus == FormStatus.PendingApproval
                        };

                        return formView;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public InflowFundForm EditFormPage(int formId, string currentUser)
        {
            var form = ViewFormPage(formId, currentUser);

            return form == null ? null : form;
        }

        #endregion

        #region Read Form Data

        public List<IfHomeGrid1> GetHomeGrid(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todayDate = DateTime.Now.Date;

                    var tsForms = db.AMSD_IF;
                    var resultVM = new List<IfHomeGrid1>();

                    foreach (var item in tsForms)
                    {
                        resultVM.Add(new IfHomeGrid1
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormDate = item.FormDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = this.EnableEdit(item.FormStatus, currentUser, PermissionKey.AMSD_InflowFundForm_Edit) ||
                                            this.EnableEdit(item.FormStatus, currentUser, PermissionKey.AMSD_InflowFundForm_Admin_Edit),
                            EnableDelete = this.EnableDelete(item.FormStatus, item.ApprovedBy, currentUser, PermissionKey.AMSD_InflowFundForm_Edit),
                            EnablePrint = this.EnablePrint(currentUser, item.FormStatus, PermissionKey.AMSD_InflowFundForm_Download),
                            EnableRetractSubmission = this.EnableFormWithdrawal(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.AMSD_InflowFundForm_Edit),

                            IsRejected = currentUser == item.PreparedBy && item.FormStatus == FormStatus.Rejected,
                            IsPendingMyApproval = currentUser == item.ApprovedBy && item.FormStatus == FormStatus.PendingApproval,
                            IsPendingApproval = item.FormStatus == FormStatus.PendingApproval
                        });
                    }

                    return resultVM;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public List<AMSD_IF_Item> GetFormItems(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    return db.AMSD_IF_Item.Where(x => x.FormId == formId).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public List<IfFormSummaryList> IfFormSummaryList(long submissionDateEpoch = 0)
        {
            try
            {
                DateTime selectedDate;
                if (submissionDateEpoch != 0)
                {
                    selectedDate = ConvertEpochToDateTime(submissionDateEpoch).Value;
                }
                else
                {
                    selectedDate = DateTime.Now;
                }

                using (var db = new kashflowDBEntities())
                {
                    var ifForms = db.AMSD_IF.Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate)).Select(x => new IfFormSummaryList()
                    {
                        FormId = x.Id,
                        PreparedBy = x.PreparedBy,
                        ApprovedBy = x.ApprovedBy,
                        FormStatus = x.FormStatus,
                        SubmittedDate = x.PreparedDate,
                        ApprovedDate = x.ApprovedDate
                    }).ToList();

                    if (ifForms.Any())
                    {
                        return ifForms;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public List<IfAmountSummary> IfAmountSummary(long submissionDateepoch = 0)
        {
            try
            {
                DateTime selectedDate;
                if (submissionDateepoch != 0)
                {
                    selectedDate = ConvertEpochToDateTime(submissionDateepoch).Value;
                }
                else
                {
                    selectedDate = DateTime.Now;
                }

                using (var db = new kashflowDBEntities())
                {
                    var ifFormIds = db.AMSD_IF.Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate) && x.FormStatus == FormStatus.Approved).Select(x => x.Id).ToList();
                    if (ifFormIds.Any())
                    {
                        var ifFormitems = db.AMSD_IF_Item.Where(x => ifFormIds.Contains(x.FormId)).GroupBy(x => x.Bank).Select(x => new IfAmountSummary()
                        {
                            Bank = x.Key,
                            TotalAmount = x.Sum(y => y.Amount)
                        }).ToList();
                        return ifFormitems;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }



        #endregion

        #region Form Action

        public int CreateForm(IfFormPage input, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var ifForm = new AMSD_IF()
                    {
                        FormType = FormType.AMSD_IF,
                        Currency = "MYR",
                        PreparedBy = currentUser,
                        PreparedDate = DateTime.Now,
                        FormDate = DateTime.Now,
                        ApprovedBy = input.Approver,
                    };
                    ifForm.FormStatus = (!string.IsNullOrEmpty(ifForm.ApprovedBy)) ? 
                                            FormStatus.PendingApproval : 
                                            FormStatus.Draft;
                    db.AMSD_IF.Add(ifForm);
                    var createdForm = db.SaveChanges();

                    if (createdForm < 1) return 0;

                    

                    var ifFormItems = new List<AMSD_IF_Item>();
                    foreach (var item in input.IfItems)
                    {
                        ifFormItems.Add(new AMSD_IF_Item()
                        {
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount,
                            ModifiedBy = currentUser,
                            ModifiedDate = DateTime.Now
                        });
                    }
                    // inject items with generated form ID
                    ifFormItems.ForEach(x => x.FormId = ifForm.Id);
                    db.AMSD_IF_Item.AddRange(ifFormItems);
                    var createdFormItems = db.SaveChanges();

                    if (createdFormItems < 1)
                    {
                        // remove back since not success create form items
                        db.AMSD_IF.Remove(ifForm);
                        db.SaveChanges();
                        return 0;
                    }

                    if (ifForm.FormStatus == FormStatus.PendingApproval)
                    {
                        Create(ifForm.Id, ifForm.FormType, ifForm.FormDate, ifForm.PreparedBy, ifForm.ApprovedBy, input.ApprovalNotes);
                    }

                    if (ifForm.FormStatus == FormStatus.Draft)
                    {
                        CreateAsDraft(ifForm.Id, ifForm.FormType, ifForm.FormDate, currentUser);   
                    }

                    return ifForm.Id;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return 0;
            }
        }

        public int EditForm(int formId, IfFormPage input, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var existingForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                    if (existingForm == null) return 0;

                    // if power user editted
                    if (input.IsSaveAdminEdit)
                    {
                        existingForm.AdminEditted = true;
                        existingForm.AdminEdittedBy = currentUser;
                        existingForm.AdminEdittedDate = DateTime.Now;
                        _auditService.FA_AdminEdit(existingForm.Id, existingForm.FormType, existingForm.FormDate, currentUser);
                    }
                    else
                    {
                        // resubmission - approved to pending approval
                        if (existingForm.FormStatus == FormStatus.Approved && !string.IsNullOrEmpty(input.Approver))
                        {
                            FormResubmission(existingForm.Id, existingForm.FormType, existingForm.FormDate, existingForm.PreparedBy, input.Approver, currentUser);
                            existingForm.PreparedBy = currentUser;
                            existingForm.PreparedDate = DateTime.Now;
                            existingForm.ApprovedBy = input.Approver;
                            existingForm.ApprovedDate = null;
                            existingForm.FormStatus = FormStatus.PendingApproval;
                        }

                        // submit draft for approval
                        if (existingForm.FormStatus == FormStatus.Draft && !string.IsNullOrEmpty(input.Approver))
                        {
                            existingForm.PreparedBy = currentUser;
                            existingForm.PreparedDate = DateTime.Now;
                            existingForm.ApprovedBy = input.Approver;
                            existingForm.ApprovedDate = null;
                            existingForm.FormStatus = FormStatus.PendingApproval;
                        }

                        // from draft save to draft again
                        if (existingForm.FormStatus == FormStatus.Draft && string.IsNullOrEmpty(input.Approver))
                        {
                            existingForm.PreparedBy = currentUser;
                            existingForm.PreparedDate = DateTime.Now;
                        }
                    }

                    var saveFormChanges = db.SaveChanges();
                    if (saveFormChanges < 1) return 0;


                    if (input.IfItems.Any())
                    {
                        var existingFormItems = db.AMSD_IF_Item.Where(x => x.FormId == formId);

                        //delete existing
                        var existingItemInGrid = input.IfItems.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems = existingFormItems.Where(x => !existingItemInGrid.Contains(x.Id));

                        if (removedItems.Any())
                        {
                            foreach (var item in removedItems)
                            {
                                _auditService.FA_RemoveRow(formId, existingForm.FormType, existingForm.FormDate, currentUser,
                                    $"{item.FundType}, {item.Bank}, {item.Amount}");
                            }
                            db.AMSD_IF_Item.RemoveRange(removedItems);
                        }

                        foreach (var item in input.IfItems)
                        {
                            if (item.Id != 0)
                            {
                                //edit existing
                                var itemInDb = existingFormItems.FirstOrDefault(x => x.Id == item.Id);
                                if (itemInDb != null)
                                {
                                    if (itemInDb.Amount != item.Amount)
                                    {
                                        _auditService.FA_EditRow(formId, existingForm.FormType,
                                            existingForm.FormDate, currentUser, itemInDb.Amount.ToString(),
                                            item.Amount.ToString(), "Amount");

                                        itemInDb.Amount = item.Amount;
                                    }
                                    if (itemInDb.Bank != item.Bank)
                                    {
                                        _auditService.FA_EditRow(formId, existingForm.FormType,
                                            existingForm.FormDate, currentUser, itemInDb.Bank,
                                            item.Bank, "Bank");

                                        itemInDb.Bank = item.Bank;
                                    }
                                    if (itemInDb.FundType != item.FundType)
                                    {
                                        _auditService.FA_EditRow(formId, existingForm.FormType,
                                            existingForm.FormDate, currentUser, itemInDb.FundType,
                                            item.FundType, "Fund Type");

                                        itemInDb.FundType = item.FundType;
                                    }

                                    itemInDb.ModifiedBy = currentUser;
                                    itemInDb.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.AMSD_IF_Item.Add(new AMSD_IF_Item
                                {
                                    FormId = formId,
                                    FundType = item.FundType,
                                    Bank = item.Bank,
                                    Amount = item.Amount,
                                    ModifiedBy = currentUser,
                                    ModifiedDate = DateTime.Now
                                });
                                _auditService.FA_AddRow(formId, existingForm.FormType,
                                            existingForm.FormDate, currentUser,
                                            $"{item.FundType}, {item.Bank}, {item.Amount}");
                            }
                        }

                    }

                    var saveFormItemsChanges = db.SaveChanges();
                    if (saveFormItemsChanges < 1) return 0;

                    if (existingForm.FormStatus == FormStatus.PendingApproval && !string.IsNullOrEmpty(input.Approver) && !input.IsSaveAdminEdit)
                    {
                        Create(existingForm.Id, existingForm.FormType, existingForm.FormDate, existingForm.PreparedBy, existingForm.ApprovedBy, input.ApprovalNotes);
                    }

                    return formId;
                 }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return 0;
            }
        }

        public int DeleteForm(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId && x.FormStatus != Common.FormStatus.PendingApproval);
                    if (form == null) return 0;
                    
                    var formItems = db.AMSD_IF_Item.Where(x => x.FormId == formId);
                    if (formItems.Any())
                    {
                        db.AMSD_IF_Item.RemoveRange(formItems);
                    }
                    db.AMSD_IF.Remove(form);
                    var deletedForm = db.SaveChanges();

                    if (deletedForm < 1) return 0;

                    _auditService.FA_Add(form.Id, form.FormType, form.FormDate, FormActionType.Delete, currentUser, $"Deleted a {form.FormType} form. (Form status at the moment of deletion is {form.FormStatus}).");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return 0;
            }
        }

        public int ApproveForm(IfFormApprovingReq input, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == input.FormId);
                    if (form == null) return 0;
                    
                    form.ApprovedDate = DateTime.Now;
                    form.FormStatus = (input.ApprovalStatus) ? FormStatus.Approved : FormStatus.Rejected;

                    var saveFormApproval = db.SaveChanges();
                    if(saveFormApproval < 1) return 0;

                    ApprovalResponse(form.Id, form.FormType, form.FormDate, form.PreparedBy, form.ApprovedBy, input.ApprovalNote, form.FormStatus);
                    return form.Id;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return 0;
            }
        }

        public bool WithdrawForm(int formId, string performedBy, string formType)
        {
            var withdraFormStatus = this.RetractFormSubmission(formId, performedBy, formType);
            return withdraFormStatus;
        }

        public bool ReassignApproverForm(int formId, string newApprover, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return false;

                    var permittedApprover = db.Config_Approver
                        .FirstOrDefault(x =>
                            x.FormType == form.FormType && x.Username != currentUser && x.Username == newApprover);
                    if (form == null) return false;

                    var currentApprover = form.ApprovedBy;
                    form.ApprovedBy = newApprover;
                    form.ApprovedDate = null;

                    var reassignStatus = db.SaveChanges();
                    if (reassignStatus < 1) return false;

                    ReassignApprover(formId, form.FormType, form.FormDate, currentUser, currentApprover, form.ApprovedBy);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return false;
            }
        }

        public string GenExportFormId(int formId, string currentUser, bool isExportToExcel)
        {
            return _genFile.GenId_IfForm(formId, currentUser, isExportToExcel);
        }

        public ExportedFile GetGeneratedForm(string generatedFileId, bool isGenCnEmailTemplate = false)
        {
            return _genFile.GenFile(generatedFileId, isGenCnEmailTemplate);
        }

        #endregion


    }
}
