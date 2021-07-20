using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace xDC_Web.Controllers
{
    public class BaseController : Controller
    {
        protected ActionResult CreateFileStreamResult(Stream stream, string contentType, string fileExtension)
        {
            return CreateFileStreamResult(stream, contentType, fileExtension, "Document");
        }
        protected ActionResult CreateFileStreamResult(Stream stream, string contentType, string fileExtension, string fileName)
        {
            stream.Seek(0, SeekOrigin.Begin);
            FileStreamResult result = new FileStreamResult(stream, contentType);
            result.FileDownloadName = Uri.EscapeDataString(String.Format("{0}.{1}", fileName, fileExtension));
            return result;
        }

        [Authorize]
        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}