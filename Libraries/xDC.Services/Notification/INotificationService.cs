using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.Notification
{
    public interface INotificationService
    {
        void NotifyPreparer(int formId, string formType, string formStatus, string formPreparer, string notes);
        void NotifyApprover(int formId, string formType, string approver, string notes);

    }
}
