using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static xDC.Utils.Common;
using System.Web.Security;

namespace xDC.Services.Notification
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly IEmailNotification _emailNotify;
        private readonly IInAppNotification _inAppNotify;

        #endregion

        #region Ctor

        public NotificationService(IEmailNotification emailNotify)
        {
            _emailNotify = emailNotify;
        }

        #endregion

        #region Methods

        public void NotifyPreparer(int formId, string formType, string formStatus, string preparer, string approver, string notes)
        {
            _emailNotify.ApprovalResponse(formId, formType, formStatus, preparer, notes);
            _inAppNotify.ApprovalResponse(formId, formType, formStatus, approver, notes);
        }
        public void NotifyApprover(int formId, string formType, string preparer, string approver, string notes)
        {
            _emailNotify.ApprovalSubmission(formId, formType, approver, notes);
            _inAppNotify.ApprovalSubmission(formId, formType, preparer, approver);
        }

        public void NotifyReassignedApprover()
        {

        }

        #endregion

    }
}
