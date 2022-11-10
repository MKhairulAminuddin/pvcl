using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using MailKit.Net.Smtp;
using MimeKit;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services
{
    public static class EmailNotificationService
    {
        private static string SubjectAppend => !Config.IsLive ? "[UAT - Kashflow]" : "[Kashflow]";

        public static void TestSendEmail(string recipient)
        {
            try
            {
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

                SendEmail(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

        }

        #region Workflow Email

        public static void WF_ApprovalSubmission(int formId, string formType, string approvedBy, string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var approver = db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == approvedBy);

                    if (approver != null)
                    {
                        var message = ComposeSubmitForApprovalMail(formId, formType, approver.DisplayName, approver.Email, notes);
                        SendEmail(message);
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

        public static void WF_ApprovalResult(int formId, string formType, string formStatus, string formPreparer, string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var preparer = db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == formPreparer);

                    if (preparer != null)
                    {
                        var message = ComposeApprovalFeedbackMail(formId, formType, formStatus, preparer.DisplayName, preparer.Email, notes);
                        SendEmail(message);
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

        #endregion

        #region Inflow Fund Form Notification

        public static void IFForm_Approved(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                    var enableNotify = EnableNotification(EmailNotiKey.Enable_FID_IF_Approved);

                    if (form != null && enableNotify)
                    {
                        var bodyBuilder = new StringBuilder();
                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/IFForm_Approved.html"))
                        {
                            string readFile = reader.ReadToEnd();
                            string StrContent = string.Empty;
                            StrContent = readFile;
                            StrContent = StrContent.Replace("[DataTable]", InflowFundTable(form.Id));
                            bodyBuilder.Append(StrContent);
                        }

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = $"{SubjectAppend} New AMSD Inflow Fund Form Approved",
                            Body = new TextPart(MimeKit.Text.TextFormat.Html)
                            {
                                Text = bodyBuilder.ToString()
                            },
                        };
                        message.To.AddRange(ReceipientsFromConfig(EmailNotiKey.FID_IF_Approved));

                        SendEmail(message);
                    }
                    else
                    {
                        Logger.LogError("NewlyApproved_InflowFundForm FAILED, form data error: " + formId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        #endregion

        #region Trade Settlement Form Notification

        public static void TSForm_Approved(int formId, string formType, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var fidUsers = db.AspNetUsers.Where(user => user.AspNetRoles.Any(r => r.Name == Config.Acl.Fid || r.Name == Config.Acl.PowerUser));

                    if (fidUsers.Any())
                    {
                        var bodyBuilder = new StringBuilder();
                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/IFForm_Approved.html"))
                        {
                            string readFile = reader.ReadToEnd();
                            string StrContent = string.Empty;
                            StrContent = readFile;
                            StrContent = StrContent.Replace("[PageUrl]", string.Format("{0}" + Common.Email_FormUrlMap(formType) + "{1}", Config.EmailApplicationUrl, formId));
                            StrContent = StrContent.Replace("[DataTable]", TsTable(formId));
                            bodyBuilder.Append(StrContent);
                        }

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = $"{SubjectAppend} Approved ISSD Trade Settlement ",
                            Body = new TextPart(MimeKit.Text.TextFormat.Html)
                            {
                                Text = bodyBuilder.ToString()
                            }
                        };

                        foreach (var fidUser in fidUsers)
                        {
                            message.To.Add(new MailboxAddress(fidUser.FullName, fidUser.Email));
                        }

                        SendEmail(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void TSForm_PartE_PE(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotifyEnabled = EnableNotification(EmailNotiKey.Enable_ISSD_TS_PeEmail);
                    var isNotifyCcEnabled = EnableNotification(EmailNotiKey.Enable_ISSD_TS_PeEmail_Cc);

                    if (isNotifyEnabled)
                    {
                        var theForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId && x.FormType == Common.FormType.ISSD_TS_E);

                        var bodyBuilder = new StringBuilder();
                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/TSForm_PartE_PE.html"))
                        {
                            string readFile = reader.ReadToEnd();
                            string StrContent = string.Empty;
                            StrContent = readFile;
                            StrContent = StrContent.Replace("[FormType]", theForm.FormType);
                            StrContent = StrContent.Replace("[FormCurrency]", theForm.Currency);
                            StrContent = StrContent.Replace("[SettlementDate]", theForm.SettlementDate?.ToString("dd/MM/yyyy"));
                            StrContent = StrContent.Replace("[DataTable]", TsTable(formId));
                            bodyBuilder.Append(StrContent);
                        }

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = Common.MailSubjectWithDate(Config.NotificationTsPeEmailSubject),
                            Body = new TextPart(MimeKit.Text.TextFormat.Html)
                            {
                                Text = bodyBuilder.ToString()
                            }
                        };

                        message.To.AddRange(ReceipientsFromConfig(EmailNotiKey.ISSD_TS_PeEmail));

                        if (isNotifyCcEnabled)
                        {
                            message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.ISSD_TS_PeEmail_Cc));
                        }

                        SendEmail(message);
                    }
                    else
                    {
                        Logger.LogError("TSForm_PartE_NotifyPE email disabled");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void TSForm_PartH_Loan(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotifyEnabled = EnableNotification(EmailNotiKey.Enable_ISSD_TS_LoanEmail);
                    var isNotifyCcEnabled = EnableNotification(EmailNotiKey.Enable_ISSD_TS_LoanEmail_Cc);

                    if (isNotifyEnabled)
                    {
                        var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId && x.FormType == Common.FormType.ISSD_TS_H);

                        var bodyBuilder = new StringBuilder();
                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/TSForm_PartE_PE.html"))
                        {
                            string readFile = reader.ReadToEnd();
                            string StrContent = string.Empty;
                            StrContent = readFile;
                            StrContent = StrContent.Replace("[FormType]", form.FormType);
                            StrContent = StrContent.Replace("[FormCurrency]", form.Currency);
                            StrContent = StrContent.Replace("[SettlementDate]", form.SettlementDate?.ToString("dd/MM/yyyy"));
                            StrContent = StrContent.Replace("[DataTable]", TsTable(formId));
                            bodyBuilder.Append(StrContent);
                        }

                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = Common.MailSubjectWithDate(Config.NotificationTsLoanEmailSubject),
                            Body = new TextPart(MimeKit.Text.TextFormat.Html)
                            {
                                Text = bodyBuilder.ToString()
                            }
                        };

                        message.To.AddRange(ReceipientsFromConfig(EmailNotiKey.ISSD_TS_LoanEmail));

                        if (isNotifyCcEnabled)
                        {
                            message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.ISSD_TS_LoanEmail_Cc));
                        }

                        SendEmail(message);

                    }

                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        #endregion

        #region Treasury Form Notification

        public static void TForm_ISSD_Approved()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotifyEnabled = EnableNotification(EmailNotiKey.Enable_ISSD_T_Approval);

                    if (isNotifyEnabled)
                    {
                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = $"{SubjectAppend}{Config.NotiTreasuryIssdEmailSubject}"
                        };

                        message.To.AddRange(ReceipientsFromConfig(EmailNotiKey.ISSD_T_Approval));

                        var bodyBuilder = new StringBuilder();
                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/Treasury_approved_toISSD.html"))
                        {
                                
                            string readFile = reader.ReadToEnd();
                            string StrContent = string.Empty;
                            StrContent = readFile;
                            //Assing the field values in the template
                            StrContent = StrContent.Replace("[Message]", "FID has approved a Cash flow.");
                            bodyBuilder.Append(StrContent);
                        }

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

                        SendEmail(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        #endregion

        #region FCA Bank Tagging Notification

        public static void FcaBankTaggingToIssd(List<ISSD_TradeSettlement> tradeSettlementItems)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isNotifyEnabled = EnableNotification(EmailNotiKey.Enable_ISSD_FcaTagging);

                    if (isNotifyEnabled)
                    {
                        var message = new MimeMessage()
                        {
                            Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                            Subject = $"{SubjectAppend}{Config.NotiFcaTaggingIssdEmailSubject}"
                        };
                        message.To.AddRange(ReceipientsFromConfig(EmailNotiKey.ISSD_FcaTagging));

                        var bodyBuilder = new StringBuilder();
                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/FcaTaggingStatus_ToISSD.html"))
                        {
                            string readFile = reader.ReadToEnd();
                            string StrContent = string.Empty;
                            StrContent = readFile;
                            StrContent = StrContent.Replace("[Message]", "Below Trade Settlement items has been tagged with Bank code.");
                            StrContent = StrContent.Replace("[FcaTaggingTable]", FcaTaggingTable(tradeSettlementItems));
                            bodyBuilder.Append(StrContent);
                        }

                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = bodyBuilder.ToString()
                        };

                        SendEmail(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        #endregion

        #region Private Functions
        private static void SendEmail(MimeMessage message)
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

        public static List<MailboxAddress> ReceipientsFromConfig(string configKey)
        {
            using (var db = new kashflowDBEntities())
            {
                var configValue = db.Config_Application.FirstOrDefault(x => x.Key == configKey);
                if (configValue != null)
                {
                    var emailAddresses = configValue.Value.Split(',').ToList();
                    var mailboxAddress = new List<MailboxAddress>();
                    foreach (var email in emailAddresses)
                    {
                        mailboxAddress.Add(MailboxAddress.Parse(email));
                    }
                    return mailboxAddress;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool EnableNotification(string configKey)
        {
            using (var db = new kashflowDBEntities())
            {
                var configValue = db.Config_Application.FirstOrDefault(x => x.Key == configKey);
                if (configValue != null)
                {
                    var conversionStatus = bool.TryParse(configValue.Value, out bool enableNotify);

                    if (conversionStatus)
                    {
                        return enableNotify;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private static MimeMessage ComposeSubmitForApprovalMail(int formId, string formType, string approverName, string approverMail, string notes)
        {
            using (var db = new kashflowDBEntities())
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

                var approvalPageUrl = string.Format("{0}" + Common.Email_FormUrlMap(formType) + "{1}", Config.EmailApplicationUrl, formId);

                var bodyBuilder = new StringBuilder();
                var root = AppDomain.CurrentDomain.BaseDirectory;
                using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/SubmitForApproval.html"))
                {
                    string readFile = reader.ReadToEnd();
                    string StrContent = string.Empty;
                    StrContent = readFile;
                    //Assing the field values in the template
                    StrContent = StrContent.Replace("[ApproverName]", approverName);
                    StrContent = StrContent.Replace("[ApprovalUrl]", approvalPageUrl);
                    StrContent = StrContent.Replace("[FormType]", formType);
                    StrContent = StrContent.Replace("[Notes]", notes);

                    if (formType == Common.FormType.FID_TREASURY)
                    {
                        StrContent = StrContent.Replace("[DataTable]", TreasuryTable(formId));

                        var isNotifyCcEnabled = EnableNotification(EmailNotiKey.Enable_FID_T_Submission_Cc);
                        if (isNotifyCcEnabled)
                        {
                            message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.FID_T_Submission_Cc));
                        }
                    }
                    else if (Common.IsTsFormType(formType))
                    {
                        StrContent = StrContent.Replace("[DataTable]", TsTable(formId));
                    }
                    else if (formType == Common.FormType.AMSD_IF)
                    {
                        StrContent = StrContent.Replace("[DataTable]", InflowFundTable(formId));
                    }
                    else
                    {
                        StrContent = StrContent.Replace("[DataTable]", "");
                    }

                    bodyBuilder.Append(StrContent);
                }

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = bodyBuilder.ToString()
                };

                return message;
            }
        }

        private static MimeMessage ComposeApprovalFeedbackMail(int formId, string formType, string formStatus, string preparerName, string preparerMail, string notes)
        {
            using (var db = new kashflowDBEntities())
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

                var root = AppDomain.CurrentDomain.BaseDirectory;
                using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/ApprovalStatus.html"))
                {
                    string readFile = reader.ReadToEnd();
                    string StrContent = string.Empty;
                    StrContent = readFile;
                    //Assing the field values in the template
                    StrContent = StrContent.Replace("[PreparerName]", preparerName);
                    StrContent = StrContent.Replace("[ApprovalUrl]", approvalPageUrl);
                    StrContent = StrContent.Replace("[Notes]", notes);

                    if (formStatus == Common.FormStatus.Approved)
                    {
                        StrContent = StrContent.Replace("[ApprovalStatusMessage]",
                            $"<p>Good News! Your submitted form <a href='{approvalPageUrl}'>#{formId}</a> have been <span style='color:#2ECC71;'>{formStatus}</span>");
                    }
                    else
                    {
                        StrContent = StrContent.Replace("[ApprovalStatusMessage]",
                            $"<p>Bad News! Your submitted form <a href='{approvalPageUrl}'>#{formId}</a> have been <span style='color:#E74C3C;'>{formStatus}</span>");
                    }


                    if (formType == Common.FormType.FID_TREASURY)
                    {
                        StrContent = StrContent.Replace("[DataTable]", TreasuryTable(formId));

                        var isNotifyCcEnabled = EnableNotification(EmailNotiKey.Enable_FID_T_Approval_Cc);
                        if (isNotifyCcEnabled)
                        {
                            message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.FID_T_Approval_Cc));
                        }
                    }
                    else if (Common.IsTsFormType(formType))
                    {
                        StrContent = StrContent.Replace("[DataTable]", TsTable(formId));
                    }
                    else if (formType == Common.FormType.AMSD_IF)
                    {
                        StrContent = StrContent.Replace("[DataTable]", InflowFundTable(formId));
                    }
                    else
                    {
                        StrContent = StrContent.Replace("[DataTable]", "");
                    }


                    bodyBuilder.Append(StrContent);
                }

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = bodyBuilder.ToString()
                };

                return message;
            }
        }

        private static MimeMessage Compose_TsAmendApprovedForm(ISSD_FormHeader form, InternetAddressList powerUsers)
        {
            var bodyBuilder = new StringBuilder();
            var formUrl = $"{Config.EmailApplicationUrl}{Common.Email_FormUrlMap(form.FormType)}{form.Id}";

            var message = new MimeMessage()
            {
                Sender = new MailboxAddress(Config.SmtpSenderAccountName, Config.SmtpSenderAccount),
                Subject = $"{SubjectAppend} Submitted {form.FormType} form amended"
            };
            message.To.AddRange(powerUsers);

            var root = AppDomain.CurrentDomain.BaseDirectory;
            using (var reader = new System.IO.StreamReader(root + @"/App_Data/EmailTemplates/TS_AmendAfterCutOff.html"))
            {
                string readFile = reader.ReadToEnd();
                string StrContent = string.Empty;
                StrContent = readFile;
                StrContent = StrContent.Replace("[Message]", $"<p>An approved form <a href='{formUrl}'>#{form.Id}</a> has been amended.</p>");
                bodyBuilder.Append(StrContent);
            }

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = bodyBuilder.ToString()
            };

            return message;
        }

        private static StringBuilder ConstructTable(List<ISSD_TradeSettlement> tradeItems)
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
                         item.Sales.ToString("N"), item.Purchase.ToString("N"),
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
                         item.Maturity.ToString("N"), item.Sales.ToString("N"), item.Purchase.ToString("N"),
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
                         item.Maturity.ToString("N"), item.Sales.ToString("N"), item.Purchase.ToString("N"),
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
                         item.Maturity.ToString("N"), item.Sales.ToString("N"), item.Purchase.ToString("N"),
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

        private static string TreasuryTable(int formId)
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
                    sb.AppendLine("<p style='margin:0;font-size:14px;mso-line-height-alt:21px'><span style='font-size:14px;'><strong>Fund Inflow - Deposit</strong></span></p><br/>");

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
                            row.AddCell("Rate (%)");
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
                                row.AddCell(item.RatePercent.ToString("N"));
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
                    sb.AppendLine("<p style='margin:0;font-size:14px;mso-line-height-alt:21px'><span style='font-size:14px;'><strong>Fund Inflow - Money Market</strong></span></p><br/>");

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
                    sb.AppendLine("<p style='margin:0;font-size:14px;mso-line-height-alt:21px'><span style='font-size:14px;'><strong>Fund Outflow - Deposit</strong></span></p><br/>");

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
                            row.AddCell("Rate (%)");
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
                                row.AddCell(item.RatePercent.ToString("N"));
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
                    sb.AppendLine("<p style='margin:0;font-size:14px;mso-line-height-alt:21px'><span style='font-size:14px;'><strong>Fund Outflow - Money Market</strong></span></p><br/>");

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
                            row.AddCell("Purchase Rate/ Yield (%)");
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

        private static string TsTable(int formId)
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

        private static string TsTable(List<ISSD_TradeSettlement> tradeItems, string tradeType)
        {
            using (var db = new kashflowDBEntities())
            {
                var bodyBuilder = new StringBuilder();

                var sb = new StringBuilder();

                if (tradeType == TsItemCategory.Equity)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Equity);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

                if (tradeType == TsItemCategory.Bond)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Bond);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Bond Type");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

                if (tradeType == TsItemCategory.Coupon)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Coupon);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Coupon Type");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

                if (tradeType == TsItemCategory.Cp)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Cp);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

                if (tradeType == TsItemCategory.NotesPapers)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.NotesPapers);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("Maturity (+)");
                            row.AddCell("Sales (+)");
                            row.AddCell("Purchase (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

                if (tradeType == TsItemCategory.Repo)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Repo);
                            row.AddCell("Stock Code/ ISIN");
                            row.AddCell("First Leg (+)");
                            row.AddCell("Second Leg (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

                if (tradeType == TsItemCategory.Mtm)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Mtm);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tradeType == TsItemCategory.Fx)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Fx);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tradeType == TsItemCategory.Altid)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Altid);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tradeType == TsItemCategory.Fees)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Fees);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.AmountMinus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tradeType == TsItemCategory.Cn)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Cn);
                            row.AddCell("Amount (+)");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(item.InstrumentCode);
                                row.AddCell(item.AmountPlus.ToString("N"));
                                row.AddCell(item.Remarks);
                            }

                            counter++;
                        }
                    }
                }

                if (tradeType == TsItemCategory.Others)
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell(Common.TsItemCategory.Others);
                            row.AddCell("Amount (+)");
                            row.AddCell("Amount (-)");
                            row.AddCell("Type");
                            row.AddCell("Remarks");
                        }

                        var counter = 1;

                        foreach (var item in tradeItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
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

        private static string InflowFundTable(int formId)
        {
            using (var db = new kashflowDBEntities())
            {
                var theForm = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                var bodyBuilder = new StringBuilder();

                var sb = new StringBuilder();

                var formItems = db.AMSD_IF_Item
                    .Where(x => x.FormId == formId)
                    .ToList();

                if (formItems.Any())
                {
                    using (var table = new Common.Table(sb))
                    {
                        using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                        {
                            row.AddCell("No");
                            row.AddCell("Fund Type");
                            row.AddCell("Bank");
                            row.AddCell("Amount");
                        }

                        var counter = 1;

                        foreach (var item in formItems)
                        {
                            using (var row = table.AddRow())
                            {
                                row.AddCell(counter.ToString());
                                row.AddCell(item.FundType);
                                row.AddCell(item.Bank);
                                row.AddCell(item.Amount.ToString("N"));
                            }

                            counter++;
                        }
                    }
                }

                bodyBuilder.Append(sb);

                return bodyBuilder.ToString();
            }
        }

        private static string FcaTaggingTable(List<ISSD_TradeSettlement> tradeSettlementItems)
        {
            using (var db = new kashflowDBEntities())
            {
                var Ids = tradeSettlementItems.Select(x => x.FormId).Distinct().ToList();
                var forms = db.ISSD_FormHeader.Where(x => Ids.Contains(x.Id)).ToList();

                if (forms.Any())
                {
                    var fcaTable = new StringBuilder();

                    foreach (var f in forms)
                    {
                        fcaTable.AppendLine("<br/><b>" + f.FormType + " </b><br/>");

                        using (var table = new Common.Table(fcaTable))
                        {
                            using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                            {
                                row.AddCell("Settlement Date");
                                row.AddCell("Currency");
                                row.AddCell("Type");
                                row.AddCell("Code");
                                row.AddCell("Stock Code");

                                if (Common.TsItemMapAmountColumn(tradeSettlementItems.FirstOrDefault(x => x.FormId == f.Id)?.InstrumentType) == 1)
                                {
                                    row.AddCell("Maturity (+)");
                                    row.AddCell("Sales (+)");
                                    row.AddCell("Purchase (-)");
                                }
                                else if (Common.TsItemMapAmountColumn(tradeSettlementItems.FirstOrDefault(x => x.FormId == f.Id)?.InstrumentType) == 2)
                                {
                                    row.AddCell("1st Leg (+)");
                                    row.AddCell("2nd Leg (-)");
                                }
                                else if (Common.TsItemMapAmountColumn(tradeSettlementItems.FirstOrDefault(x => x.FormId == f.Id)?.InstrumentType) == 3)
                                {
                                    row.AddCell("Amount (+)");
                                    row.AddCell("Amount (-)");
                                }
                                else if (Common.TsItemMapAmountColumn(tradeSettlementItems.FirstOrDefault(x => x.FormId == f.Id)?.InstrumentType) == 4)
                                {
                                    row.AddCell("Amount (+)");
                                }

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
                                    row.AddCell(f.Currency);
                                    row.AddCell(ts.InstrumentType);
                                    row.AddCell(ts.InstrumentCode);
                                    row.AddCell(ts.StockCode);

                                    if (Common.TsItemMapAmountColumn(ts.InstrumentType) == 1)
                                    {
                                        row.AddCell(ts.Maturity.ToString("N"));
                                        row.AddCell(ts.Sales.ToString("N"));
                                        row.AddCell(ts.Purchase.ToString("N"));
                                    }
                                    else if (Common.TsItemMapAmountColumn(ts.InstrumentType) == 2)
                                    {
                                        row.AddCell(ts.FirstLeg.ToString("N"));
                                        row.AddCell(ts.SecondLeg.ToString("N"));
                                    }
                                    else if (Common.TsItemMapAmountColumn(ts.InstrumentType) == 3)
                                    {
                                        row.AddCell(ts.AmountPlus.ToString("N"));
                                        row.AddCell(ts.AmountMinus.ToString("N"));
                                    }
                                    else if (Common.TsItemMapAmountColumn(ts.InstrumentType) == 4)
                                    {
                                        row.AddCell(ts.AmountPlus.ToString("N"));
                                    }

                                    row.AddCell(ts.InflowTo);
                                    row.AddCell(ts.OutflowFrom);
                                    row.AddCell(ts.AssignedBy);
                                    row.AddCell(ts.AssignedDate?.ToString("dd/MM/yyyy HH:mm"));
                                }
                            }

                        }
                    }

                    return fcaTable.ToString();
                }
                else
                {
                    return string.Empty;
                }

            }

        }

        #endregion
    }
}
