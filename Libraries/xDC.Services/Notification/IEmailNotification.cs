using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.Notification
{
    public interface IEmailNotification
    {
        void ApprovalSubmission(int formId, string formType, string approvedBy, string notes);
        void ApprovalResponse(int formId, string formType, string formStatus, string formPreparer, string notes);
    }
}
