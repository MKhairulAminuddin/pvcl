using System;
using System.IO;
using System.Web.Mvc;
using xDC.Logging;
using xDC_Web.Extension.MailMerge;
using xDC_Web.ViewModels.Fid.DealCutOff;

namespace xDC_Web.Controllers.Mvc
{
    public class BaseController : Controller
    {
        private readonly IXDcLogger _logger = Startup.Container.GetInstance<IXDcLogger>();

        protected ActionResult CreateFileStreamResult(Stream stream, string contentType, string fileExtension)
        {
            return CreateFileStreamResult(stream, contentType, fileExtension, "Document");
        }
        protected ActionResult CreateFileStreamResult(Stream stream, string contentType, string fileExtension, string fileName)
        {
            stream.Seek(0, SeekOrigin.Begin);
            FileStreamResult result = new FileStreamResult(stream, contentType);
            result.FileDownloadName = Uri.EscapeDataString(string.Format("{0}.{1}", fileName, fileExtension));
            return result;
        }

        public ContentResult GenerateHtmlPreview(SpreadsheetPreviewModel previewModel)
        {
            ContentResult result = new ContentResult();
            result.ContentType = "text/html";
            result.Content = GenerateHtmlContentForWorkbook(previewModel);
            return result;
        }

        private string GenerateHtmlContentForWorkbook(SpreadsheetPreviewModel previewModel)
        {
            MemoryStream stream = new MemoryStream();
            HtmlContentGenerator htmlGenerator = new HtmlContentGenerator(stream);
            htmlGenerator.Generate(previewModel.Workbook, previewModel.PreviewSheetIndex);
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public ViewResult xDcErrorPage(Exception ex, string customErrorMessage = null)
        {
            _logger.LogError(ex);
            TempData["ErrorMessage"] = (!string.IsNullOrEmpty(customErrorMessage)) ? customErrorMessage : ex.Message;
            return View("Error");
        }
    }
}