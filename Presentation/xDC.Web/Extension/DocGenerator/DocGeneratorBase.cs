using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using xDC.Logging;
using xDC.Utils;

namespace xDC_Web.Extension.DocGenerator
{
    public class DocGeneratorBase
    {
        public FileStream GetFile(string filename)
        {
            var tempFolder = Config.TempFolderPath;

            var filePath = Directory.GetFiles(tempFolder, filename + "*").SingleOrDefault();
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

        public void SaveExcelFile(string filename, MemoryStream ms)
        {
            try
            {
                var fileExt = ".xlsx";
                var fullFileName = filename + fileExt;

                var tempFolder = Config.TempFolderPath;
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                tempFolder = Path.Combine(tempFolder, fullFileName);

                using (FileStream fs = new FileStream(tempFolder, FileMode.Create))
                {
                    ms.WriteTo(fs);
                    fs.Close();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        protected internal string MapPath(string path)
        {
            return System.Web.HttpContext.Current.Request.MapPath(path);
        }
    }
}