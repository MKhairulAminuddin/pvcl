using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        #endregion


    }
}
