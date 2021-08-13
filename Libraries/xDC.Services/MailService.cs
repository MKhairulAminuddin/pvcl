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

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = "[Kashflow] Inflow Funds Submission Require Approval",
                            To =
                            {
                                new MailboxAddress(approverName.DisplayName, approverName.Email)
                            }
                        };
                        
                        var approvalPageUrl = string.Format("{0}amsd/InflowFund/View?id={1}", Config.EmailApplicationUrl, formId);
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", approverName.DisplayName));
                        bodyBuilder.AppendLine(string.Format("<p>An item is pending for your approval. </p> "));
                        bodyBuilder.AppendLine(string.Format("<p>Click <a href='" + approvalPageUrl + "'>here</a> to view.</p>"));

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };
                        
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

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = "[Kashflow] Inflow Funds Approval Status",
                            To =
                            {
                                new MailboxAddress(preparerName.DisplayName, preparerName.Email)
                            }
                        };

                        var approvalPageUrl = string.Format("{0}amsd/InflowFund/View?id={1}", Config.EmailApplicationUrl, formId);
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", preparerName.DisplayName));
                        bodyBuilder.AppendLine(string.Format("<p>Your form  <a href='" + approvalPageUrl + "'>#" + getForm.Id +"</a> have been " + getForm.FormStatus));

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };
                        
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
                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName,
                                Config.SmtpSenderAccount),
                            Subject = "[Kashflow] Inflow Funds Form Violate Cut Off Time"
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

        public void SubmitForApprovalTradeSettlement(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var approverName =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.ApprovedBy);

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = "[Kashflow] Request for Approval " + getForm.FormType,
                            To =
                            {
                                new MailboxAddress(approverName.DisplayName, approverName.Email)
                            }
                        };

                        var approvalPageUrl = string.Format("{0}" + Common.FormUrlViewMappingForEmailNotification(getForm.FormType) + "{1}", Config.EmailApplicationUrl, formId);

                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", approverName.DisplayName));
                        bodyBuilder.AppendLine(string.Format("<p>An item is pending for your approval. </p> "));
                        bodyBuilder.AppendLine(string.Format("<p>Click <a href='" + approvalPageUrl + "'>here</a> to view.</p>"));

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

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

        public void ApprovalStatusTradeSettlement(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (getForm != null)
                    {
                        var preparerName =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == getForm.PreparedBy);

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = "[Kashflow] Approval Status " + getForm.FormType,
                            To =
                            {
                                new MailboxAddress(preparerName.DisplayName, preparerName.Email)
                            }
                        };

                        var approvalPageUrl = string.Format("{0}" + Common.FormUrlViewMappingForEmailNotification(getForm.FormType) + "{1}", Config.EmailApplicationUrl, formId);

                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append(string.Format("<p>Hi {0}, </p>", preparerName.DisplayName));
                        bodyBuilder.AppendLine(string.Format("<p>Your form  <a href='" + approvalPageUrl + "'>#" + getForm.Id + "</a> have been " + getForm.FormStatus));

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

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
                            var contributionItemKey = Common.TradeSettlementMapping(10);
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

                                var pageUrl = string.Format("{0}" + Common.FormUrlViewMappingForEmailNotification(getForm.FormType) + "{1}", Config.EmailApplicationUrl, formId);
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
    }
}
