using System.IO;
using System.Linq;
using xDC.Utils;
using DevExpress.Spreadsheet;
using DevExpress.ClipboardSource.SpreadsheetML;
using System;
using xDC.Logging;
using xDC.Domain.Web.Application;

namespace xDC.Services.FileGenerator
{
    public class FileGenerator
    {
        private readonly IXDcLogger _logger;

        public FileGenerator(IXDcLogger logger)
        {
            _logger = logger;
        }

        public ExportedFile GenFile(string generatedFileName)
        {
            try
            {
                var outputFile = new ExportedFile();
                var tempFolder = Config.TempFolderPath;
                generatedFileName = generatedFileName.Replace("\"", "");

                var filePath = Directory.GetFiles(tempFolder, generatedFileName + "*").SingleOrDefault();
                if (!string.IsNullOrEmpty(filePath))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        using (var stream = new MemoryStream())
                        {
                            fileStream.CopyTo(stream);
                            outputFile.FileBytes = stream.ToArray();
                        }
                        outputFile.FileName = Common.GetFileName(fileStream);
                        outputFile.FileExt = Common.GetFileExt(fileStream);
                    }

                    return outputFile;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
