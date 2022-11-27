using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.Notification
{
    public interface INotificationService
    {
        void NotifyPreparer(int formId, string formType, string formStatus, string preparer, string approver, string notes);
        void NotifyApprover(int formId, string formType, string preparer, string approver, string notes);

    }
}
