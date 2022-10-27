using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class FormService
    {
        public static void NotifyApprover(string approverUsername, int formId, string submittedBy, string formType, string notes)
        {
            new NotificationService().NotifyApprovalRequest(approverUsername, formId, submittedBy, formType);
            new MailService().SubmitForApproval(formId, formType, approverUsername, notes);
            WorkflowService.SubmitForApprovalWorkflow(formId, formType, notes);
        }

        public static void NotifyPreparer(int formId, string formType, string formStatus, string preparedBy, string approvedBy, string approvalNotes)
        {
            new NotificationService().NotifyApprovalResult(preparedBy, formId, approvedBy, formType, formStatus);
            new MailService().SendApprovalStatus(formId, formType, formStatus, preparedBy, approvalNotes);
            WorkflowService.ApprovalResponse(formId, formStatus, approvalNotes, formType, preparedBy, approvedBy);
        }

        #region Forms Landing Page

        public static bool EnableEdit(string formStatus, string formPreparer, string formApprover, string currentUser)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isFormApprover = formApprover == currentUser;
            var isFormPreparer = formPreparer == currentUser;

            return !isPendingApproval && !isFormApprover && isFormPreparer;
        }

        public static bool EnableDelete(string formStatus, string formPreparer, string formApprover, string currentUser)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isFormApprover = formApprover == currentUser;
            var isFormPreparer = formPreparer == currentUser;

            return !isPendingApproval && !isFormApprover && isFormPreparer;
        }

        public static bool EnablePrint(string currentUser, string formStatus, string permissionKey)
        {
            var isDownloadAllowed = new AuthService().IsUserHaveAccess(currentUser, permissionKey);
            var isDraft = formStatus == Common.FormStatus.Draft;

            return (isDownloadAllowed && !isDraft);
        }

        public static bool EnableRetractSubmission(string currentUser, string formPreparedBy, string formStatus, string permissionKey)
        {
            var haveEditPermission = new AuthService().IsUserHaveAccess(currentUser, permissionKey);
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isPreparedByMe = currentUser == formPreparedBy;

            return (haveEditPermission && isPendingApproval && isPreparedByMe);
        }

        public static bool RetractFormSubmission(int formId, string performedBy, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    switch (formType)
                    {
                        case Common.FormType.AMSD_IF:
                            var ifForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                            if (ifForm != null)
                            {
                                ifForm.FormStatus = Common.FormStatus.Draft;
                                ifForm.ApprovedBy = null;
                                db.SaveChanges();

                                AuditService.Capture_FA(ifForm.Id, ifForm.FormType, Common.FormActionType.RetractSubmission, 
                                    performedBy, $"Retract form submission for {ifForm.FormType} form.");

                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        case Common.FormType.FID_TREASURY:
                            var treasuryForm = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                            if (treasuryForm != null)
                            {
                                treasuryForm.FormStatus = Common.FormStatus.Draft;
                                treasuryForm.ApprovedBy = null;
                                db.SaveChanges();

                                AuditService.Capture_FA(treasuryForm.Id, treasuryForm.FormType, Common.FormActionType.RetractSubmission, 
                                    performedBy, $"Retract form submission for {treasuryForm.FormType} form.");

                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        case Common.FormType.ISSD_TS:
                        case Common.FormType.ISSD_TS_A:
                        case Common.FormType.ISSD_TS_B:
                        case Common.FormType.ISSD_TS_C:
                        case Common.FormType.ISSD_TS_D:
                        case Common.FormType.ISSD_TS_E:
                        case Common.FormType.ISSD_TS_F:
                        case Common.FormType.ISSD_TS_G:
                        case Common.FormType.ISSD_TS_H:
                            var tsForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                            if (tsForm != null)
                            {
                                tsForm.FormStatus = Common.FormStatus.Draft;
                                tsForm.ApprovedBy = null;
                                db.SaveChanges();

                                AuditService.Capture_FA(tsForm.Id, tsForm.FormType, Common.FormActionType.RetractSubmission,
                                    performedBy, $"Retract form submission for {tsForm.FormType} form.");

                                return true;
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

        #endregion


    }
}
