using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using DevExpress.Spreadsheet;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;

namespace xDC_Web.Extension.DocGenerator
{
    public class DealCutOffForm_FCY : DocGeneratorBase
    {
        private Color _tableHeaderPrimaryColor = System.Drawing.ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = System.Drawing.ColorTranslator.FromHtml("#3498DB");
        private Color _outFlowColor = System.Drawing.ColorTranslator.FromHtml("#E67E22");

        public IWorkbook GenerateWorkbook(DateTime? selectedDate)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var dataObj = ConstructData(db, selectedDate);

                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_DealCutOff_FCY));
                    workbook = GenerateDocument(workbook, dataObj);

                    return workbook;

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public string GenerateFile(DateTime selectedDate, bool isExportAsExcel)
        {
            try
            {
                IWorkbook workbook = GenerateWorkbook(selectedDate);
                var randomFileName = Common.DownloadedFileName.FID_DealCutOff_FCY + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (isExportAsExcel)
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
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        private IWorkbook GenerateDocument(IWorkbook workbook, MYR_DealCutOffData dataItem)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                // 0. Date
                sheet["J2"].Value = (dataItem.SelectedDate != null) ? dataItem.SelectedDate.Value.ToString("dd/MM/yyyy") : null;

                // 1. Rentas OB
                sheet["J4"].Value = dataItem.RentasOb;
                sheet["G8"].Value = dataItem.MmaOb;
                sheet["G10"].Value = dataItem.TotalRhb;

                sheet["E13"].Value =  dataItem.InflowDepoPrincipal;
                sheet["E14"].Value = dataItem.InflowDepoInterest;
                sheet["G15"].Value = dataItem.InflowTotalDepoPrincipalInterest;
                
                workbook.Calculate();
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

        private MYR_DealCutOffData ConstructData(kashflowDBEntities db, DateTime? selectedDate)
        {
            //get data 
            var dataObj = new MYR_DealCutOffData()
            {
                SelectedDate = selectedDate
            };
            

            return dataObj;
        }
    }

    public class FCY_DealCutOffData
    {
        public DateTime? SelectedDate { get; set; }
        public double? RentasOb { get; set; }
        public double? MmaOb { get; set; }
        public double? TotalRhb { get; set; }
        public double? InflowDepoPrincipal { get; set; }
        public double? InflowDepoInterest { get; set; }
        public double? InflowTotalDepoPrincipalInterest { get; set; }
        public double? InflowEquity { get; set; }
        public double? OutflowEquity { get; set; }

    }
}