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
                    PushInflowFundAfterCutOffSubmissionNotification(formId);
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
                    var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    var notificationObj = new App_Notification()
                    {
                        Title = "Form #" + formId + " " + formHeader.FormStatus ,
                        ShortMessage = string.Format("Submitted form #" + formId +
                                                     "has been "+ formHeader.FormStatus + ". <a href='../amsd/InflowFundsFormStatus?id=" +
                                                     formId + "'>Click here to open the form</a>"),
                        Message = string.Format("Submitted form #" + formId +
                                                "has been " + formHeader.FormStatus + ". <a href='../amsd/InflowFundsFormStatus?id=" +
                                                formId + "'>Click here to open the form</a>"),
                        NotificationIconClass = (formHeader.FormStatus == "Approved")? "fa fa-check-circle" : "fa fa-times-circle",
                        NotificationType = (formHeader.FormStatus == "Approved") ? "bg-green" : "bg-red",
                        CreatedOn = DateTime.Now,
                        UserId = formHeader.PreparedBy
                    };

                    PushNotification(notificationObj);
                    new MailService().SendApprovalStatusEmail(formId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void PushInflowFundAfterCutOffSubmissionNotification(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
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
                            
                            var fidAdminRole = db.AspNetRoles.FirstOrDefault(x => x.Name == "Power User");
                            var adminList = db.AspNetUsers.Where(x => x.AspNetRoles.Any(y => y.Name == fidAdminRole.Name)).ToList();

                            if (adminList.Any())
                            {
                                var adminListEmail = new List<string>();

                                foreach (var admin in adminList)
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
                                    adminListEmail.Add(admin.Email);
                                }

                                new MailService().SendCutOffTimeViolationEmail(formId, adminListEmail);
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
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}
