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
                sheet["J4"].Value = (dataItem.RentasOb != null) ? dataItem.RentasOb.Value : 0.00;
                sheet["G8"].Value = (dataItem.MmaOb != null) ? dataItem.MmaOb.Value : 0.00;
                sheet["G10"].Value = (dataItem.TotalRhb != null) ? dataItem.TotalRhb.Value : 0.00;

                sheet["E13"].Value = (dataItem.InflowDepoPrincipal != null) ? dataItem.InflowDepoPrincipal.Value : 0.00;
                sheet["E14"].Value = (dataItem.InflowDepoInterest != null) ? dataItem.InflowDepoInterest.Value : 0.00;
                sheet["G15"].Value = (dataItem.InflowTotalDepoPrincipalInterest != null) ? dataItem.InflowTotalDepoPrincipalInterest.Value : 0.00;

                sheet["E23"].Value = (dataItem.InflowEquity != null) ? dataItem.InflowEquity.Value : 0.00;

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

            //1. Opening Balance RENTAS from EDW
            var rentasOb = db.EDW_BankBalance
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "RENTAS")
                .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                .Select(x => x.Sum(y => y.Amount))
                .FirstOrDefault();
            dataObj.RentasOb = rentasOb;

            //2. Opening Balance MMA from EDW
            var mmaOb = db.EDW_BankBalance
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "MMA")
                .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                .Select(x => x.Sum(y => y.Amount))
                .FirstOrDefault();
            dataObj.MmaOb = mmaOb;

            //3. AMSD Inflow Fund RHB
            var amsdApprovedForms = db.AMSD_IF
                .Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate) && x.FormStatus == Common.FormStatus.Approved)
                .Select(x => x.Id)
                .ToList();
            var totalRhb = db.AMSD_IF_Item
                .Where(x => amsdApprovedForms.Contains(x.FormId))
                .GroupBy(x => x.FundType)
                .Select(x => x.Sum(y => y.Amount))
                .FirstOrDefault();
            dataObj.TotalRhb = (double?)totalRhb;

            //4. Inflow Money Market Principal 
            var treasuryApprovedForms = db.FID_Treasury
                .Where(x => DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.FormStatus == Common.FormStatus.Approved)
                .Select(x => x.Id)
                .ToList();

            var inflowTotalDepoPrincipal = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.Principal))
                .FirstOrDefault();

            var inflowTotalDepoInterest = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.IntProfitReceivable))
                .FirstOrDefault();

            var inflowTotalDepoPrincipalInterest = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.PrincipalIntProfitReceivable))
                .FirstOrDefault();

            dataObj.InflowDepoPrincipal = (double?)inflowTotalDepoPrincipal;
            dataObj.InflowDepoInterest = (double?)inflowTotalDepoInterest;
            dataObj.InflowTotalDepoPrincipalInterest = (double?)inflowTotalDepoPrincipalInterest;


            // 5. TS Equity
            var approveTradeSettlementForms = db.ISSD_FormHeader
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.FormStatus == Common.FormStatus.Approved)
                .Select(x => x.Id)
                .ToList();
            var inflowTotalEquity = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && x.InstrumentType == Common.TsItemCategory.Equity)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    AmountPlus = x.Sum(y => (double?)y.AmountMinus),
                    Sales = x.Sum(y => (double?)y.Sales),
                    Maturity = x.Sum(y => (double?)y.Maturity)
                })
                .Select(x => (x.Sales + x.Maturity + x.AmountPlus))
                .FirstOrDefault();
            var outflowTotalEquity = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && x.InstrumentType == Common.TsItemCategory.Equity)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    AmountMinus = x.Sum(y => (double?)y.AmountMinus),
                    Purchase = x.Sum(y => (double?)y.Purchase)
                })
                .Select(x => (x.AmountMinus + x.Purchase))
                .FirstOrDefault();
            dataObj.InflowEquity = inflowTotalEquity;
            dataObj.OutflowEquity = outflowTotalEquity;

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