using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Utils
{
    public static class Config
    {
        public static string DbCon => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static string TempFolderPath => ConfigurationManager.AppSettings["TempFolder.Path"];
        public static string LoggerFilePathFormat => ConfigurationManager.AppSettings["Logger.FilePathFormat"];
        public static string SmtpServerIp => ConfigurationManager.AppSettings["Smtp.ServerIp"];
        public static string SmtpServerPort => ConfigurationManager.AppSettings["Smtp.ServerPort"];
        public static string SmtpSenderAccount => ConfigurationManager.AppSettings["Smtp.SenderAccount"];
        public static string SmtpSenderAccountName => ConfigurationManager.AppSettings["Smtp.SenderAccountName"];

        public static string EmailApplicationUrl => ConfigurationManager.AppSettings["Email.ApplicationUrl"];
        public static bool IsLive => Convert.ToBoolean(ConfigurationManager.AppSettings["Environment.IsLive"]);

        public static string FormViewAllSubmittedData => ConfigurationManager.AppSettings["Form.DealCutOff.ViewAllSubmittedFormText"];


        public static string NotificationTsCnEmail => ConfigurationManager.AppSettings["Notification.TS.CnEmail"];
        public static string NotificationTsCnEmailCc => ConfigurationManager.AppSettings["Notification.TS.CnEmail.CC"];
        public static string NotificationTsPeEmail => ConfigurationManager.AppSettings["Notification.TS.PeEmail"];
        public static string NotificationTsPeEmailCc => ConfigurationManager.AppSettings["Notification.TS.PeEmail.CC"];
        public static string NotificationTsPropertyEmail => ConfigurationManager.AppSettings["Notification.TS.PropertyEmail"];
        public static string NotificationTsPropertyEmailCc => ConfigurationManager.AppSettings["Notification.TS.PropertyEmail.CC"];
        public static string NotificationTsLoanEmail => ConfigurationManager.AppSettings["Notification.TS.LoanEmail"];
        public static string NotificationTsLoanEmailCc => ConfigurationManager.AppSettings["Notification.TS.LoanEmail.CC"];
        public static string NotiTreasuryIssdEmail=> ConfigurationManager.AppSettings["Notification.T.IssdEmail"];


        public static string NotificationTsCnEmailSubject => ConfigurationManager.AppSettings["Notification.TS.CnEmailSubject"];
        public static string NotificationTsPeEmailSubject => ConfigurationManager.AppSettings["Notification.TS.PeEmailSubject"];
        public static string NotificationTsPropertyEmailSubject => ConfigurationManager.AppSettings["Notification.TS.PropertySubject"];
        public static string NotificationTsLoanEmailSubject => ConfigurationManager.AppSettings["Notification.TS.LoanSubject"];

        public static class Acl
        {
            public const  string Administrator = "Administrator";
            public const string PowerUser = "Power User";
            public const string Amsd = "AMSD";
            public const string Issd = "ISSD";
            public const string Fid = "FID";
        }
    }
}
