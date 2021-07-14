using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;

namespace xDC.Services
{
    public class NotificationService
    {
        private void PushNotification(App_Notification notification)
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

        /// <summary>
        /// Send notification to a Approver for Submitted Form Approval
        /// </summary>
        /// <param name="initiator">Username who initiate the notification</param>
        /// <param name="formId">Id of the related form</param>
        public void PushSubmitForApprovalNotification(string initiator, int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formHeader = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    var notificationObj = new App_Notification()
                    {
                        Title = "Pending your approval",
                        ShortMessage = string.Format("Submitted by " + initiator +
                                                     "form is pending your approval. <a href='../Amsd/InflowFundStatus?id=" +
                                                     formId + "'"),
                        Message = string.Format("Submitted by " + initiator +
                                                "form is pending your approval. <a href='../Amsd/InflowFundStatus?id=" +
                                                formId + "'"),
                        NotificationIconClass = "fa fa-exclamation",
                        NotificationType = "Approval",
                        CreatedOn = DateTime.Now,
                        UserId = formHeader.ApprovedBy
                    };

                    PushNotification(notificationObj);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}
