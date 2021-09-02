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

                    var message = new MimeMessage()
                    {
                        Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                        Subject = "[Kashflow] Test Email",
                        To =
                        {
                            new MailboxAddress(recipient, recipient)
                        }
                    };
                    
                    var bodyBuilder = new StringBuilder();
                    bodyBuilder.Append($"<p>Testing, testing....</p>");
                    bodyBuilder.AppendLine($"<p>Plz ignore...</p>");

                    message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = bodyBuilder.ToString()
                    };

                    client.Send(message);
                    client.Disconnect(true);
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
                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName,
                                Config.SmtpSenderAccount),
                            Subject = "[Kashflow] Inflow Fund Form Violate Cut Off Time"
                        };
                        
                        var approvalPageUrl = string.Format("{0}amsd/InflowFund/View?id={1}", Config.EmailApplicationUrl, formId);
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

        public void SubmitForApproval(int formId, string formType, string approvedBy, string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var approver =
                        db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == approvedBy);

                    if (approver != null)
                    {
                        var message = ComposeSubmitForApprovalMail(formId, formType, approver.DisplayName, approver.Email, notes);
                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("SubmitForApproval Failed. Approver email not found in AD.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void SendApprovalStatus(int formId, string formType, string formStatus, string formPreparer, string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var preparer =
                        db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == formPreparer);

                    if (preparer != null)
                    {
                        var message = ComposeApprovalFeedbackMail(formId, formType, formStatus, preparer.DisplayName,
                            preparer.Email, notes);
                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("SendApprovalStatus email failed. Preparer info not found in AD.");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void ContributionCreditedTradeSettlement(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var contributionEmailKey = Common.ApplicationConfigKeyMapping(4);
                    var currentCnEmailList = db.Config_Application.FirstOrDefault(x => x.Key == contributionEmailKey);

                    if (currentCnEmailList != null && currentCnEmailList.Value != null)
                    {
                        var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                        if (getForm != null)
                        {
                            var contributionItemKey = Common.TsItemCategory.Cn;
                            var getContributionItems = db.ISSD_TradeSettlement
                                .Where(x => x.InstrumentType == contributionItemKey && x.FormId == formId).ToList();

                            if (getContributionItems.Any())
                            {
                                var preparerName =
                                    db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.PreparedBy);

                                var contributionEmailList = currentCnEmailList.Value.Split(';').ToList();
                                var cnEmailListAddress = new InternetAddressList();
                                foreach (var item in contributionEmailList)
                                {
                                    cnEmailListAddress.Add(new MailboxAddress(item,item));
                                }

                                var message = new MimeMessage()
                                {
                                    Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                                    Subject = "[Kashflow] Contribution Credited submitted in " + getForm.FormType
                                };

                                message.To.AddRange(cnEmailListAddress);

                                var pageUrl = string.Format("{0}" + Common.Email_FormUrlMap(getForm.FormType) + "{1}", Config.EmailApplicationUrl, formId);
                                var bodyBuilder = new StringBuilder();
                                bodyBuilder.Append("<p>Hi All, </p>");
                                bodyBuilder.AppendLine(string.Format("<p>Contribution Credited item in  <a href='" + pageUrl + "'>#" + getForm.Id + "</a> form have been " + getForm.FormStatus));

                                bodyBuilder.Append(
                                    @"<table>
                                      <tr>
                                        <th>Contribution Credited</th>
                                        <th>Amount (+)</th>
                                        <th>Remarks</th>
                                        <th>Modified By</th>
                                        <th>Modified Date</th>
                                      </tr>");
                                var tableRows = string.Empty;
                                foreach (var item in getContributionItems)
                                {
                                    tableRows += string.Format(@"<tr>
                                            <td>{0}</td>
                                            <td>{1}</td>
                                            <td>{3}</td>
                                            <td>{4}</td>
                                          </tr>", item.InstrumentCode, item.AmountPlus, item.Remarks, item.ModifiedBy, item.ModifiedDate.Value.ToString("dd/MM/yyyy"))
                                        ;
                                }
                                bodyBuilder.Append(tableRows);
                                bodyBuilder.Append(@"</table>");



                                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                                {
                                    Text = bodyBuilder.ToString()
                                };

                                SendEmailToSmtp(message);
                            }
                        }
                        else
                        {
                            Logger.LogError("SendApprovalStatusEmail FAILED, form data error: " + formId);
                        }
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

        private MimeMessage ComposeSubmitForApprovalMail(int formId, string formType, string approverName, string approverMail, string notes)
        {
            var message = new MimeMessage()
            {
                Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                Subject = "[Kashflow] Request for Approval " + formType,
                To =
                {
                    new MailboxAddress(approverName, approverMail)
                }
            };

            var approvalPageUrl = string.Format("{0}" + Common.Email_FormUrlMap(formType) + "{1}",
                Config.EmailApplicationUrl, formId);

            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append($"<p>Hi {approverName}, </p>");
            bodyBuilder.AppendLine($"<p>A {formType} form is pending for your approval. </p>");
            bodyBuilder.AppendLine($"<p>Click <a href='{approvalPageUrl}'>here</a> to view.</p>");

            if (!string.IsNullOrEmpty(notes))
            {
                bodyBuilder.AppendLine($"<br/><br/><p style='font-weight:bold'>Notes from preparer: </p>");
                bodyBuilder.AppendLine($"<p>{notes}</p>");
            }

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = bodyBuilder.ToString()
            };

            return message;
        }

        private MimeMessage ComposeApprovalFeedbackMail(int formId, string formType, string formStatus, string preparerName, string preparerMail, string notes)
        {
            var message = new MimeMessage()
            {
                Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                Subject = $"[Kashflow] Approval Status for submitted {formType} form",
                To =
                {
                    new MailboxAddress(preparerName, preparerMail)
                }
            };

            var approvalPageUrl = $"{Config.EmailApplicationUrl}{Common.Email_FormUrlMap(formType)}{formId}";

            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", preparerName));

            if (formStatus == Common.FormStatus.Approved)
            {
                bodyBuilder.AppendLine($"<p>Good News! Your submitted form <a href='{approvalPageUrl}'>#{formId}</a> have been <span style='color:#2ECC71;'>{formStatus}</span>");
            }
            else
            {
                bodyBuilder.AppendLine($"<p>Bad News! Your submitted form <a href='{approvalPageUrl}'>#{formId}</a> have been <span style='color:#E74C3C;'>{formStatus}</span>");
            }

            if (!string.IsNullOrEmpty(notes))
            {
                bodyBuilder.AppendLine($"<br/><br/><p style='font-weight:bold'>Notes from approver: </p>");
                bodyBuilder.AppendLine($"<p>{notes}</p>");
            }

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = bodyBuilder.ToString()
            };

            return message;
        }
    }
}
