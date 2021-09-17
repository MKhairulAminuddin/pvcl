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

                    var dataObj = ConstructData(db, selectedDate);

                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_DealCutOff_MYR));
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
                var randomFileName = Common.DownloadedFileName.FID_DealCutOff_MYR + DateTime.Now.ToString("yyyyMMddHHmmss");

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

                sheet["E18"].Value = (dataItem.InflowOthersFixedIncome != null) ? dataItem.InflowOthersFixedIncome.Value : 0.00;

                var currentRowIndex = 19;

                if (dataItem.InflowMgs.Any())
                {
                    foreach (var item in dataItem.InflowMgs)
                    {
                        if (item.Amount > 0)
                        {
                            sheet.Rows[currentRowIndex].Insert();

                            sheet["B" + currentRowIndex].Borders.LeftBorder.Color = Color.Black;
                            sheet["B" + currentRowIndex].Borders.LeftBorder.LineStyle = BorderLineStyle.Thin;

                            sheet["C" + currentRowIndex].Value = item.Name;

                            sheet["E" + currentRowIndex].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                            sheet["E" + currentRowIndex].Value = item.Amount;
                            sheet["E" + currentRowIndex].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                            sheet["H" + currentRowIndex].Borders.RightBorder.Color = Color.Black;
                            sheet["H" + currentRowIndex].Borders.RightBorder.LineStyle = BorderLineStyle.Thin;

                            currentRowIndex++;
                        }
                    }
                }

                if (dataItem.InflowGii.Any())
                {
                    foreach (var item in dataItem.InflowGii)
                    {
                        if (item.Amount > 0)
                        {
                            sheet.Rows[currentRowIndex].Insert();

                            sheet["B" + currentRowIndex].Borders.LeftBorder.Color = Color.Black;
                            sheet["B" + currentRowIndex].Borders.LeftBorder.LineStyle = BorderLineStyle.Thin;

                            sheet["C" + currentRowIndex].Value = item.Name;

                            sheet["E" + currentRowIndex].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                            sheet["E" + currentRowIndex].Value = item.Amount;
                            sheet["E" + currentRowIndex].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                            sheet["H" + currentRowIndex].Borders.RightBorder.Color = Color.Black;
                            sheet["H" + currentRowIndex].Borders.RightBorder.LineStyle = BorderLineStyle.Thin;

                            currentRowIndex++;
                        }
                    }
                }

                // total inflow fixed income
                sheet["G" + currentRowIndex].Value = dataItem.InflowTotalFixedIncome;
                sheet["G" + currentRowIndex].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";


                currentRowIndex += 3;
                sheet["E"+ currentRowIndex++].Value = (dataItem.InflowEquity != null) ? dataItem.InflowEquity.Value : 0.00;
                sheet["G"+ currentRowIndex++].Value = (dataItem.InflowEquity != null) ? dataItem.InflowEquity.Value : 0.00;

                currentRowIndex += 1;
                sheet["G" + currentRowIndex].Value = (dataItem.InflowTotalNet != null) ? dataItem.InflowTotalNet.Value : 0.00;
                sheet["J" + currentRowIndex].Value = (dataItem.InflowTotalNetWithOb != null) ? dataItem.InflowTotalNetWithOb.Value : 0.00;

                currentRowIndex += 1;
                sheet["E"+ currentRowIndex].Value = (dataItem.OutflowOthersFixedIncome != null) ? dataItem.OutflowOthersFixedIncome.Value : 0.00;

                // fixed income outflow
                currentRowIndex += 16;

                if (dataItem.OutflowMgs.Any())
                {
                    foreach (var item in dataItem.OutflowMgs)
                    {
                        if (item.Amount > 0)
                        {
                            sheet.Rows[currentRowIndex].Insert();

                            sheet["B" + currentRowIndex].Borders.LeftBorder.Color = Color.Black;
                            sheet["B" + currentRowIndex].Borders.LeftBorder.LineStyle = BorderLineStyle.Thin;

                            sheet["C" + currentRowIndex].Value = item.Name;

                            sheet["E" + currentRowIndex].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                            sheet["E" + currentRowIndex].Value = item.Amount;
                            sheet["E" + currentRowIndex].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                            sheet["H" + currentRowIndex].Borders.RightBorder.Color = Color.Black;
                            sheet["H" + currentRowIndex].Borders.RightBorder.LineStyle = BorderLineStyle.Thin;

                            currentRowIndex++;
                        }
                    }
                }

                if (dataItem.OutflowGii.Any())
                {
                    foreach (var item in dataItem.OutflowGii)
                    {
                        if (item.Amount > 0)
                        {
                            sheet.Rows[currentRowIndex].Insert();

                            sheet["B" + currentRowIndex].Borders.LeftBorder.Color = Color.Black;
                            sheet["B" + currentRowIndex].Borders.LeftBorder.LineStyle = BorderLineStyle.Thin;

                            sheet["C" + currentRowIndex].Value = item.Name;

                            sheet["E" + currentRowIndex].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                            sheet["E" + currentRowIndex].Value = item.Amount;
                            sheet["E" + currentRowIndex].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

                            sheet["H" + currentRowIndex].Borders.RightBorder.Color = Color.Black;
                            sheet["H" + currentRowIndex].Borders.RightBorder.LineStyle = BorderLineStyle.Thin;

                            currentRowIndex++;
                        }
                    }
                }

                // total outflow fixed income
                sheet["B" + currentRowIndex].Borders.LeftBorder.Color = Color.Black;
                sheet["B" + currentRowIndex].Borders.LeftBorder.LineStyle = BorderLineStyle.Thin;
                sheet["G" + currentRowIndex].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                sheet["G" + currentRowIndex].Value = dataItem.OutflowTotalFixedIncome;
                sheet["G" + currentRowIndex].NumberFormat = "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";
                sheet["H" + currentRowIndex].Borders.RightBorder.Color = Color.Black;
                sheet["H" + currentRowIndex].Borders.RightBorder.LineStyle = BorderLineStyle.Thin;


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
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) 
                            && x.Currency == "MYR" && x.FormStatus == Common.FormStatus.Approved)
                .Select(x => x.Id)
                .ToList();

            var inflowTotalEquity = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && x.InstrumentType == Common.TsItemCategory.Equity)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountPlus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Sales = x.Sum(y => (double?)y.Sales) ?? 0,
                    Maturity = x.Sum(y => (double?)y.Maturity) ?? 0
                })
                .Select(x => (x.Sales + x.Maturity + x.AmountPlus))
                .FirstOrDefault();

            var outflowTotalEquity = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && x.InstrumentType == Common.TsItemCategory.Equity)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountMinus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Purchase = x.Sum(y => (double?)y.Purchase) ?? 0
                })
                .Select(x => (x.AmountMinus + x.Purchase))
                .FirstOrDefault();

            dataObj.InflowEquity = inflowTotalEquity;
            dataObj.OutflowEquity = outflowTotalEquity;

            // 6. Fix Income Others
            var excludeFromOthers = new List<string>()
            {
                Common.TsItemCategory.Equity,
                Common.TsItemCategory.Bond,
                Common.TsItemCategory.Coupon
            };

            var inflowOthersFixIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && !excludeFromOthers.Contains(x.InstrumentType))
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountPlus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Sales = x.Sum(y => (double?)y.Sales) ?? 0,
                    Maturity = x.Sum(y => (double?)y.Maturity) ?? 0,
                    FirstLeg = x.Sum(y => (double?)y.FirstLeg) ?? 0
                })
                .Select(x => (x.Sales + x.Maturity + x.AmountPlus + x.FirstLeg))
                .FirstOrDefault();

            var outflowOthersFixIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && !excludeFromOthers.Contains(x.InstrumentType))
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountMinus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Purchase = x.Sum(y => (double?)y.Purchase) ?? 0,
                    SecondLeg = x.Sum(y => (double?)y.SecondLeg) ?? 0
                })
                .Select(x => (x.AmountMinus + x.Purchase + x.SecondLeg))
                .FirstOrDefault();

            dataObj.InflowOthersFixedIncome = inflowOthersFixIncome;
            dataObj.OutflowOthersFixedIncome = outflowOthersFixIncome;

            // 7. Mgs
            var inflowMgsFixedIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) 
                            && (x.InstrumentType == Common.TsItemCategory.Bond || x.InstrumentType == Common.TsItemCategory.Coupon)
                            && x.InstrumentCode.StartsWith("MGS"))
                .GroupBy(x => x.InstrumentCode)
                .Select(x => new
                {
                    Name = x.Key,
                    AmountPlus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Sales = x.Sum(y => (double?)y.Sales) ?? 0,
                    Maturity = x.Sum(y => (double?)y.Maturity) ?? 0,
                    FirstLeg = x.Sum(y => (double?)y.FirstLeg) ?? 0
                })
                .Select(x => new MYR_DealCutOffData_Item()
                {
                    Name = x.Name,
                    Amount = (x.AmountPlus + x.Sales + x.Maturity + x.FirstLeg)
                })
                .ToList();

            var outflowMgsFixedIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Bond || x.InstrumentType == Common.TsItemCategory.Coupon)
                            && x.InstrumentCode.StartsWith("MGS"))
                .GroupBy(x => x.InstrumentCode)
                .Select(x => new
                {
                    Name = x.Key,
                    AmountMinus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Purchase = x.Sum(y => (double?)y.Purchase) ?? 0,
                    SecondLeg = x.Sum(y => (double?)y.SecondLeg) ?? 0
                })
                .Select(x => new MYR_DealCutOffData_Item()
                {
                    Name = x.Name,
                    Amount = (x.AmountMinus + x.Purchase + x.SecondLeg)
                })
                .ToList();

            dataObj.InflowMgs = inflowMgsFixedIncome;
            dataObj.OutflowMgs = outflowMgsFixedIncome;

            // 8. GII
            var inflowGiiFixedIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Bond || x.InstrumentType == Common.TsItemCategory.Coupon)
                            && x.InstrumentCode.StartsWith("GII"))
                .GroupBy(x => x.InstrumentCode)
                .Select(x => new
                {
                    Name = x.Key,
                    AmountPlus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Sales = x.Sum(y => (double?)y.Sales) ?? 0,
                    Maturity = x.Sum(y => (double?)y.Maturity) ?? 0,
                    FirstLeg = x.Sum(y => (double?)y.FirstLeg) ?? 0,
                })
                .Select(x => new MYR_DealCutOffData_Item()
                {
                    Name = x.Name,
                    Amount = (x.AmountPlus + x.Sales + x.Maturity + x.FirstLeg)
                })
                .ToList();

            var outflowGiiFixedIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Bond || x.InstrumentType == Common.TsItemCategory.Coupon)
                            && x.InstrumentCode.StartsWith("GII"))
                .GroupBy(x => x.InstrumentCode)
                .Select(x => new
                {
                    Name = x.Key,
                    AmountMinus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Purchase = x.Sum(y => (double?)y.Purchase) ?? 0,
                    SecondLeg = x.Sum(y => (double?)y.SecondLeg) ?? 0
                })
                .Select(x => new MYR_DealCutOffData_Item()
                {
                    Name = x.Name,
                    Amount = (x.AmountMinus + x.Purchase + x.SecondLeg)
                })
                .ToList();

            dataObj.InflowGii = inflowGiiFixedIncome;
            dataObj.OutflowGii = outflowGiiFixedIncome;

            // 8. Total Fixed Income
            var inflowTotalFixIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && x.InstrumentType != Common.TsItemCategory.Equity)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountPlus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Sales = x.Sum(y => (double?)y.Sales) ?? 0,
                    Maturity = x.Sum(y => (double?)y.Maturity) ?? 0,
                    FirstLeg = x.Sum(y => (double?)y.FirstLeg) ?? 0
                })
                .Select(x => (x.Sales + x.Maturity + x.AmountPlus + x.FirstLeg))
                .FirstOrDefault();

            dataObj.InflowTotalFixedIncome = inflowTotalFixIncome;

            var outflowTotalFixIncome = db.ISSD_TradeSettlement
                .Where(x => approveTradeSettlementForms.Contains(x.FormId) && x.InstrumentType != Common.TsItemCategory.Equity)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountMinus = x.Sum(y => (double?)y.AmountMinus) ?? 0,
                    Purchase = x.Sum(y => (double?)y.Purchase) ?? 0,
                    SecondLeg = x.Sum(y => (double?)y.SecondLeg) ?? 0
                })
                .Select(x => (x.AmountMinus + x.Purchase + x.SecondLeg))
                .FirstOrDefault();

            dataObj.OutflowTotalFixedIncome = outflowTotalFixIncome;

            // 9. total net inflow
            dataObj.InflowTotalNet = (dataObj.MmaOb ?? 0) + (dataObj.TotalRhb ?? 0) + (dataObj.InflowTotalDepoPrincipalInterest ?? 0) +
                (dataObj.InflowTotalFixedIncome ?? 0) + (dataObj.InflowEquity ?? 0);

            dataObj.InflowTotalNetWithOb = dataObj.InflowTotalNet + dataObj.RentasOb ?? 0;

            return dataObj;

            
        }
    }

    public class MYR_DealCutOffData
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

        public List<MYR_DealCutOffData_Item> InflowMgs { get; set; }
        public List<MYR_DealCutOffData_Item> InflowGii { get; set; }

        public List<MYR_DealCutOffData_Item> OutflowMgs { get; set; }
        public List<MYR_DealCutOffData_Item> OutflowGii { get; set; }

        public double? InflowOthersFixedIncome { get; set; }
        public double? OutflowOthersFixedIncome { get; set; }

        public double? InflowTotalFixedIncome { get; set; }
        public double? OutflowTotalFixedIncome { get; set; }

        public double? InflowTotalNet { get; set; }
        public double? InflowTotalNetWithOb { get; set; }

    }

    public class MYR_DealCutOffData_Item
    {
        public string Name { get; set; }
        public double? Amount { get; set; }
    }
    
}