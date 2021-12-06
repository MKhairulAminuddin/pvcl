using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using xDC.Logging;
using xDC.Utils;

namespace xDC_Web.Extension.MailGenerator
{
    public class BaseGenerator
    {
        public FileStream GetFile(string filename)
        {
            var tempFolder = Path.Combine(Config.TempFolderPath, filename);

            // tempFolder should contain 1 eml file
            var filePath = Directory.GetFiles(tempFolder).SingleOrDefault();

            if (!string.IsNullOrEmpty(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Open);
                return fs;
            }
            else
            {
                return null;
            }
        }

        public void SaveEmlFile(string filename, MailMessage message)
        {
            using (var client = new SmtpClient())
            {
                // create a temp folder to hold just this .eml file so that we can find it easily.
                var tempFolder = Path.Combine(Config.TempFolderPath, filename);

                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                client.UseDefaultCredentials = true;
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = tempFolder;
                client.Send(message);
            }
        }
    }
}