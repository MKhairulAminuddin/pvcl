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

        public void PushNotificationForApproval(string formApprover, int formId, string formSubmittedBy, int formType)
        {
            var notificationObj = new App_Notification()
            {
                Title = "Request for approval",
                ShortMessage = string.Format("Please approve " + " <a href='" + Common.FormUrlViewMapping(Common.FormTypeMapping(formType)) + formId + "'>Click here to open it</a>"),
                Message = string.Format(
                    Common.FormTypeMapping(formType) + 
                    " form was submitted by " + formSubmittedBy +
                    " and is now pending your approval. You can approve it from here " +
                    " <a href='" + Common.FormUrlViewMapping(Common.FormTypeMapping(formType)) + formId + "'>Click here to open it</a>"),
                NotificationIconClass = "fa fa-exclamation",
                NotificationType = "bg-aqua",
                CreatedOn = DateTime.Now,
                UserId = formApprover
            };

            PushNotification(notificationObj);
        }

        public void PushNotificationForApprovalResult(string formPreparer, int formId, string formApprovalBy, int formType, bool isApprove)
        {
            var approvalResult = (isApprove) ? "<b>Approved</b>" : "<b>Rejected</b>";

            var notificationObj = new App_Notification()
            {
                Title = "Approval Result",
                ShortMessage = string.Format("Your submission have been " + approvalResult + " <a href='" +
                                             Common.FormUrlViewMapping(Common.FormTypeMapping(formType)) + formId +
                                             "'>Click here to open it</a>"),
                Message = string.Format(
                    Common.FormTypeMapping(formType) +
                    " form was " + approvalResult + " by " + formApprovalBy + " " +
                    " <a href='" + Common.FormUrlViewMapping(Common.FormTypeMapping(formType)) + formId +
                    "'>Click here to open it</a>"),
                NotificationIconClass = "fa fa-exclamation",
                NotificationType = (isApprove) ? "bg-green" : "bg-red",
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
                        var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                        var notificationObj = new App_Notification()
                        {
                            Title = "Pending your approval",
                            ShortMessage = string.Format("Submitted form " + formHeader.FormType + " by " + formHeader.PreparedBy +
                                                         "form is pending your approval. <a href='../amsd/InflowFundsFormStatus?id=" +
                                                         formId + "'>Click here to open it</a>"),
                            Message = string.Format("Submitted form " + formHeader.FormType + " by " + formHeader.PreparedBy +
                                                    "form is pending your approval. <a href='../amsd/InflowFundsFormStatus?id=" +
                                                    formId + "'>Click here to open it</a>"),
                            NotificationIconClass = "fa fa-exclamation",
                            NotificationType = "bg-aqua",
                            CreatedOn = DateTime.Now,
                            UserId = formHeader.ApprovedBy
                        };

                        PushNotification(notificationObj);
                        new MailService().SendSubmitForApprovalEmail(formId);
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
                        var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                        var notificationObj = new App_Notification()
                        {
                            Title = "Form #" + formId + " " + formHeader.FormStatus,
                            ShortMessage = string.Format("Submitted form #" + formId +
                                                         "has been " + formHeader.FormStatus + ". <a href='../amsd/InflowFundsFormStatus?id=" +
                                                         formId + "'>Click here to open the form</a>"),
                            Message = string.Format("Submitted form #" + formId +
                                                    "has been " + formHeader.FormStatus + ". <a href='../amsd/InflowFundsFormStatus?id=" +
                                                    formId + "'>Click here to open the form</a>"),
                            NotificationIconClass = (formHeader.FormStatus == "Approved") ? "fa fa-check-circle" : "fa fa-times-circle",
                            NotificationType = (formHeader.FormStatus == "Approved") ? "bg-green" : "bg-red",
                            CreatedOn = DateTime.Now,
                            UserId = formHeader.PreparedBy
                        };

                        PushNotification(notificationObj);
                        new MailService().SendApprovalStatusEmail(formId);
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
