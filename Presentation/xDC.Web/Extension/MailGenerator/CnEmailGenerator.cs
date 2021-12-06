using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC_Web.Extension.MailGenerator
{
    public class CnEmailGenerator: BaseGenerator
    {
        public string MailBody(int formId)
        {
            try
            {
                var mailBody = string.Empty;

                using (var db = new kashflowDBEntities())
                {
                    var cnForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (cnForm != null)
                    {
                        var cnItems = db.ISSD_TradeSettlement.Where(x => x.FormId == cnForm.Id);

                        var pageUrl = string.Format("{0}" + Common.Email_FormUrlMap(cnForm.FormType) + "{1}", Config.EmailApplicationUrl, formId);

                        var bodyBuilder = new StringBuilder();
                        bodyBuilder.Append("<p>Dear All, </p>");
                        bodyBuilder.AppendLine(string.Format("<p>Contribution Credited item in  <a href='" + pageUrl +
                                                             "'>#" + cnForm.Id + "</a> form has been " +
                                                             cnForm.FormStatus + " by " + cnForm.ApprovedBy));

                        using (var table = new Common.Table(bodyBuilder))
                        {
                            using (var row = table.AddHeaderRow("#5B8EFB", "white"))
                            {
                                row.AddCell("#");
                                row.AddCell("Contribution Credited");
                                row.AddCell("Amount (+)");
                                row.AddCell("Remarks");
                            }

                            var count = 0;
                            foreach (var item in cnItems)
                            {
                                count += 1;

                                using (var row = table.AddRow())
                                {
                                    row.AddCell(count.ToString());
                                    row.AddCell(item.InstrumentCode);
                                    row.AddCell(item.AmountPlus.ToString("N"));
                                    row.AddCell(item.Remarks);
                                }
                            }
                        }
                        
                        mailBody = bodyBuilder.ToString();
                        return mailBody;
                    }
                    else
                    {
                        Logger.LogError("CN Form not found. ID:" + formId);
                        return mailBody;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return string.Empty;
            }
        }

        public string GenerateMailFile(int formId, string senderEmail)
        {
            try
            {
                var fileName = "TradeSettlement_CN_TRX_" + DateTime.Now.ToString("yyyyMMddhhss");
                var mailMessage = new MailMessage();

                var recipientsConfig = Config.NotificationTsCnEmail;
                var recipients = recipientsConfig.Split(';').ToList();
                foreach (var recipient in recipients.Where(recipient => !string.IsNullOrEmpty(recipient)))
                {
                    mailMessage.To.Add(recipient);
                }

                mailMessage.From = new MailAddress(senderEmail);
                mailMessage.Headers.Add("X-Unsent", "1");
                mailMessage.Subject = Config.NotificationTsCnEmailSubject;

                mailMessage.Body = MailBody(formId);
                mailMessage.IsBodyHtml = true;

                SaveEmlFile(fileName, mailMessage);

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }


    }
}