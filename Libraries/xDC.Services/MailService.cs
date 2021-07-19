using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services
{
    public class MailService
    {
        public void TestSendEmailToSmtp(string recipient)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(Config.SmtpServerIp, Convert.ToInt32(Config.SmtpServerPort), false);

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount));
                    message.Subject = "[Kashflow] Test Email";
                    
                    var bodyBuilder = new StringBuilder();
                    bodyBuilder.Append($"<p>Testing, testing....</p>");
                    bodyBuilder.AppendLine($"<p>Plz... just ignore kay...</p>");

                    message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = bodyBuilder.ToString()
                    };

                    message.To.Add(new MailboxAddress(recipient, recipient));

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

        }

        public void SendSubmitForApprovalEmail(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var approverName =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.ApprovedBy);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount));
                        message.Subject = "[Kashflow] Inflow Funds Submission Require Approval";
                        
                        var approvalPageUrl = string.Format("{0}/amsd/InflowFundsFormStatus?id={1}", Config.EmailApplicationUrl, formId);
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", approverName.DisplayName));
                        bodyBuilder.AppendLine(string.Format("<p>An item is pending for your approval. </p> "));
                        bodyBuilder.AppendLine(string.Format("<p>Click <a href='" + approvalPageUrl + "'>here</a> to view.</p>"));

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

                        message.To.Add(new MailboxAddress(approverName.DisplayName, approverName.Email));
                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("SendSubmitForApprovalEmail FAILED, form data error: " + formId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void SendApprovalStatusEmail(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var preparerName =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.PreparedBy);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount));
                        message.Subject = "[Kashflow] Inflow Funds Approval Status";

                        var approvalPageUrl = string.Format("{0}/amsd/InflowFundsFormStatus?id={1}", Config.EmailApplicationUrl, formId);
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", preparerName.DisplayName));
                        bodyBuilder.AppendLine(string.Format("<p>Your form  <a href='" + approvalPageUrl + "'>#" + getForm.Id +"</a> have been " + getForm.FormStatus));

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

                        message.To.Add(new MailboxAddress(preparerName.DisplayName, preparerName.Email));
                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("SendApprovalStatusEmail FAILED, form data error: " + formId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void SendCutOffTimeViolationEmail(int formId, List<AspNetUsers> fidAdmins, TimeSpan cutOffTime)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var cutOffTimeString = new DateTime(cutOffTime.Ticks).ToString("HH:mm");
                    var getForm = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var preparerName =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.PreparedBy);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount));
                        message.Subject = "[Kashflow] Inflow Funds Form Violate Cut Off Time";

                        var approvalPageUrl = string.Format("{0}/amsd/InflowFundsFormStatus?id={1}", Config.EmailApplicationUrl, formId);
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append($"<p>Hello there, </p>");
                        bodyBuilder.AppendLine(
                            $" <p>FIY, {getForm.FormType} form <a href='{approvalPageUrl}'>#{getForm.Id}</a> has been submitted/edited/approved outside agreed Cut Off time ({cutOffTimeString}).");

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

                        foreach (var admin in fidAdmins)
                        {
                            message.To.Add(new MailboxAddress(admin.FullName, admin.Email));
                        }
                        
                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("SendApprovalStatusEmail FAILED, form data error: " + formId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void SendEmailToSmtp(MimeMessage message)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(Config.SmtpServerIp, Convert.ToInt32(Config.SmtpServerPort), false);
                    
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            
        }
    }
}
