using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.DocGenerator;
using xDC_Web.Extension.MailMerge;
using xDC_Web.ViewModels.Fid.DealCutOff;

namespace xDC_Web.Controllers
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.Report)]
    [RoutePrefix("Report")]
    public class ReportController : Controller
    {
        // GET: DealCutOff
        public ActionResult Index()
        {
            return View();
        }

        #region Deal Cut Off (MYR)

        [Route("DealCutOffMyr")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_MYR)]
        public ActionResult DealCutOffMyr()
        {
            return View();
        }

        [HttpPost]
        [Route("DealCutOffMyr/Print")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_MYR)]
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

        [Route("DealCutOffMyr/Printed/{id}")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_MYR)]
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

        [Route("DealCutOffMyrPreview")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_MYR)]
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

        #endregion

        #region Deal Cut Off (FCY)

        [Route("DealCutOffFcy")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_FCY)]
        public ActionResult DealCutOffFcy()
        {
            return View();
        }

        [HttpPost]
        [Route("DealCutOffFcy/Print")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_FCY)]
        public ActionResult DealCutOffFcy_Print(string TradeDate, bool isExportAsExcel, string viewType)
        {
            try
            {
                DateTime? selectedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(TradeDate))
                {
                    selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
                }
                bool viewApprovedOnly = (viewType.ToLower() == Common.FormStatus.Approved.ToLower());

                var generatedDocumentFile = new DealCutOffForm_FCY().GenerateFile(selectedDate.Value, isExportAsExcel, viewApprovedOnly);

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

        [Route("DealCutOffFcy/Printed/{id}")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_FCY)]
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

        [Route("DealCutOffFcyPreview")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_FCY)]
        public ActionResult DealCutOffFcyPreview(string tradeDate, string viewType, int SheetIndex)
        {
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(tradeDate))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(tradeDate));
            }
            bool viewApprovedOnly = (viewType.ToLower() == Common.FormStatus.Approved.ToLower());

            var previewModel = new SpreadsheetPreviewModel
            {
                Workbook = new DealCutOffForm_FCY().GenerateWorkbook(selectedDate, viewApprovedOnly),
                PreviewSheetIndex = SheetIndex
            };
            return GenerateHtmlPreview(previewModel);
        }


        #endregion

        #region Private Methods

        private ContentResult GenerateHtmlPreview(SpreadsheetPreviewModel previewModel)
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

        #endregion

        #region 10 AM Deal Cut Off

        [Route("TenAmDealCutOff")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_10am)]
        public ActionResult TenAmDealCutOff()
        {
            return View();
        }

        [HttpPost]
        [Route("TenAmDealCutOff/Print")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_10am)]
        public ActionResult TenAmDealCutOff_Print(string selectedDateEpoch, bool isExportAsExcel, string viewType)
        {
            try
            {
                DateTime? selectedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(selectedDateEpoch))
                {
                    selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(selectedDateEpoch));
                }

                bool viewApprovedOnly = (viewType.ToLower() == Common.FormStatus.Approved.ToLower());

                var generatedDocumentFile = new TenAmDealCutOffDoc().GenerateFile(selectedDate.Value, isExportAsExcel, viewApprovedOnly);

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
        [KflowAuthorize(Common.PermissionKey.Report_DCO_10am)]
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

        #endregion
    }
}