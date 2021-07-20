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

        protected internal string MapPath(string path)
        {
            return System.Web.HttpContext.Current.Request.MapPath(path);
        }
        
    }
}