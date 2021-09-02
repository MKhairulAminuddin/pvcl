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



        /// <summary>
        /// Send notification to a Approver for Submitted Form Approval
        /// </summary>
        /// <param name="formId">Id of the related form</param>
        public void PushSubmitForApprovalNotification(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotificationEnabledKey = Common.ApplicationConfigKeyMapping(2);
                    var isNotificationEnabled = db.Config_Application.FirstOrDefault(x => x.Key == isNotificationEnabledKey);
                    var isNotificationEnabledParsed = isNotificationEnabled != null && bool.Parse(isNotificationEnabled.Value);

                    if (isNotificationEnabledParsed)
                    {
                        var form = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                        var notificationObj = new App_Notification()
                        {
                            Title = "Pending your approval",
                            ShortMessage = string.Format("Submitted form " + form.FormType + " by " + form.PreparedBy +
                                                         "form is pending your approval. <a href='../amsd/InflowFundsFormStatus?id=" +
                                                         formId + "'>Click here to open it</a>"),
                            Message = string.Format("Submitted form " + form.FormType + " by " + form.PreparedBy +
                                                    "form is pending your approval. <a href='../amsd/InflowFundsFormStatus?id=" +
                                                    formId + "'>Click here to open it</a>"),
                            NotificationIconClass = "fa fa-exclamation",
                            NotificationType = "bg-aqua",
                            CreatedOn = DateTime.Now,
                            UserId = form.ApprovedBy
                        };

                        PushNotification(notificationObj);
                        new MailService().SubmitForApproval(form.Id, form.FormType, form.ApprovedBy, "some note here..");
                    }
                    else
                    {
                        // notification disabled
                        Logger.LogInfo("PushSubmitForApprovalNotification is Disabled!");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Send notification to a Preparer for his/her approval status
        /// </summary>
        /// <param name="formId">Id of the related form</param>
        public void PushApprovalStatusNotification(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotificationEnabledKey = Common.ApplicationConfigKeyMapping(2);
                    var isNotificationEnabled = db.Config_Application.FirstOrDefault(x => x.Key == isNotificationEnabledKey);
                    var isNotificationEnabledParsed = isNotificationEnabled != null && bool.Parse(isNotificationEnabled.Value);

                    if (isNotificationEnabledParsed)
                    {
                        var form = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                        var notificationObj = new App_Notification()
                        {
                            Title = "Form #" + formId + " " + form.FormStatus,
                            ShortMessage = string.Format("Submitted form #" + formId +
                                                         "has been " + form.FormStatus + ". <a href='../amsd/InflowFundsFormStatus?id=" +
                                                         formId + "'>Click here to open the form</a>"),
                            Message = string.Format("Submitted form #" + formId +
                                                    "has been " + form.FormStatus + ". <a href='../amsd/InflowFundsFormStatus?id=" +
                                                    formId + "'>Click here to open the form</a>"),
                            NotificationIconClass = (form.FormStatus == "Approved") ? "fa fa-check-circle" : "fa fa-times-circle",
                            NotificationType = (form.FormStatus == "Approved") ? "bg-green" : "bg-red",
                            CreatedOn = DateTime.Now,
                            UserId = form.PreparedBy
                        };

                        PushNotification(notificationObj);
                        new MailService().SendApprovalStatus(form.Id, form.FormType, form.FormStatus, form.PreparedBy, "some note here...");
                    }
                    else
                    {
                        // notification disabled
                        Logger.LogInfo("PushApprovalStatusNotification is Disabled!");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Send notification to a Power User if any form is submitted or approved beyond the cut off time.
        /// </summary>
        /// <param name="formId">Id of the related form</param>
        public void PushInflowFundAfterCutOffSubmissionNotification(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotificationEnabledKey = Common.ApplicationConfigKeyMapping(2);
                    var isNotificationEnabled = db.Config_Application.FirstOrDefault(x => x.Key == isNotificationEnabledKey);
                    var isNotificationEnabledParsed = isNotificationEnabled != null && bool.Parse(isNotificationEnabled.Value);

                    if (isNotificationEnabledParsed)
                    {
                        var cutOffTimeConfigKey = Common.ApplicationConfigKeyMapping(1);
                        var cutOffTime = db.Config_Application.FirstOrDefault(x => x.Key == cutOffTimeConfigKey);

                        if (cutOffTime != null)
                        {
                            var cutOffTimeParsed = TimeSpan.Parse(cutOffTime.Value);
                            var isViolateCutOffTime = DateTime.Now.TimeOfDay > cutOffTimeParsed;

                            if (isViolateCutOffTime)
                            {
                                var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                                var shortMessage =
                                    string.Format(
                                        "{0} Form submitted/approved after Cut-off time <a href='{1}{2}'>Click here to open it</a>",
                                        formHeader.FormType, Config.UrlAmsdInflowFundsStatus, formId);

                                var longMessage =
                                    string.Format(
                                        "{0} Form submitted/approved after Cut-off time <a href='{1}{2}'>Click here to open it</a>",
                                        formHeader.FormType, Config.UrlAmsdInflowFundsStatus, formId);

                                var adminList = db.AspNetRoles.FirstOrDefault(x => x.Name == "Power User");

                                if (adminList != null)
                                {

                                    foreach (var admin in adminList.AspNetUsers)
                                    {
                                        var notificationObj = new App_Notification()
                                        {
                                            Title = "Inflow Funds Submitted after cut off Time",
                                            ShortMessage = shortMessage,
                                            Message = longMessage,
                                            NotificationIconClass = "fa fa-exclamation",
                                            NotificationType = "bg-aqua",
                                            CreatedOn = DateTime.Now,
                                            UserId = admin.UserName
                                        };

                                        PushNotification(notificationObj);
                                    }

                                    new MailService().SendCutOffTimeViolationEmail(formId, adminList.AspNetUsers.ToList(), cutOffTimeParsed);
                                }
                                else
                                {
                                    Logger.LogError("PushInflowFundAfterCutOffSubmissionNotification no admin email");
                                }

                            }
                        }
                        else
                        {
                            Logger.LogError("PushInflowFundAfterCutOffSubmissionNotification no cut off time setup");
                        }
                    }
                    else
                    {
                        Logger.LogInfo("PushInflowFundAfterCutOffSubmissionNotification is disabled");
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
