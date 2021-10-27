using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Extension.MailMerge;
using xDC_Web.ViewModels.Fid.DealCutOff;

namespace xDC_Web.Controllers
{
    [Authorize(Roles = "Administrator, Power User, FID, ISSD")]
    [RoutePrefix("DealCutOff")]
    public class DealCutOffController : Controller
    {
        // GET: DealCutOff
        public ActionResult Index()
        {
            return View();
        }

        #region Deal Cut Off (MYR)

        public ActionResult DealCutOffMyr()
        {
            return View("Myr");
        }

        [HttpPost]
        [Route("Myr/Print")]
        public ActionResult DealCutOffMyr_Print(string tradeDate, bool isExportAsExcel, string viewType)
        {
            try
            {
                DateTime? selectedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(tradeDate))
                {
                    selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(tradeDate));
                }
                bool viewApprovedOnly = (viewType.ToLower() == Common.FormStatus.Approved.ToLower());

                var generatedDocumentFile = new DealCutOffForm_MYR().GenerateFile(selectedDate.Value, isExportAsExcel, viewApprovedOnly);

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

        [Route("Myr/Printed/{id}")]
        public ActionResult DealCutOffMyr_ViewPrinted(string id)
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

        public ActionResult DealCutOffMyrPreview(string TradeDate, string viewType, int SheetIndex)
        {
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(TradeDate))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
            }

            bool viewApprovedOnly = (viewType.ToLower() == Common.FormStatus.Approved.ToLower());

            var previewModel = new SpreadsheetPreviewModel
            {
                Workbook = new DealCutOffForm_MYR().GenerateWorkbook(selectedDate, viewApprovedOnly),
                PreviewSheetIndex = SheetIndex
            };
            return GenerateHtmlPreview(previewModel);
        }

        protected ContentResult GenerateHtmlPreview(SpreadsheetPreviewModel previewModel)
        {
            ContentResult result = new ContentResult();
            result.ContentType = "text/html";
            result.Content = GenerateHtmlContentForWorkbook(previewModel);
            return result;
        }

        string GenerateHtmlContentForWorkbook(SpreadsheetPreviewModel previewModel)
        {
            MemoryStream stream = new MemoryStream();
            HtmlContentGenerator htmlGenerator = new HtmlContentGenerator(stream);
            htmlGenerator.Generate(previewModel.Workbook, previewModel.PreviewSheetIndex);
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        #endregion

        #region Deal Cut Off (FCY)

        public ActionResult DealCutOffFcy()
        {
            return View("Fcy");
        }

        [HttpPost]
        [Route("Fcy/Print")]
        public ActionResult DealCutOffFcy_Print(string TradeDate, bool isExportAsExcel)
        {
            try
            {
                DateTime? selectedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(TradeDate))
                {
                    selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
                }

                var generatedDocumentFile = new DealCutOffForm_FCY().GenerateFile(selectedDate.Value, isExportAsExcel);

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

        [Route("Fcy/Printed/{id}")]
        public ActionResult DealCutOffFcy_ViewPrinted(string id)
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

        public ActionResult DealCutOffFcyPreview(string TradeDate, int SheetIndex)
        {
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(TradeDate))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
            }

            var previewModel = new SpreadsheetPreviewModel
            {
                Workbook = new DealCutOffForm_FCY().GenerateWorkbook(selectedDate),
                PreviewSheetIndex = SheetIndex
            };
            return GenerateHtmlPreview(previewModel);
        }


        #endregion
    }
}