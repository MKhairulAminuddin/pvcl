using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using MailKit.Net.Smtp;
using MimeKit;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services
{
    public class MailService
    {
        private string SubjectAppend => !Config.IsLive ? "[UAT - Kashflow]" : "[Kashflow]";

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
                        Subject = $"{SubjectAppend} Test Email",
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
                            Subject = $"{SubjectAppend} New AMSD Inflow Funds Approved"
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
        
        public void TS_PartE_NotifyPe(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var theForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId && x.FormType == Common.FormType.ISSD_TS_E);
                    
                    var message = new MimeMessage()
                    {
                        Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                        Subject = Common.MailSubjectWithDate(Config.NotificationTsPeEmailSubject)
                    };

                    var recipients = Config.NotificationTsPeEmail.Split(';');

                    foreach (var recipient in recipients)
                    {
                        message.To.Add(MailboxAddress.Parse(recipient));
                    }

                    var cc = Config.NotificationTsPeEmailCc.Split(';');

                    foreach (var recipient in cc)
                    {
                        message.Cc.Add(MailboxAddress.Parse(recipient));
                    }
                    
                    var bodyBuilder = new StringBuilder();
                    bodyBuilder.Append($"<p>Dear All, </p>");
                    bodyBuilder.AppendLine($"<p>A Trade Settlement ({theForm.FormType}) ({theForm.Currency}) form dated {theForm.SettlementDate?.ToString("dd/MM/yyyy")} has been submitted and approved by ISSD. Below are the details of the submission. </p>");

                    var sb = new StringBuilder();
                    
                    var tsFormPartE = db.ISSD_TradeSettlement
                        .Where(x => x.FormId == formId)
                        .ToList();
                
                    if (tsFormPartE.Any())
                    {
                        sb.AppendLine("<br/><b>Trade Settlement (Part E - ALTID)</b><br/>");

                        using (var table = new Common.Table(sb))
                        {
                            using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                            {
                                row.AddCell("ALTID Distribution & Drawdown");
                                row.AddCell("Amount (+)");
                                row.AddCell("Amount (-)");
                                row.AddCell("Remarks");
                            }

                            foreach (var item in tsFormPartE)
                            {
                                using (var row = table.AddRow())
                                {
                                    row.AddCell(item.InstrumentCode);
                                    row.AddCell(item.AmountPlus.ToString("N"));
                                    row.AddCell(item.AmountMinus.ToString("N"));
                                    row.AddCell(item.Remarks);
                                }
                            }
                        }

                        bodyBuilder.Append(sb);
                        bodyBuilder.AppendLine(Common.EmailTemplate.Footer);


                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };


                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("TS_PartE_NotifyPe email failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void TS_PartH_Notify(int formId, string othersType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var theForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId && x.FormType == Common.FormType.ISSD_TS_H);

                    var message = new MimeMessage()
                    {
                        Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount)
                    };

                    var recipients = new List<string>();
                    var cc = new List<string>();
                    var tsFormPartE = new List<ISSD_TradeSettlement>();

                    // 24/12/2021 - premah requested to remove
                    /*if (othersType == Common.TsOthersTypeItem.Property)
                    {
                        message.Subject = Common.MailSubjectWithDate(Config.NotificationTsPropertyEmailSubject);

                        recipients = Config.NotificationTsPropertyEmail.Split(';').ToList();
                        cc = Config.NotificationTsPropertyEmailCc.Split(';').ToList();
                        tsFormPartE = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == formId && x.OthersType == Common.TsOthersTypeItem.Property)
                            .ToList();
                    }*/
                    if (othersType == Common.TsOthersTypeItem.Loan)
                    {
                        message.Subject = Common.MailSubjectWithDate(Config.NotificationTsLoanEmailSubject);

                        recipients = Config.NotificationTsLoanEmail.Split(';').ToList();
                        cc = Config.NotificationTsLoanEmailCc.Split(';').ToList();
                        tsFormPartE = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == formId && x.OthersType == Common.TsOthersTypeItem.Loan)
                            .ToList();
                    }

                    foreach (var recipient in recipients)
                    {
                        message.To.Add(MailboxAddress.Parse(recipient));
                    }
                    
                    foreach (var recipient in cc)
                    {
                        message.Cc.Add(MailboxAddress.Parse(recipient));
                    }

                    var bodyBuilder = new StringBuilder();
                    bodyBuilder.Append($"<p>Dear All, </p>");
                    bodyBuilder.AppendLine($"<p>A Trade Settlement ({theForm.FormType}) ({theForm.Currency}) form dated {theForm.SettlementDate?.ToString("dd/MM/yyyy")} has been submitted and approved by ISSD. Below are the details of the submission. </p>");

                    var sb = new StringBuilder();

                    if (tsFormPartE.Any())
                    {
                        sb.AppendLine("<br/><b>Trade Settlement (Part H - Others) " + othersType + " </b><br/>");

                        using (var table = new Common.Table(sb))
                        {
                            using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                            {
                                row.AddCell("Others");
                                row.AddCell("Amount (+)");
                                row.AddCell("Amount (-)");
                                row.AddCell("Remarks");
                            }

                            foreach (var item in tsFormPartE)
                            {
                                using (var row = table.AddRow())
                                {
                                    row.AddCell(item.InstrumentCode);
                                    row.AddCell(item.AmountPlus.ToString("N"));
                                    row.AddCell(item.AmountMinus.ToString("N"));
                                    row.AddCell(item.Remarks);
                                }
                            }
                        }

                        bodyBuilder.Append(sb);
                        bodyBuilder.AppendLine(Common.EmailTemplate.Footer);

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };


                        SendEmailToSmtp(message);
                    }
                    else
                    {
                        Logger.LogError("TS_PartH_Notify email failed");
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


        /// <summary>
        /// Notification on approved TS to FID user group
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="formType"></param>
        /// <param name="currency"></param>
        public void TS_IncomingFund(int formId, string formType, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var fidUsers = db.AspNetUsers.Where(user =>
                        user.AspNetRoles.Any(r => r.Name == Config.Acl.Fid || r.Name == Config.Acl.PowerUser));

                    var message = new MimeMessage()
                    {
                        Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                        Subject = $"{SubjectAppend} Approved ISSD Trade Settlement "
                    };

                    foreach (var fidUser in fidUsers)
                    {
                        message.To.Add(new MailboxAddress(fidUser.FullName, fidUser.Email));
                    }

                    var pageUrl = string.Format("{0}" + Common.Email_FormUrlMap(formType) + "{1}", Config.EmailApplicationUrl, formId);

                    var bodyBuilder = new StringBuilder();
                    bodyBuilder.Append($"<p>Dear All, </p>");
                    bodyBuilder.AppendLine($"<p>A {formType} ({currency}) form has been approved. Below are the details of it. </p>");

                    var sb = new StringBuilder();

                    if (fidUsers.Any())
                    {
                        var tsItems = db.ISSD_TradeSettlement.Where(x => x.FormId == formId).ToList();

                        if (tsItems.Any(x => x.InstrumentType == Common.TsItemCategory.Equity ||
                                             x.InstrumentType == Common.TsItemCategory.Bond ||
                                             x.InstrumentType == Common.TsItemCategory.Cp ||
                                             x.InstrumentType == Common.TsItemCategory.NotesPapers))
                        {
                            using (var table = new Common.Table(sb))
                            {
                                using (var row = table.AddHeaderRow())
                                {
                                    row.AddCell("Type");
                                    row.AddCell("Details");
                                    row.AddCell("Stock Code/ ISIN");
                                    row.AddCell("Maturity (+)");
                                    row.AddCell("Sales (+)");
                                    row.AddCell("Purchase (-)");
                                    row.AddCell("Remarks");
                                }

                                foreach (var item in tsItems)
                                {
                                    using (var row = table.AddRow())
                                    {
                                        row.AddCell(item.InstrumentType);
                                        row.AddCell(item.InstrumentCode);
                                        row.AddCell(item.StockCode);
                                        row.AddCell_IntegerType(item.Maturity.ToString("N"), true);
                                        row.AddCell_IntegerType(item.Sales.ToString("N"), true);
                                        row.AddCell_IntegerType(item.Purchase.ToString("N"), false);
                                        row.AddCell(item.Remarks);
                                    }
                                }

                                
                            }
                        }
                        else if (tsItems.Any(x => x.InstrumentType == Common.TsItemCategory.Repo))
                        {
                            using (var table = new Common.Table(sb))
                            {
                                using (var row = table.AddHeaderRow())
                                {
                                    row.AddCell("Type");
                                    row.AddCell("Details");
                                    row.AddCell("Stock Code/ ISIN");
                                    row.AddCell("1st Leg (+)");
                                    row.AddCell("2nd Leg (-)");
                                    row.AddCell("Remarks");
                                }

                                foreach (var item in tsItems)
                                {
                                    using (var row = table.AddRow())
                                    {
                                        row.AddCell(item.InstrumentType);
                                        row.AddCell(item.InstrumentCode);
                                        row.AddCell(item.StockCode);
                                        row.AddCell_IntegerType(item.FirstLeg.ToString("N"), true);
                                        row.AddCell_IntegerType(item.SecondLeg.ToString("N"), false);
                                        row.AddCell(item.Remarks);
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (var table = new Common.Table(sb))
                            {
                                using (var row = table.AddHeaderRow())
                                {
                                    row.AddCell("Type");
                                    row.AddCell("Details");
                                    row.AddCell("Stock Code/ ISIN");
                                    row.AddCell_IntegerType("Amount (+)", true);
                                    row.AddCell_IntegerType("Amount (-)", false);
                                    row.AddCell("Remarks");
                                }

                                foreach (var item in tsItems)
                                {
                                    using (var row = table.AddRow())
                                    {
                                        row.AddCell(item.InstrumentType);
                                        row.AddCell(item.InstrumentCode);
                                        row.AddCell(item.StockCode);
                                        row.AddCell(item.AmountPlus.ToString("N"));
                                        row.AddCell(item.AmountMinus.ToString("N"));
                                        row.AddCell(item.Remarks);
                                    }
                                }

                                
                            }
                        }

                        bodyBuilder.Append(sb);
                        
                        bodyBuilder.AppendLine($"<p>Click <a href='{pageUrl}'>here</a> to view the submission in Kashflow.</p>");
                        bodyBuilder.AppendLine(Common.EmailTemplate.Footer);

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };


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

        public void FcaBankTagging(List<ISSD_TradeSettlement> tradeSettlementItems)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var message = new MimeMessage()
                    {
                        Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount)
                    };

                    var recipients = new List<string>();
                    recipients = Config.NotiTreasuryIssdEmail.Split(';').ToList();
                    foreach (var recipient in recipients)
                    {
                        message.To.Add(MailboxAddress.Parse(recipient));
                    }

                    var Ids = tradeSettlementItems.Select(x => x.FormId).Distinct().ToList();
                    var forms = db.ISSD_FormHeader.Where(x => Ids.Contains(x.Id)).ToList();

                    if (forms.Any())
                    {
                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append($"<p>Hello there, </p>");
                        bodyBuilder.AppendLine($"<p>Below Trade Settlement items has been tagged with Bank code. </p>");

                        foreach (var f in forms)
                        {
                            bodyBuilder.AppendLine("<br/><b>" + f.FormType + " </b><br/>");
                            
                            using (var table = new Common.Table(bodyBuilder))
                            {
                                using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                                {
                                    row.AddCell("Settlement Date");
                                    row.AddCell("Type");
                                    row.AddCell("Code");
                                    row.AddCell("Stock Code");
                                    row.AddCell("Inflow");
                                    row.AddCell("Outflow");
                                    row.AddCell("Tagged By");
                                    row.AddCell("Tagged Datetime");
                                }

                                foreach (var ts in tradeSettlementItems.Where(x => x.FormId == f.Id))
                                {
                                    using (var row = table.AddRow())
                                    {
                                        row.AddCell(f.SettlementDate?.ToString("dd/MM/yyyy"));
                                        row.AddCell(ts.InstrumentType);
                                        row.AddCell(ts.InstrumentCode);
                                        row.AddCell(ts.StockCode);
                                        row.AddCell(ts.InflowTo);
                                        row.AddCell(ts.OutflowFrom);
                                        row.AddCell(ts.AssignedBy);
                                        row.AddCell(ts.AssignedDate?.ToString("dd/MM/yyyy HH:mm"));
                                    }
                                }
                                
                            }
                        }

                        bodyBuilder.AppendLine(Common.EmailTemplate.Footer);

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

                        SendEmailToSmtp(message);
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
                Subject = $"{SubjectAppend} Request for Approval {formType}",
                To =
                {
                    new MailboxAddress(approverName, approverMail)
                }
            };

            var approvalPageUrl = string.Format("{0}" + Common.Email_FormUrlMap(formType) + "{1}",
                Config.EmailApplicationUrl, formId);
            
            var bodyBuilder = new StringBuilder();

            bodyBuilder.Append($"<p>Dear {approverName}, </p>");
            bodyBuilder.AppendLine($"<p>A {formType} form is pending for your approval. </p>");
            bodyBuilder.AppendLine($"<p>Click <a href='{approvalPageUrl}'>here</a> to view.</p>");

            if (formType == Common.FormType.FID_TREASURY)
            {
                bodyBuilder.AppendLine(TreasuryTable(formId));
            }
            else if (Common.IsTsFormType(formType))
            {
                bodyBuilder.AppendLine(TsTable(formId));
            }
            
            if (!string.IsNullOrEmpty(notes))
            {
                bodyBuilder.AppendLine($"<br/><br/><p style='font-weight:bold'>Notes from preparer: </p>");
                bodyBuilder.AppendLine($"<p>{notes}</p>");
            }

            bodyBuilder.AppendLine(Common.EmailTemplate.Footer);

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
                Subject = $"{SubjectAppend} Approval Status for submitted {formType} form",
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
            
            if (formType == Common.FormType.FID_TREASURY)
            {
                bodyBuilder.AppendLine(TreasuryTable(formId));
            }
            else if (Common.IsTsFormType(formType))
            {
                bodyBuilder.AppendLine(TsTable(formId));
            }

            if (!string.IsNullOrEmpty(notes))
            {
                bodyBuilder.AppendLine($"<br/><br/><p style='font-weight:bold'>Notes from approver: </p>");
                bodyBuilder.AppendLine($"<p>{notes}</p>");
            }

            bodyBuilder.AppendLine(Common.EmailTemplate.Footer);
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
                Subject = $"{SubjectAppend} Submitted {form.FormType} form amended"
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

        private string TreasuryTable(int formId)
        {
            using (var db = new kashflowDBEntities())
            {
                var bodyBuilder = new StringBuilder();
                bodyBuilder.AppendLine();

                var sb = new StringBuilder();
                
                var treasuryDepositInflow = db.FID_Treasury_Deposit
                    .Where(x => x.FormId == formId && x.CashflowType == Common.Cashflow.Inflow)
                    .ToList();

                var treasuryMmiInflow = db.FID_Treasury_MMI
                    .Where(x => x.FormId == formId && x.CashflowType == Common.Cashflow.Inflow)
                    .ToList();

                var treasuryDepositOutflow = db.FID_Treasury_Deposit
                    .Where(x => x.FormId == formId && x.CashflowType == Common.Cashflow.Outflow)
                    .ToList();

                var treasuryMmiOutflow = db.FID_Treasury_MMI
                    .Where(x => x.FormId == formId && x.CashflowType == Common.Cashflow.Outflow)
                    .ToList();

                if (treasuryDepositInflow.Any())
                {
                    sb.AppendLine("<br/><b>Fund Inflow - Deposit</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("Dealer");
                            row.AddCell("Bank");
                            row.AddCell("Trade Date");
                            row.AddCell("Value Date");
                            row.AddCell("Maturity Date");
                            row.AddCell("Principal");
                            row.AddCell("Tenor (day)");
                            row.AddCell("Interest/ Profit Receivable");
                            row.AddCell("Principal + Interest/ Profit Receivable");
                            row.AddCell("Asset Type");
                            row.AddCell("Repo Tag");
                            row.AddCell("Contact Person");
                            row.AddCell("Notes");
                        }

                        foreach (var item in treasuryDepositInflow)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(item.Dealer);
                                row.AddCell(item.Bank);
                                row.AddCell(item.TradeDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.ValueDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.MaturityDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.Principal.ToString("N"));
                                row.AddCell(item.Tenor.ToString());
                                row.AddCell(item.IntProfitReceivable.ToString("N"));
                                row.AddCell(item.PrincipalIntProfitReceivable.ToString("N"));
                                row.AddCell(item.AssetType);
                                row.AddCell(item.RepoTag);
                                row.AddCell(item.ContactPerson);
                                row.AddCell(item.Notes);
                            }
                        }
                    }
                }

                if (treasuryMmiInflow.Any())
                {
                    sb.AppendLine("<br/><b>Fund Inflow - Money Market</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("Dealer");
                            row.AddCell("Issuer");
                            row.AddCell("Product Type");
                            row.AddCell("CounterParty");
                            row.AddCell("Trade Date");
                            row.AddCell("Value Date");
                            row.AddCell("Maturity Date");
                            row.AddCell("Tenor");
                            row.AddCell("Nominal");
                            row.AddCell("Sell Rate/ Yield (%)");
                            row.AddCell("Price");
                            row.AddCell("Purchase Proceeds");
                            row.AddCell("Interest/ Dividend Receivable");
                            row.AddCell("Proceeds");
                            row.AddCell("Cert No/ Stock Code");
                        }

                        foreach (var item in treasuryMmiInflow)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(item.Dealer);
                                row.AddCell(item.Issuer);
                                row.AddCell(item.ProductType);
                                row.AddCell(item.CounterParty);
                                row.AddCell(item.ValueDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.TradeDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.MaturityDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.HoldingDayTenor.ToString());
                                row.AddCell(item.Nominal.ToString("N"));
                                row.AddCell(item.SellPurchaseRateYield.ToString("N"));
                                row.AddCell(item.Price.ToString("N"));
                                row.AddCell(item.PurchaseProceeds.ToString("N"));
                                row.AddCell(item.IntDividendReceivable.ToString("N"));
                                row.AddCell(item.Proceeds.ToString("N"));
                                row.AddCell(item.CertNoStockCode);
                            }
                        }
                    }
                }

                if (treasuryDepositOutflow.Any())
                {
                    sb.AppendLine("<br/><b>Fund Outflow - Deposit</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#F39C12", "black"))
                        {
                            row.AddCell("Dealer");
                            row.AddCell("Bank");
                            row.AddCell("Trade Date");
                            row.AddCell("Value Date");
                            row.AddCell("Maturity Date");
                            row.AddCell("Principal");
                            row.AddCell("Tenor (day)");
                            row.AddCell("Interest/ Profit Receivable");
                            row.AddCell("Principal + Interest/ Profit Receivable");
                            row.AddCell("Asset Type");
                            row.AddCell("Repo Tag");
                            row.AddCell("Contact Person");
                            row.AddCell("Notes");
                        }

                        foreach (var item in treasuryDepositOutflow)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(item.Dealer);
                                row.AddCell(item.Bank);
                                row.AddCell(item.TradeDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.ValueDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.MaturityDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.Principal.ToString("N"));
                                row.AddCell(item.Tenor.ToString());
                                row.AddCell(item.IntProfitReceivable.ToString("N"));
                                row.AddCell(item.PrincipalIntProfitReceivable.ToString("N"));
                                row.AddCell(item.AssetType);
                                row.AddCell(item.RepoTag);
                                row.AddCell(item.ContactPerson);
                                row.AddCell(item.Notes);
                            }
                        }
                    }
                }

                if (treasuryMmiOutflow.Any())
                {
                    sb.AppendLine("<br/><b>Fund Outflow - Money Market</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#F39C12", "black"))
                        {
                            row.AddCell("Dealer");
                            row.AddCell("Issuer");
                            row.AddCell("Product Type");
                            row.AddCell("CounterParty");
                            row.AddCell("Trade Date");
                            row.AddCell("Value Date");
                            row.AddCell("Maturity Date");
                            row.AddCell("Tenor");
                            row.AddCell("Nominal");
                            row.AddCell("Sell Rate/ Yield (%)");
                            row.AddCell("Price");
                            row.AddCell("Purchase Proceeds");
                            row.AddCell("Interest/ Dividend Receivable");
                            row.AddCell("Proceeds");
                            row.AddCell("Cert No/ Stock Code");
                        }

                        foreach (var item in treasuryMmiOutflow)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(item.Dealer);
                                row.AddCell(item.Issuer);
                                row.AddCell(item.ProductType);
                                row.AddCell(item.CounterParty);
                                row.AddCell(item.TradeDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.ValueDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.MaturityDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.HoldingDayTenor.ToString());
                                row.AddCell(item.Nominal.ToString("N"));
                                row.AddCell(item.SellPurchaseRateYield.ToString("N"));
                                row.AddCell(item.Price.ToString("N"));
                                row.AddCell(item.PurchaseProceeds.ToString("N"));
                                row.AddCell(item.IntDividendReceivable.ToString("N"));
                                row.AddCell(item.Proceeds.ToString("N"));
                                row.AddCell(item.CertNoStockCode);
                            }
                        }
                    }
                }

                bodyBuilder.Append(sb);

                return bodyBuilder.ToString();
            }
        }

        private string TsTable(int formId)
        {
            using (var db = new kashflowDBEntities())
            {
                var theForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);
                
                var bodyBuilder = new StringBuilder();

                var sb = new StringBuilder();

                var tsEquity = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Equity)
                    .ToList();

                var tsBond = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Bond)
                    .ToList();

                var tsCoupon = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Coupon)
                    .ToList();

                var tsCp = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Cp)
                    .ToList();

                var tsRepo = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Repo)
                    .ToList();

                var tsNotesPapers = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.NotesPapers)
                    .ToList();

                var tsMtm = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Mtm)
                    .ToList();

                var tsFx = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Fx)
                    .ToList();

                var tsAltid = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Altid)
                    .ToList();

                var tsFees = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Fees)
                    .ToList();

                var tsCn = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Cn)
                    .ToList();

                var tsOthers = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == Common.TsItemCategory.Others)
                    .ToList();


                if (tsEquity.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Equity}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Equity);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsEquity)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.StockCode);
                                row.AddCell(item.Maturity.ToString("N"));
                                row.AddCell(item.Sales.ToString("N"));
                                row.AddCell(item.Purchase.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsBond.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Bond}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Bond);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Bond Type");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsBond)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.StockCode);
                                row.AddCell(item.BondType);
                                row.AddCell(item.Maturity.ToString("N"));
                                row.AddCell(item.Sales.ToString("N"));
                                row.AddCell(item.Purchase.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsCoupon.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Coupon}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Coupon);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Coupon Type");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsCoupon)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.StockCode);
                                row.AddCell(item.CouponType);
                                row.AddCell(item.Maturity.ToString("N"));
                                row.AddCell(item.Sales.ToString("N"));
                                row.AddCell(item.Purchase.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsCp.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Cp}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Cp);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsCp)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.StockCode);
                                row.AddCell(item.Maturity.ToString("N"));
                                row.AddCell(item.Sales.ToString("N"));
                                row.AddCell(item.Purchase.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsNotesPapers.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.NotesPapers}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.NotesPapers);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsNotesPapers)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.StockCode);
                                row.AddCell(item.Maturity.ToString("N"));
                                row.AddCell(item.Sales.ToString("N"));
                                row.AddCell(item.Purchase.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsRepo.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Repo}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Repo);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("First Leg (+)");
                            row.AddCell("Second Leg (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsRepo)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.StockCode);
                                row.AddCell(item.FirstLeg.ToString("N"));
                                row.AddCell(item.SecondLeg.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsMtm.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Mtm}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Mtm);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsMtm)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsFx.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Fx}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Fx);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsFx)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsAltid.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Altid}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Altid);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsAltid)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsFees.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Fees}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Fees);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsFees)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsCn.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Cn}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Cn);
                            row.AddCell("Amount (+)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsCn)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tsOthers.Any())
                {
                    sb.AppendLine($"<br/><b>{Common.TsItemCategory.Others}</b><br/>");

                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Settlement Date");
                            row.AddCell(Common.TsItemCategory.Others);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Type");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tsOthers)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.OthersType);
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                bodyBuilder.Append(sb);

                return bodyBuilder.ToString();
            }
        }
    }
}
