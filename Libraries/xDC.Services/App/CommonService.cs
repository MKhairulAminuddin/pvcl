using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.App
{
    public static class CommonService
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
    }
}
