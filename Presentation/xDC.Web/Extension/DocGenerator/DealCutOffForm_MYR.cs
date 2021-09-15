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
    public class DealCutOffForm_MYR : DocGeneratorBase
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
                    //get data 

                    //1. Opening Balance RENTAS from EDW
                    var rentasOb = db.EDW_BankBalance
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "RENTAS")
                        .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate})
                        .Select(x => x.Sum(y => y.Amount))
                        .FirstOrDefault();


                    //2. Opening Balance MMA from EDW
                    var mmaOb = db.EDW_BankBalance
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "MMA")
                        .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                        .Select(x => x.Sum(y => y.Amount))
                        .FirstOrDefault();


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


                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath("~/App_Data/Deal Cut Off MYR Template.xltx"));
                    workbook = GenerateDocument(workbook, selectedDate, rentasOb, mmaOb, (double?)totalRhb,
                        (double?)inflowTotalDepoPrincipal, (double?)inflowTotalDepoInterest, (double?)inflowTotalDepoPrincipalInterest);

                    return workbook;

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public string GenerateFile(DateTime? selectedDate, bool isExportAsExcel)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    //get data 

                    //1. Opening Balance RENTAS from EDW
                    var rentasOb = db.EDW_BankBalance
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "RENTAS")
                        .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                        .Select(x => x.Sum(y => y.Amount))
                        .FirstOrDefault();


                    //2. Opening Balance MMA from EDW
                    var mmaOb = db.EDW_BankBalance
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "MMA")
                        .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                        .Select(x => x.Sum(y => y.Amount))
                        .FirstOrDefault();


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


                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath("~/App_Data/Deal Cut Off MYR Template.xltx"));
                    workbook = GenerateDocument(workbook, selectedDate, rentasOb, mmaOb, (double?)totalRhb,
                        (double?)inflowTotalDepoPrincipal, (double?)inflowTotalDepoInterest, (double?)inflowTotalDepoPrincipalInterest);
                    var randomFileName = "FID Deal Cut Off MYR wei - " + DateTime.Now.ToString("yyyyMMddHHmmss");

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
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        private IWorkbook GenerateDocument(IWorkbook workbook, DateTime? selectedDate, double? rentasOb, double? mmaOb, double? totalRhb,
            double? inflowDepoPrincipal, double? inflowDepoInterest, double? inflowTotalDepoPrincipalInterest)
        {
            workbook.BeginUpdate();
            try
            {
                var sheet = workbook.Worksheets[0];

                // 0. Date
                sheet["J2"].Value = (selectedDate != null) ? selectedDate.Value.ToString("dd/MM/yyyy") : null;

                // 1. Rentas OB
                sheet["J4"].Value = (rentasOb != null) ? rentasOb.Value : 0.00;
                sheet["G8"].Value = (mmaOb != null) ? mmaOb.Value : 0.00;
                sheet["G10"].Value = (totalRhb != null) ? totalRhb.Value : 0.00;

                sheet["E13"].Value = (inflowDepoPrincipal != null) ? inflowDepoPrincipal.Value : 0.00;
                sheet["E14"].Value = (inflowDepoInterest != null) ? inflowDepoInterest.Value : 0.00;
                sheet["G15"].Value = (inflowTotalDepoPrincipalInterest != null) ? inflowTotalDepoPrincipalInterest.Value : 0.00;

                workbook.Calculate();
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }
    }
}