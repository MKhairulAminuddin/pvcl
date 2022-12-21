using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Form;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Audit;
using xDC.Services.Membership;
using xDC.Services.Notification;
using xDC.Services.Workflow;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.Form
{
    public class FormService
    {
        #region Fields

        private readonly IWorkflowService _wfService;
        private readonly INotificationService _notifyService;
        private readonly IAuditService _auditService;
        private readonly IRoleManagementService _roleService;

        #endregion

        #region Ctor

        public FormService(IWorkflowService wfService, INotificationService notifyService, IAuditService auditService, IRoleManagementService roleService)
        {
            _wfService = wfService;
            _notifyService = notifyService;
            _auditService = auditService;
            _roleService = roleService;
        }

        #endregion

        #region Enable Form Features

        public bool EnableSaveAsDraft(string currentUser, string formStatus, string preparedBy, string approvedBy)
        {
            var isDraft = formStatus == FormStatus.Draft;
            var isMyForm = preparedBy == currentUser;
            var isApproverAssigned = !string.IsNullOrEmpty(approvedBy);

            return isDraft && isMyForm & !isApproverAssigned;
        }

        public bool EnableEdit(string formStatus, string currentUser, string permissionKey)
        {
            var isPendingApproval = formStatus == FormStatus.PendingApproval;
            var isAllowedToEdit = _roleService.IsUserHaveAccess(currentUser, permissionKey);

            return !isPendingApproval && isAllowedToEdit;
        }

        public bool EnableDelete(string formStatus, string formApprover, string currentUser, string permissionKey)
        {
            var isPendingApproval = formStatus == FormStatus.PendingApproval;
            var isFormApprover = formApprover == currentUser;
            var isUserHavePermission = _roleService.IsUserHaveAccess(currentUser, permissionKey);

            return !isPendingApproval && !isFormApprover && isUserHavePermission;
        }

        public bool EnablePrint(string currentUser, string formStatus, string permissionKey)
        {
            var isDownloadAllowed = _roleService.IsUserHaveAccess(currentUser, permissionKey);
            var isDraft = formStatus == FormStatus.Draft;

            return isDownloadAllowed && !isDraft;
        }

        public bool EnableReassignApprover(string formStatus, string formApprover, string currentUser, string permissionKey)
        {
            var isFormPendingApproval = formStatus == FormStatus.PendingApproval;
            var isNotCurrentFormApprover = formApprover != currentUser;
            var isAllowedToEdit = _roleService.IsUserHaveAccess(currentUser, permissionKey);

            return isFormPendingApproval && isNotCurrentFormApprover && isAllowedToEdit;
        }

        public bool EnableResubmission(string formStatus, string formApprover, string currentUser, string permissionKey)
        {
            var isFormAllowedForResubmission = (formStatus == FormStatus.Approved || formStatus == FormStatus.Rejected);
            var isAllowedToEdit = _roleService.IsUserHaveAccess(currentUser, permissionKey);

            return isFormAllowedForResubmission && isAllowedToEdit;
        }

        public bool EnableApprovalSubmission(string formStatus, string approver, string currentUser, string permissionKey)
        {
            var formIsInDraftAndAllowedForSubmission = (formStatus == FormStatus.Draft || formStatus == null) && string.IsNullOrEmpty(approver);
            var isAllowedToEdit = _roleService.IsUserHaveAccess(currentUser, permissionKey);

            return formIsInDraftAndAllowedForSubmission && isAllowedToEdit;
        }

        public bool EnableFormWithdrawal(string currentUser, string formPreparedBy, string formStatus, string permissionKey)
        {
            var haveEditPermission = _roleService.IsUserHaveAccess(currentUser, permissionKey);
            var isPendingApproval = formStatus == FormStatus.PendingApproval || formStatus == FormStatus.Approved || formStatus == FormStatus.Rejected;
            //var isPreparedByMe = currentUser == formPreparedBy;

            return haveEditPermission && isPendingApproval;
        }

        public bool EnableFormApproval(string currentUser, string assignedApprover, string formStatus, kashflowDBEntities db)
        {
            var isApprover = assignedApprover == currentUser;
            var isPendingApproval = formStatus == FormStatus.PendingApproval;
            var isUserAllowedForApproval = db.Config_Approver.Any(x => x.Username == currentUser);

            return isApprover && isPendingApproval && isUserAllowedForApproval;
        }

        #endregion



        public void Create(int formId, string formType, DateTime? formDate, string preparer, string approver, string notes)
        {
            _wfService.Initiate(formId, formType, preparer, approver, notes);
            _notifyService.NotifyApprover(formId, formType, preparer, approver, notes);

            _auditService.FA_Add(formId, formType, formDate, FormActionType.Create, preparer, $"Created an {formType} form");
            _auditService.FA_Add(formId, formType, formDate, FormActionType.RequestApproval, preparer, $"Request Approval for {formType} form");
        }

        public void Delete(int formId, string formType, DateTime? formDate, string currentUser)
        {
            _auditService.FA_Add(formId, formType, formDate, FormActionType.Delete, currentUser, $"Deleted {formType} form");
        }

        public void ApprovalResponse(int formId, string formType, DateTime? formDate, string preparer, string approver, string notes, string formStatus)
        {
            
            _wfService.Approval(formId, formType, preparer, approver, notes, formStatus);
            _notifyService.NotifyPreparer(formId, formType, formStatus, preparer, approver, notes);

            _auditService.FA_Approval(formId, formType, formStatus, formDate, approver);
        }

        public bool RetractFormSubmission(int formId, string performedBy, string formType)
        {
            // TODO: Add notification on form withdrawal

            try
            {
                using (var db = new kashflowDBEntities())
                {
                    switch (formType)
                    {
                        case FormType.AMSD_IF:
                            var ifForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                            if (ifForm != null)
                            {
                                ifForm.FormStatus = FormStatus.Draft;
                                ifForm.ApprovedBy = null;
                                ifForm.ApprovedDate = null;
                                var updateStatus = db.SaveChanges();

                                _auditService.FA_Add(ifForm.Id, ifForm.FormType, ifForm.FormDate, FormActionType.RetractSubmission,
                                    performedBy, $"Retract form submission for {ifForm.FormType} form.");

                                return updateStatus > 0;
                            }
                            else
                            {
                                return false;
                            }

                        case FormType.FID_TREASURY:
                            var treasuryForm = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                            if (treasuryForm != null)
                            {
                                treasuryForm.FormStatus = FormStatus.Draft;
                                treasuryForm.ApprovedBy = null;
                                treasuryForm.ApprovedDate = null;
                                var updateStatus = db.SaveChanges();

                                _auditService.FA_Add(treasuryForm.Id, treasuryForm.FormType, treasuryForm.ValueDate, FormActionType.RetractSubmission,
                                    performedBy, $"Retract form submission for {treasuryForm.FormType} form.");

                                return updateStatus > 0;
                            }
                            else
                            {
                                return false;
                            }

                        case FormType.ISSD_TS:
                        case FormType.ISSD_TS_A:
                        case FormType.ISSD_TS_B:
                        case FormType.ISSD_TS_C:
                        case FormType.ISSD_TS_D:
                        case FormType.ISSD_TS_E:
                        case FormType.ISSD_TS_F:
                        case FormType.ISSD_TS_G:
                        case FormType.ISSD_TS_H:
                            var tsForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                            if (tsForm != null)
                            {
                                tsForm.FormStatus = FormStatus.Draft;
                                tsForm.ApprovedBy = null;
                                tsForm.ApprovedDate = null;
                                var updateStatus = db.SaveChanges();

                                _auditService.FA_Add(tsForm.Id, tsForm.FormType, tsForm.SettlementDate, FormActionType.RetractSubmission,
                                    performedBy, $"Retract form submission for {tsForm.FormType} form.");

                                return updateStatus > 0; ;
                            }
                            else
                            {
                                return false;
                            }

                        default:
                            return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return false;
            }
        }

        public void ReassignApprover(int formId, string formType, DateTime? formDate, string currentUser, string currentApprover, string newApprover)
        {
            _auditService.FA_ReassignApprover(formId, formType, formDate, currentUser, currentApprover, newApprover);
            _notifyService.NotifyApprover(formId, formType, currentUser, newApprover, "Reassign approver");
            _wfService.Reassign(formId, formType, currentUser, newApprover);
        }

        public void FormResubmission(int formId, string formType, DateTime? formDate, string preparer, string approver, string currentUser)
        {
            _wfService.Resubmit(formId, formType, preparer, approver);
            _auditService.FA_Add(formId, formType, formDate, FormActionType.Resubmit, currentUser, $"Resubmit {formType} form which has been approved before.");
        }
    }
}
