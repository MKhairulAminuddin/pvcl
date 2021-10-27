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
