using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Infrastructure.Application;

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

        public bool IsMeApprover(kashflowDBEntities db)
        {
            var getApprover = db.Config_Approver.Where(x => x.Username == User.Identity.Name);
            var isMeApprover = getApprover.Any();
            return isMeApprover;
        }
    }
}