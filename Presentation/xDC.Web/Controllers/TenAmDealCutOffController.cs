using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Extension.DocGenerator;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, FID, ISSD")]
    public class TenAmDealCutOffController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [Route("TenAmDealCutOff/Print")]
        public ActionResult TenAmDealCutOff_Print(string selectedDateEpoch, bool isExportAsExcel)
        {
            try
            {
                DateTime? selectedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(selectedDateEpoch))
                {
                    selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(selectedDateEpoch));
                }

                var generatedDocumentFile = new TenAmDealCutOffDoc().GenerateFile(selectedDate.Value, isExportAsExcel);

                if (!string.IsNullOrEmpty(generatedDocumentFile))
                {
                    return Content(generatedDocumentFile);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return HttpNotFound();
            }
        }

        [Route("TenAmDealCutOff/Printed/{id}")]
        public ActionResult TenAmDealCutOff_ViewPrinted(string id)
        {
            try
            {
                var fileStream = new DocGeneratorBase().GetFile(id);

                if (fileStream != null)
                {
                    var fileName = Common.GetFileName(fileStream);
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

                    if (Common.GetFileExt(fileStream) == ".xlsx")
                    {
                        return File(fileStream, Common.ConvertIndexToContentType(4));
                    }
                    else
                    {
                        return File(fileStream, Common.ConvertIndexToContentType(11));
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "Generated file not found... sorry...";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                TempData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }
    }
}