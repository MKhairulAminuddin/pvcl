using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;

namespace xDC.Services.Notification
{
    public interface IEmailNotification
    {
        void FormSubmission(int formId, string formType, string approvedBy, string notes);
        void ApprovalResponse(int formId, string formType, string formStatus, string formPreparer, string notes);
        void TestSendEmail(string recipientEmail);
        
        void OnFcaTagging();
        void FcaTaggingQueue(ISSD_TradeSettlement issdTaggedItem);
    }
}
