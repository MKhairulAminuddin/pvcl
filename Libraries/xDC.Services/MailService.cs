﻿using System;
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
                    var getForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

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

        public void NewApprovedInflowFund(int formId, List<AspNetUsers> fidAdmins)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var inflowFunds = db.AMSD_IF_Item.Where(x => x.FormId == form.Id).ToList();

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName,
                                Config.SmtpSenderAccount),
                            Subject = "[Kashflow] New AMSD Inflow Funds Approved"
                        };
                        
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append($"<p>Hello there, </p>");
                        bodyBuilder.AppendLine(
                            $"There's new AMSD Inflow Funds approved. The details are as follows:");

                        bodyBuilder.Append(
                                    @"<table style='border-collapse: collapse;'>
                                      <tr>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Bank</th>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount</th>
                                      </tr>");
                        var tableRows = string.Empty;
                        foreach (var item in inflowFunds)
                        {
                            tableRows += string.Format(@"<tr>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1:C}</td>
                                          </tr>", item.Bank, item.Amount);
                        }
                        bodyBuilder.Append(tableRows);
                        bodyBuilder.Append(@"</table>");


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
                    var currentCnEmailList = db.Config_Application.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail);

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
                                    @"<table style='border-collapse: collapse;'>
                                      <tr>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Contribution Credited</th>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Modified By</th>
                                        <th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Modified Date</th>
                                      </tr>");
                                var tableRows = string.Empty;
                                foreach (var item in getContributionItems)
                                {
                                    tableRows += string.Format(@"<tr>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td>
                                            <td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td>
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

        public void TS_AmendAfterCutOff(List<ISSD_TradeSettlement> itemBefore, List<ISSD_TradeSettlement> itemAfter, ISSD_FormHeader form)
        {
            try
            {
                if (form.FormStatus == Common.FormStatus.PendingApproval && DateTime.Now.Hour >= 10)
                {
                    using (var db = new kashflowDBEntities())
                    {
                        var listOfPowerUsers = new InternetAddressList();
                        var powerUsers =
                            db.AspNetRoles.Where(x => x.Name == Config.Acl.PowerUser).ToList();
                        foreach (var user in powerUsers)
                        {
                            foreach (var item in user.AspNetUsers)
                            {
                                listOfPowerUsers.Add(new MailboxAddress(item.FullName, item.Email));
                            }
                        }

                        var message = ComposeAmendmentAfterCutOffMail(itemBefore, itemAfter, form, listOfPowerUsers);
                        SendEmailToSmtp(message);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }


        public void TS_IncomingFund(int formId, string formType, string approvedBy, string notes)
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
                        Logger.LogError("TS_IncomingFund email failed");
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

        private MimeMessage ComposeAmendmentAfterCutOffMail(List<ISSD_TradeSettlement> itemBefore, List<ISSD_TradeSettlement> itemAfter, ISSD_FormHeader form, InternetAddressList powerUsers)
        {
            var message = new MimeMessage()
            {
                Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                Subject = $"[Kashflow] Submitted {form.FormType} form amended"
            };
            message.To.AddRange(powerUsers);

            var formUrl = $"{Config.EmailApplicationUrl}{Common.Email_FormUrlMap(form.FormType)}{form.Id}";

            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append("<p>Hi there, </p>");
            bodyBuilder.AppendLine($"<p>A submitted form <a href='{formUrl}'>#{form.Id}</a> has been amended.</p>");

            bodyBuilder.AppendLine("<p>Data before changes: </p>");
            bodyBuilder.Append(ConstructTable(itemBefore));

            bodyBuilder.AppendLine("<p>Data after changes: </p>");
            bodyBuilder.Append(ConstructTable(itemAfter));

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = bodyBuilder.ToString()
            };

            return message;
        }

        private StringBuilder ConstructTable(List<ISSD_TradeSettlement> tradeItems)
        {
            var output = new StringBuilder();
            
            var equity = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Equity).ToList();
            if (equity.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Equity</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Maturity (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Sales (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Purchase (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in equity)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{5}</td></tr>",
                        item.InstrumentCode, item.StockCode,
                        item.Maturity.ToString("N"), 
                         item.Sales.ToString("N"),  item.Purchase.ToString("N"),
                        item.Remarks));
                }
                output.Append("</table>");
            }

            var bond = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Bond).ToList();
            if (bond.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Bond</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Maturity (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Sales (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Purchase (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in bond)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{5}</td></tr>",
                        item.InstrumentCode, item.StockCode,
                         item.Maturity.ToString("N"),  item.Sales.ToString("N"),  item.Purchase.ToString("N"),
                        item.Remarks));
                }
                output.Append("</table>");
            }

            var cp = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Cp).ToList();
            if (cp.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>CP</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Maturity (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Sales (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Purchase (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in cp)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{5}</td></tr>",
                        item.InstrumentCode, item.StockCode,
                         item.Maturity.ToString("N"),  item.Sales.ToString("N"),  item.Purchase.ToString("N"),
                        item.Remarks));
                }
                output.Append("</table>");
            }

            var np = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.NotesPapers).ToList();
            if (np.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Notes/Papers</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Maturity (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Sales (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Purchase (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in np)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{5}</td></tr>",
                        item.InstrumentCode, item.StockCode,
                         item.Maturity.ToString("N"),  item.Sales.ToString("N"),  item.Purchase.ToString("N"),
                        item.Remarks));
                }
                output.Append("</table>");
            }

            var repo = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Repo).ToList();
            if (repo.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>REPO</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>1st Leg(+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>2st Leg (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in repo)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td></tr>",
                        item.InstrumentCode, item.StockCode,
                        item.FirstLeg.ToString("N"), item.SecondLeg.ToString("N"), 
                        item.Remarks));
                }
                output.Append("</table>");
            }

            var coupon = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Coupon).ToList();
            if (coupon.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Coupon</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in coupon)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td></tr>",
                        item.InstrumentCode, item.StockCode, item.AmountPlus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            var mtm = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Mtm).ToList();
            if (mtm.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Payment/Receipt (MTM)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in mtm)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td></tr>",
                        item.InstrumentCode, item.StockCode, item.AmountPlus.ToString("N"), item.AmountMinus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            var fx = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Fx).ToList();
            if (fx.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>FX</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Stock Code/ISIN</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in fx)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{4}</td></tr>",
                        item.InstrumentCode, item.StockCode, item.AmountPlus.ToString("N"), item.AmountMinus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            var altid = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Altid).ToList();
            if (altid.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>ALTID</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (-)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in altid)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{3}</td></tr>",
                        item.InstrumentCode, item.AmountPlus.ToString("N"), item.AmountMinus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            var fees = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Fees).ToList();
            if (fees.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Fees</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in fees)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td></tr>",
                        item.InstrumentCode, item.AmountPlus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            var cn = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Cn).ToList();
            if (cn.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Contribution Credited</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in cn)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td></tr>",
                        item.InstrumentCode, item.AmountPlus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            var others = tradeItems.Where(x => x.InstrumentType == Common.TsItemCategory.Others).ToList();
            if (others.Any())
            {
                output.Append("<table style='border-collapse: collapse;'><tr><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Others</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Amount (+)</th><th style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>Remarks</th></tr>");
                foreach (var item in others)
                {
                    output.Append(string.Format(
                        "<tr><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{0}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{1}</td><td style='border: 1px solid #999;padding: 0.5rem;text-align: left;'>{2}</td></tr>",
                        item.InstrumentCode, item.AmountPlus.ToString("N"), item.Remarks));
                }
                output.Append("</table>");
            }

            return output;

        }

        private MimeMessage ComposeTS_IncomingFundMail(int formId, string formType, string approverName, string approverMail, string notes)
        {
            var message = new MimeMessage()
            {
                Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                Subject = "[Kashflow] Incoming Funds dated " + DateTime.Now.ToString("dd/MM/yyyy"),
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
    }
}
