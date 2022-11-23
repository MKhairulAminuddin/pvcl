using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using static xDC.Utils.Common;
using xDC.Utils;

namespace xDC.Services.Notification
{
    public class EmailNotification : IEmailNotification
    {
        #region Fields

        private string SubjectAppend => !Config.IsLive ? "[UAT - Kashflow]" : "[Kashflow]";

        #endregion

        public void ApprovalSubmission(int formId, string formType, string approvedBy, string notes)
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

        public void ApprovalResponse(int formId, string formType, string formStatus, string formPreparer, string notes)
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


        private void SendEmail(MimeMessage message)
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

        private bool EnableNotification(string configKey)
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

        private List<MailboxAddress> ReceipientsFromConfig(string configKey)
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

        private MimeMessage ComposeSubmitForApprovalMail(int formId, string formType, string approverName, string approverMail, string notes)
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

                        var isNotifyCcEnabled = EnableNotification(EmailNotiKey.Enable_FID_T_Submitted_Cc);
                        if (isNotifyCcEnabled)
                        {
                            message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.FID_T_Submitted_Cc));
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
        private MimeMessage ComposeApprovalFeedbackMail(int formId, string formType, string formStatus, string preparerName, string preparerMail, string notes)
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

                        if (formStatus == FormStatus.Approved)
                        {
                            var isNotifyCcEnabled = EnableNotification(EmailNotiKey.Enable_FID_T_Approved_Cc);
                            if (isNotifyCcEnabled)
                            {
                                message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.FID_T_Approved_Cc));
                            }
                        }
                    }
                    else if (Common.IsTsFormType(formType))
                    {
                        StrContent = StrContent.Replace("[DataTable]", TsTable(formId));

                        if (formStatus == FormStatus.Approved)
                        {
                            var enableFidCc = EnableNotification(EmailNotiKey.Enable_FID_TS_Approved_Cc);
                            if (enableFidCc)
                            {
                                message.Cc.AddRange(ReceipientsFromConfig(EmailNotiKey.FID_TS_Approved_Cc));
                            }
                        }
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

        private string TsOthersTable(int formId, string othersType)
        {
            using (var db = new kashflowDBEntities())
            {
                var theForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                var tsTradeItems = db.ISSD_TradeSettlement
                    .Where(x => x.FormId == formId && x.InstrumentType == TsItemCategory.Others)
                    .ToList();

                if (tsTradeItems.Any(x => x.OthersType == othersType))
                {
                    var bodyBuilder = new StringBuilder();
                    var sb = new StringBuilder();

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

                        foreach (var item in tsTradeItems.Where(x => x.OthersType == othersType))
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

                    bodyBuilder.Append(sb);

                    return bodyBuilder.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        private string InflowFundTable(int formId)
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

    }
}
