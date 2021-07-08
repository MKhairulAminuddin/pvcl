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
        public void SendSubmitForApprovalEmail(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var approverName =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.ApprovedBy);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount));
                        message.Subject = "[Kashflow] Inflow Funds Submission Require Approval";
                        
                        var approvalPageUrl = string.Format("http://localhost/amsd/InflowFundsFormStatus?id={0}", formId);
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

        private void SendEmailToSmtp(MimeMessage message)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(Config.SmtpServerIp, Convert.ToInt32(Config.SmtpServerPort), false);

                    // Note: only needed if the SMTP server requires authentication
                    //client.Authenticate("joey", "password");

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
