using System.IO;
using System.Linq;
using xDC.Utils;
using DevExpress.Spreadsheet;
using DevExpress.ClipboardSource.SpreadsheetML;
using System;

namespace xDC.Services.FileGenerator
{
    public class FileGenerator
    {
        public FileStream GenFile(string generatedFileName)
        {
            var tempFolder = Config.TempFolderPath;

            var filePath = Directory.GetFiles(tempFolder, generatedFileName + "*").SingleOrDefault();
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

        public string SaveAndGenDocId(IWorkbook workbook, string filenamePrefix, bool isExportToExcel)
        {
            var randomFileName = filenamePrefix + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (isExportToExcel)
            {
                var documentFormat = DocumentFormat.Xlsx;
                var tempFolder = Common.GetSystemTempFilePath(randomFileName + ".xlsx");
                workbook.SaveDocument(tempFolder, documentFormat);
            }
            else
            {
                var tempFolder = Common.GetSystemTempFilePath(randomFileName + ".pdf");
                workbook.ExportToPdf(tempFolder);
            }

            return randomFileName;
        }
    }
}
