using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class TradeSettlementService
    {
        public static bool IsSameDateAndCurrencyExist(kashflowDBEntities db, DateTime settlementDate, string currency, string formType)
        {
            // check for same date and same currency exist
            var isExist = db.ISSD_FormHeader.Any(x =>
                DbFunctions.TruncateTime(x.SettlementDate) == settlementDate.Date && x.Currency == currency && x.FormType == formType);
            return isExist;
        }

        public static bool IsTMinus(DateTime settlementDate)
        {
            var isTMinus = settlementDate.Date < DateTime.Now.Date;
            return isTMinus;
        }

        public static void NotifyApprover(string approverUsername, int formId, string submittedBy, int formType, string notes)
        {
            new NotificationService().PushNotificationForApproval(approverUsername, formId, submittedBy, formType);
            new MailService().SubmitForApprovalTradeSettlement(formId);
            new WorkflowService().SubmitForApprovalWorkflow(formId, formType, notes);
        }

        public static void NotifyPreparer(string preparer, int formId, string approvedBy, int formType, bool isApproved, string approvalNotes)
        {
            new NotificationService().PushNotificationForApprovalResult(preparer, formId, approvedBy, formType, isApproved);
            new MailService().ApprovalStatusTradeSettlement(formId);
            new MailService().ContributionCreditedTradeSettlement(formId);
            new WorkflowService().ApprovalFeedbackWorkflow(formId, isApproved, approvalNotes, formType);
        }

        public static List<Form_Workflow> GetWorkflow(kashflowDBEntities db, int formId)
        {
            var result = db.Form_Workflow
                            .Where(x => x.FormId == formId)
                            .OrderByDescending(x => x.EndDate)
                            .ToList();

            return result;
        }

        public static List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, int formId)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => x.FormId == formId)
                .ToList();

            return result;
        }

        public static bool IsInPendingStatus(string formStatus)
        {
            return (formStatus.ToUpper() == Common.FormStatusMapping(2).ToUpper());
        }

        public static bool IsIamThisFormApprover(kashflowDBEntities db, int formId, string currentUsername)
        {
            var result = db.ISSD_FormHeader
                .FirstOrDefault(x => x.Id == formId);

            if (result != null)
            {
                return result.ApprovedBy == currentUsername;
            }
            else
            {
                return false;
            }
        }
    }
}
