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

        public static string LoggerFilePathFormat => ConfigurationManager.AppSettings["Logger.FilePathFormat"];
        public static string SmtpServerIp => ConfigurationManager.AppSettings["Smtp.ServerIp"];
        public static string SmtpServerPort => ConfigurationManager.AppSettings["Smtp.ServerPort"];
        public static string SmtpSenderAccount => ConfigurationManager.AppSettings["Smtp.SenderAccount"];
        public static string SmtpSenderAccountName => ConfigurationManager.AppSettings["Smtp.SenderAccountName"];

        public static string EmailApplicationUrl => ConfigurationManager.AppSettings["Email.ApplicationUrl"];

        public static string AclAdministrator = "Administrator";
        public static string AclPowerUser = "Power User";
        public static string AclAmsd = "Amsd";
        public static string AclIisd = "Iisd";
        public static string AclFid = "Fid";
    }
}
