using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.Notification
{
    public interface IInAppNotification
    {
        void ApprovalSubmission(int formId, string formType, string preparer, string approver);
        void ApprovalResponse(int formId, string formType, string formStatus, string preparer, string approver);


    }
}
