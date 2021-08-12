using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;

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

        public static void NotifyApprover(string approverUsername, int formId, string submittedBy, int formType)
        {
            new NotificationService().PushNotificationForApproval(approverUsername, formId, submittedBy, formType);
        }

        public static void NotifyPreparer(string preparer, int formId, string approvedBy, int formType, bool isApproved)
        {
            new NotificationService().PushNotificationForApprovalResult(preparer, formId, approvedBy, formType, isApproved);
        }
    }
}
