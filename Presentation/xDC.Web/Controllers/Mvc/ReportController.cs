﻿using System;
using System.IO;
using System.Web.Mvc;
using xDC.Services.FileGenerator;
using xDC.Services.Membership;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.MailMerge;
using xDC_Web.ViewModels.Fid.DealCutOff;

namespace xDC_Web.Controllers.Mvc
{
    [Authorize]
    [KflowAuthorize(Common.PermissionKey.Report)]
    [RoutePrefix("Report")]
    public class ReportController : BaseController
    {
        #region Fields

        private readonly IGenFile_DealCutOffMyrReport _genDcoMyrReport = Startup.Container.GetInstance<IGenFile_DealCutOffMyrReport>();
        private readonly IGenFile_DealCutOffFcyReport _genDcoFcyReport = Startup.Container.GetInstance<IGenFile_DealCutOffFcyReport>();
        private readonly IGenFile_10amDealCutOffReport _gen10amDcoReport = Startup.Container.GetInstance<IGenFile_10amDealCutOffReport>();

        #endregion


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
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(tradeDate))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(tradeDate));
            }
            bool viewApprovedOnly = viewType.ToLower() == Common.FormStatus.Approved.ToLower();

            var generatedDocumentFile = _genDcoMyrReport.GenerateFile(selectedDate.Value, isExportAsExcel, viewApprovedOnly);

            if (!string.IsNullOrEmpty(generatedDocumentFile))
            {
                return Content(generatedDocumentFile);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Route("DealCutOffMyr/Printed/{id}")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_MYR)]
        public ActionResult DealCutOffMyr_ViewPrinted(string id)
        {
            var outputFile = _genDcoMyrReport.GenFile(id);

            if (outputFile != null)
            {
                Response.AddHeader("Content-Disposition", "attachment; filename=" + outputFile.FileName);

                if (outputFile.FileExt == ".xlsx")
                {
                    return File(outputFile.FileBytes, Common.ConvertIndexToContentType(4));
                }
                else
                {
                    return File(outputFile.FileBytes, Common.ConvertIndexToContentType(11));
                }

            }
            else
            {
                TempData["ErrorMessage"] = "Generated file not found... sorry...";
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

            bool viewApprovedOnly = viewType.ToLower() == Common.FormStatus.Approved.ToLower();

            var previewModel = new SpreadsheetPreviewModel
            {
                Workbook = _genDcoMyrReport.GenerateWorkbook(selectedDate, viewApprovedOnly),
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
            
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(TradeDate))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(TradeDate));
            }
            bool viewApprovedOnly = viewType.ToLower() == Common.FormStatus.Approved.ToLower();

            var generatedDocumentFile = _genDcoFcyReport.GenerateFile(selectedDate.Value, isExportAsExcel, viewApprovedOnly);

            if (!string.IsNullOrEmpty(generatedDocumentFile))
            {
                return Content(generatedDocumentFile);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Route("DealCutOffFcy/Printed/{id}")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_FCY)]
        public ActionResult DealCutOffFcy_ViewPrinted(string id)
        {
            var outputFile = _genDcoFcyReport.GenFile(id);

            if (outputFile != null)
            {
                Response.AddHeader("Content-Disposition", "attachment; filename=" + outputFile.FileName);

                if (outputFile.FileExt == ".xlsx")
                {
                    return File(outputFile.FileBytes, Common.ConvertIndexToContentType(4));
                }
                else
                {
                    return File(outputFile.FileBytes, Common.ConvertIndexToContentType(11));
                }

            }
            else
            {
                TempData["ErrorMessage"] = "Generated file not found... sorry...";
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
            bool viewApprovedOnly = viewType.ToLower() == Common.FormStatus.Approved.ToLower();

            var previewModel = new SpreadsheetPreviewModel
            {
                Workbook = _genDcoFcyReport.GenerateWorkbook(selectedDate, viewApprovedOnly),
                PreviewSheetIndex = SheetIndex
            };
            return GenerateHtmlPreview(previewModel);
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
            DateTime? selectedDate = DateTime.Now;
            if (!string.IsNullOrEmpty(selectedDateEpoch))
            {
                selectedDate = Common.ConvertEpochToDateTime(Convert.ToInt64(selectedDateEpoch));
            }

            bool viewApprovedOnly = viewType.ToLower() == Common.FormStatus.Approved.ToLower();

            var generatedDocumentFile = _gen10amDcoReport.GenerateFile(selectedDate.Value, isExportAsExcel, viewApprovedOnly);

            if (!string.IsNullOrEmpty(generatedDocumentFile))
            {
                return Content(generatedDocumentFile);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Route("TenAmDealCutOff/Printed/{id}")]
        [KflowAuthorize(Common.PermissionKey.Report_DCO_10am)]
        public ActionResult TenAmDealCutOff_ViewPrinted(string id)
        {
            var outputFile = _gen10amDcoReport.GenFile(id);

            if (outputFile != null)
            {
                Response.AddHeader("Content-Disposition", "attachment; filename=" + outputFile.FileName);

                if (outputFile.FileExt == ".xlsx")
                {
                    return File(outputFile.FileBytes, Common.ConvertIndexToContentType(4));
                }
                else
                {
                    return File(outputFile.FileBytes, Common.ConvertIndexToContentType(11));
                }

            }
            else
            {
                TempData["ErrorMessage"] = "Generated file not found... sorry...";
                return View("Error");
            }
        }

        #endregion
    }
}