using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

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

        public void NotifyApprovalRequest(string formApprover, int formId, string formSubmittedBy, string formType)
        {
            var notificationObj = new App_Notification()
            {
                Title = "Request for approval",
                ShortMessage = $"<a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>{formType}</a> form pending your approval",
                Message = $"{formType} form was submitted by {formSubmittedBy} and is now pending your approval. Please approve it <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>here</a>.",
                NotificationIconClass = "fa fa-exclamation",
                NotificationType = "bg-aqua",
                CreatedOn = DateTime.Now,
                UserId = formApprover
            };

            PushNotification(notificationObj);
        }

        public void NotifyApprovalResult(string formPreparer, int formId, string formApprovalBy, string formType, string formStatus)
        {
            var approvalResult = (formStatus == Common.FormStatus.Approved) ? "<b>Approved</b>" : "<b>Rejected</b>";

            var notificationObj = new App_Notification()
            {
                Title = "Approval Result",
                ShortMessage = $"Your <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>{formType}</a> form has been {approvalResult}",
                Message = $"Your submitted <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>{formType}</a> form has been approved by {formApprovalBy}",
                NotificationIconClass = "fa fa-exclamation",
                NotificationType = (formStatus == Common.FormStatus.Approved) ? "bg-green" : "bg-red",
                CreatedOn = DateTime.Now,
                UserId = formPreparer
            };

            PushNotification(notificationObj);
        }
        
        public void NotifyNewApproveInflowFund(int formId, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var message =
                        $"New AMSD Inflow Fund approved. <a href='{Common.PushNotification_FormUrlMap(formType)}{formId}'>Form #{formId}</a>";
                    
                    var adminList = db.AspNetRoles.FirstOrDefault(x => x.Name == Config.Acl.PowerUser);

                    if (adminList != null)
                    {
                        foreach (var admin in adminList.AspNetUsers)
                        {
                            var notificationObj = new App_Notification()
                            {
                                Title = message,
                                ShortMessage = message,
                                Message = message,
                                NotificationIconClass = "fa fa-usd",
                                NotificationType = "bg-green",
                                CreatedOn = DateTime.Now,
                                UserId = admin.UserName
                            };

                            PushNotification(notificationObj);
                        }
                        new MailService().NewApprovedInflowFund(formId, adminList.AspNetUsers.ToList());
                    }
                    else
                    {
                        Logger.LogError("PushInflowFundAfterCutOffSubmissionNotification no admin email");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}
