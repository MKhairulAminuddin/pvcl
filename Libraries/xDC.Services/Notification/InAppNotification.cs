using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services.Notification
{
    public class InAppNotification: IInAppNotification
    {
        #region Fields


        #endregion

        #region Ctor

        #endregion

        private void InAppNotify(App_Notification notification)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    db.App_Notification.Add(notification);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void ApprovalSubmission(int formId, string formType, string preparer, string approver)
        {
            var notificationObj = new App_Notification()
            {
                Title = "Request for approval",
                ShortMessage = $"<a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>{formType}</a> form pending your approval",
                NotificationUrl = $"{Common.PushNotification_FormUrlMap(formType)}{formId}",
                Message = $"{formType} form was submitted by {preparer} and is now pending your approval. Please approve it <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>here</a>.",
                NotificationIconClass = "fa fa-exclamation",
                NotificationType = "bg-aqua",
                CreatedOn = DateTime.Now,
                UserId = approver
            };

            InAppNotify(notificationObj);
        }

        public void ApprovalResponse(int formId, string formType, string formStatus, string preparer, string approver)
        {
            var approvalResult = (formStatus == Common.FormStatus.Approved) ? "<b>Approved</b>" : "<b>Rejected</b>";

            var notificationObj = new App_Notification()
            {
                Title = "Approval Result",
                NotificationUrl = $"{Common.PushNotification_FormUrlMap(formType)}{formId}",
                ShortMessage = $"Your <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>{formType}</a> form has been {approvalResult}",
                Message = $"Your submitted <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>{formType}</a> form has been approved by {approver}",
                NotificationIconClass = "fa fa-exclamation",
                NotificationType = (formStatus == Common.FormStatus.Approved) ? "bg-green" : "bg-red",
                CreatedOn = DateTime.Now,
                UserId = preparer
            };

            InAppNotify(notificationObj);
        }

    }
}
