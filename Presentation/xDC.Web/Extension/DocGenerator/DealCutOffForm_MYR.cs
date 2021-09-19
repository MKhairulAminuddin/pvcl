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
                #region Sheet 1

                var sheet = workbook.Worksheets[0];

                sheet["J2"].Value = (dataItem.SelectedDate != null)
                    ? dataItem.SelectedDate.Value.ToString("dd/MM/yyyy")
                    : null;

                #region 1 - IF: OB Rentas, MMA, RHB

                sheet["J4"].Value = dataItem.RentasOb;
                sheet["G8"].Value = dataItem.MmaOb;
                sheet["G10"].Value = dataItem.TotalRhb;

                #endregion

                #region 2 - IF: Money Market

                sheet["E13"].Value = dataItem.InflowDepoPrincipal;
                sheet["E14"].Value = dataItem.InflowDepoInterest;
                sheet["G15"].Value = dataItem.InflowTotalDepoPrincipalInterest;

                #endregion

                #region 3 - IF: Fixed Income

                var currentRowIndex = 18;

                IterateFixedIncomeItem(dataItem.IF_Bond, Common.TsItemCategory.Bond, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_CP, Common.TsItemCategory.Cp, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_NP, Common.TsItemCategory.NotesPapers, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_REPO, Common.TsItemCategory.Repo, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Coupon, Common.TsItemCategory.Coupon, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Fees, Common.TsItemCategory.Fees, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Mtm, Common.TsItemCategory.Mtm, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Fx, Common.TsItemCategory.Fx, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Cn, Common.TsItemCategory.Cn, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Altid, Common.TsItemCategory.Altid, 18, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others, Common.TsItemCategory.Others, 18, ref sheet, ref currentRowIndex);

                if (currentRowIndex == 18)
                {
                    currentRowIndex += 1;
                }
                sheet["G" + currentRowIndex].Value = dataItem.IF_TotalFixedIncome;

                #endregion

                #region 4 - IF: Equity

                currentRowIndex += 3;
                sheet["E" + currentRowIndex].Value = dataItem.IF_Equity;
                currentRowIndex += 1;
                sheet["G" + currentRowIndex].Value = dataItem.IF_Equity;

                #endregion

                #region 5 - IF: Net

                currentRowIndex += 2;
                sheet["G" + currentRowIndex].Value = dataItem.IF_Net;
                sheet["J" + currentRowIndex].Value = dataItem.IF_Net;

                #endregion

                #region 6 - OF: Money Market

                currentRowIndex += 10;
                sheet["E" + currentRowIndex].Value = dataItem.OF_MM_Rollover;
                currentRowIndex += 1;
                sheet["E" + currentRowIndex].Value = dataItem.OF_MM_NewPlacement;
                currentRowIndex += 1;
                sheet["G" + currentRowIndex].Value = dataItem.OF_MM_Rollover + dataItem.OF_MM_NewPlacement;

                #endregion

                #region 7 - OF: Fixed Income

                int startIndex = currentRowIndex + 3;
                currentRowIndex += 3;

                IterateFixedIncomeItem(dataItem.OF_Bond, Common.TsItemCategory.Bond, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_CP, Common.TsItemCategory.Cp, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_NP, Common.TsItemCategory.NotesPapers, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_REPO, Common.TsItemCategory.Repo, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Coupon, Common.TsItemCategory.Coupon, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Fees, Common.TsItemCategory.Fees, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Mtm, Common.TsItemCategory.Mtm, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Fx, Common.TsItemCategory.Fx, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Cn, Common.TsItemCategory.Cn, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Altid, Common.TsItemCategory.Altid, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others, Common.TsItemCategory.Others, startIndex, ref sheet, ref currentRowIndex);

                if (currentRowIndex == startIndex)
                {
                    currentRowIndex += 1;
                }
                sheet["G" + currentRowIndex].Value = dataItem.OF_TotalFixedIncome;

                #endregion

                #region 8 - OF: Equity

                currentRowIndex += 3;
                sheet["E" + currentRowIndex].Value = dataItem.OF_Equity;
                currentRowIndex += 1;
                sheet["G" + currentRowIndex].Value = dataItem.OF_Equity;

                #endregion

                #region 9 - OF: Net

                currentRowIndex += 2;
                sheet["G" + currentRowIndex].Value = dataItem.OF_Net;
                sheet["J" + currentRowIndex].Value = dataItem.OF_Net * -1;

                #endregion

                #region 10 - Rentas + Inflow - Outflow

                currentRowIndex += 3;
                sheet["J" + currentRowIndex].Value = dataItem.RentasOb + dataItem.IF_Net - dataItem.OF_Net;

                #endregion

                workbook.Calculate();

                #endregion

                #region Sheet 2 - Money Market

                var sheet2 = workbook.Worksheets[1];

                var title = "MONEY MARKET TRANSACTIONS WITH FINANCIAL INSTITUTIONS FOR ";
                sheet2["C3"].Value = title + dataItem.SelectedDate.Value.ToString("dd/MM/yyyy");
                
                var sheet2_startIndex = 8;
                var sheet2_currentIndex = 8;
                
                MmiTab_MaturityTable(dataItem.SelectedDate.Value, dataItem.IF_MMI_Items, sheet2_startIndex, ref sheet2, ref sheet2_currentIndex);

                sheet2_startIndex = (sheet2_currentIndex != sheet2_startIndex) ? sheet2_currentIndex += 5: 14;
                sheet2_currentIndex = (sheet2_startIndex != 14) ? sheet2_currentIndex : 14;

                MmiTab_MaturityTable(dataItem.SelectedDate.Value, dataItem.OF_MMI_RolloverItems, sheet2_startIndex, ref sheet2, ref sheet2_currentIndex);

                sheet2_startIndex = (sheet2_currentIndex != sheet2_startIndex) ? sheet2_currentIndex += 5 : 20;
                sheet2_currentIndex = (sheet2_startIndex != 20) ? sheet2_currentIndex : 20;

                MmiTab_MaturityTable(dataItem.SelectedDate.Value, dataItem.OF_MMI_NewPlacementItems, sheet2_startIndex, ref sheet2, ref sheet2_currentIndex);

                workbook.Calculate();

                #endregion

                #region Sheet 3 - Others

                var sheet3 = workbook.Worksheets[2];

                sheet3["E6"].Value = dataItem.MmaOb;
                sheet3["E7"].Value = dataItem.TotalRhb;

                sheet3["E13"].Value = dataItem.IF_Equity;

                int s3StartIndex = 16;
                int s3CurrentIndex = 16;

                OthersTab_IfTable(dataItem.IF_OthersTab_Others, s3StartIndex, ref sheet3, ref s3CurrentIndex);


                workbook.Calculate();

                #endregion


            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
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

            #region 1 - IF OB Rentas, MMA, RHB

            //1. Opening Balance RENTAS from EDW
            var rentasOb = db.EDW_BankBalance
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "RENTAS")
                .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                .Select(x => x.Sum(y => y.Amount))
                .FirstOrDefault();
            dataObj.RentasOb = rentasOb ?? 0;

            //2. Opening Balance MMA from EDW
            var mmaOb = db.EDW_BankBalance
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate) && x.Currency == "MYR" && x.InstrumentType == "MMA")
                .GroupBy(x => new { x.Currency, x.InstrumentType, x.SettlementDate })
                .Select(x => x.Sum(y => y.Amount))
                .FirstOrDefault();
            dataObj.MmaOb = mmaOb ?? 0;

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
            dataObj.TotalRhb = totalRhb;

            #endregion

            #region 2 - IF MMI 
            
            var treasuryApprovedForms = db.FID_Treasury
                .Where(x => DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate) 
                            && x.Currency == "MYR" 
                            && x.FormStatus == Common.FormStatus.Approved)
                .Select(x => x.Id)
                .ToList();

            var ifTotalPrincipal = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.Principal))
                .FirstOrDefault();

            var ifTotalInterest = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.IntProfitReceivable))
                .FirstOrDefault();

            var inflowTotalDepoPrincipalInterest = ifTotalPrincipal + ifTotalInterest;

            dataObj.InflowDepoPrincipal = ifTotalPrincipal;
            dataObj.InflowDepoInterest = ifTotalInterest;
            dataObj.InflowTotalDepoPrincipalInterest = inflowTotalDepoPrincipalInterest;

            #endregion
            
            #region 3 - IF Fixed Income

            var approvedTsForms = db.ISSD_FormHeader
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                            && x.Currency == "MYR" && x.FormStatus == Common.FormStatus.Approved)
                .Select(x => x.Id)
                .ToList();

            dataObj.IF_Bond = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Bond);
            dataObj.IF_CP = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Cp);
            dataObj.IF_NP = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.NotesPapers);
            dataObj.IF_REPO = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Repo);
            dataObj.IF_Coupon = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Coupon);
            dataObj.IF_Fees = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Fees);
            dataObj.IF_Mtm = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Mtm);
            dataObj.IF_Fx = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Fx);
            dataObj.IF_Cn = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Cn);
            dataObj.IF_Altid = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Altid);
            dataObj.IF_Others = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Others);

            dataObj.IF_TotalFixedIncome = TradeSettlementSvc.GetTotalInflowWithoutEquity(db, approvedTsForms);

            #endregion

            #region 4 - IF Equity

            dataObj.IF_Equity = TradeSettlementSvc.GetTotalInflowByCategory(db, approvedTsForms, Common.TsItemCategory.Equity);

            #endregion

            #region 5 - IF Net

            dataObj.IF_Net = dataObj.IF_Equity + dataObj.IF_TotalFixedIncome +
                             dataObj.InflowTotalDepoPrincipalInterest + dataObj.TotalRhb + dataObj.MmaOb;

            #endregion

            #region 6 - OF MMI 
            
            dataObj.OF_MM_NewPlacement = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) 
                            && x.CashflowType == Common.Cashflow.Outflow
                            && x.Notes == "New")
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.Principal))
                .FirstOrDefault();

            dataObj.OF_MM_Rollover = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) 
                            && x.CashflowType == Common.Cashflow.Outflow
                            && x.Notes == "r/o p+i")
                .GroupBy(x => x.CashflowType)
                .Select(x => x.Sum(y => y.IntProfitReceivable))
                .FirstOrDefault();

            #endregion

            #region 7 - OF Fixed Income

            dataObj.OF_Bond = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Bond);
            dataObj.OF_CP = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Cp);
            dataObj.OF_NP = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.NotesPapers);
            dataObj.OF_REPO = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Repo);
            dataObj.OF_Coupon = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Coupon);
            dataObj.OF_Fees = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Fees);
            dataObj.OF_Mtm = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Mtm);
            dataObj.OF_Fx = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Fx);
            dataObj.OF_Cn = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Cn);
            dataObj.OF_Altid = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Altid);
            dataObj.OF_Others = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Others);

            dataObj.OF_TotalFixedIncome = TradeSettlementSvc.GetTotalOutflowWithoutEquity(db, approvedTsForms);

            #endregion

            #region 8 - OF Equity

            dataObj.OF_Equity = TradeSettlementSvc.GetTotalOutflowByCategory(db, approvedTsForms, Common.TsItemCategory.Equity);

            #endregion

            #region 9 - OF Net

            dataObj.OF_Net = dataObj.OF_Equity + dataObj.OF_TotalFixedIncome +
                             dataObj.OF_MM_NewPlacement + dataObj.OF_MM_Rollover;

            #endregion

            #region 10 - MMI Inflow

            var ifInflowItems = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) 
                            && x.CashflowType == Common.Cashflow.Inflow)
                .Select(x => new MYR_DealCutOffData_Mmi
                {
                    Bank = x.Bank,
                    MaturityDate = x.MaturityDate.Value,
                    ValueDate = x.ValueDate.Value,
                    Principal = x.Principal,
                    Interest = x.IntProfitReceivable,
                    PrincipalInterest = x.PrincipalIntProfitReceivable,
                    Rate = x.RatePercent,
                    ContactPerson = x.ContactPerson,
                    Notes = x.Notes
                })
                .OrderBy(x => x.MaturityDate)
                .ToList();

            dataObj.IF_MMI_Items = ifInflowItems;

            #endregion

            #region 11 - MMI OF Rollover & New Placement

            var ofInflowRolloverItems = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId) 
                            && x.CashflowType == Common.Cashflow.Outflow
                            && x.Notes == "r/o P+I")
                .Select(x => new MYR_DealCutOffData_Mmi
                {
                    Bank = x.Bank,
                    MaturityDate = x.MaturityDate.Value,
                    ValueDate = x.ValueDate.Value,
                    Principal = x.Principal,
                    Interest = x.IntProfitReceivable,
                    PrincipalInterest = x.PrincipalIntProfitReceivable,
                    Rate = x.RatePercent,
                    ContactPerson = x.ContactPerson,
                    Notes = x.Notes
                })
                .OrderBy(x => x.MaturityDate)
                .ToList();

            dataObj.OF_MMI_RolloverItems = ofInflowRolloverItems;

            var ofInflowNewPlacementItems = db.FID_Treasury_Deposit
                .Where(x => treasuryApprovedForms.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Outflow
                            && x.Notes == "New")
                .Select(x => new MYR_DealCutOffData_Mmi
                {
                    Bank = x.Bank,
                    MaturityDate = x.MaturityDate.Value,
                    ValueDate = x.ValueDate.Value,
                    Principal = x.Principal,
                    Interest = x.IntProfitReceivable,
                    PrincipalInterest = x.PrincipalIntProfitReceivable,
                    Rate = x.RatePercent,
                    ContactPerson = x.ContactPerson,
                    Notes = x.Notes
                })
                .ToList();

            dataObj.OF_MMI_NewPlacementItems = ofInflowNewPlacementItems;

            #endregion

            #region Others Tab - IF Others

            /*var ifTsOthers = db.ISSD_TradeSettlement
                .Where(x => approvedTsForms.Contains(x.FormId) && x.InstrumentType != Common.TsItemCategory.Equity)
                .Select(x => new MYR_DealCutOffData_OthersItem
                {
                    InstrumentType = x.InstrumentType,
                    Bank = x.InstrumentCode,
                    StockCode = x.StockCode,
                    TradeDate = null,
                    SettlementDate = x.se,
                    NominalAmount = x.,
                    Price = 0,
                    Rate = 0,
                    Proceed = 0,
                    Notes = null
                })
                .ToList();*/

            #endregion

            return dataObj;

            
        }

        private void IterateFixedIncomeItem(double amount, string category, int startIndex, ref Worksheet sheet, ref int currentRowIndex)
        {
            if (amount > 0)
            {
                if (currentRowIndex != startIndex)
                {
                    sheet.Rows[currentRowIndex-1].Insert(InsertCellsMode.ShiftCellsDown);
                    sheet.Rows[currentRowIndex-1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                }
                sheet["C" + currentRowIndex].Value = category;
                sheet["E" + currentRowIndex].Value = amount;
                currentRowIndex++;
                
            }
        }

        private void MmiTab_MaturityTable(DateTime selectedDate, List<MYR_DealCutOffData_Mmi> items, int startIndex, ref Worksheet sheet, ref int currentIndex)
        {
            sheet["E" + (startIndex - 3)].Value = selectedDate;

            if (items.Any())
            {
                foreach (var item in items)
                {
                    if (currentIndex != startIndex)
                    {
                        sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                        sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                    }

                    sheet["D" + currentIndex].Value = item.Bank;
                    sheet["E" + currentIndex].Value = item.MaturityDate;
                    sheet["F" + currentIndex].Value = item.ValueDate;
                    sheet["G" + currentIndex].Value = item.Principal;
                    sheet["I" + currentIndex].Value = item.Rate;
                    sheet["J" + currentIndex].Value = item.Interest;
                    sheet["K" + currentIndex].Value = item.PrincipalInterest;
                    sheet["L" + currentIndex].Value = item.ContactPerson;
                    sheet["M" + currentIndex].Value = item.Notes;

                    currentIndex++;
                }

                if (currentIndex != startIndex)
                {
                    sheet["G" + currentIndex].Formula = "=SUM($G$" + startIndex + ":$G$" + (currentIndex - 1) + ")";
                    sheet["J" + currentIndex].Formula = "=SUM($J$" + startIndex + ":$J$" + (currentIndex - 1) + ")";
                    sheet["K" + currentIndex].Formula = "=SUM($K$" + startIndex + ":$K$" + (currentIndex - 1) + ")";
                }
            }
        }

        private void OthersTab_IfTable(List<MYR_DealCutOffData_OthersItem> items, int startIndex, ref Worksheet sheet, ref int currentIndex)
        {
            if (items.Any())
            {
                var availableGroup = items.GroupBy(x => x.InstrumentType);

                foreach (var group in availableGroup)
                {
                    if (currentIndex == startIndex)
                    {
                        sheet["C16"].Value = group.Key;
                    }
                    else
                    {
                        sheet["C" + currentIndex].Value = group.Key;
                    }

                    foreach (var item in items)
                    {
                        if (currentIndex != startIndex)
                        {
                            sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                            sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                        }

                        sheet["D" + currentIndex].Value = item.Bank;
                        sheet["E" + currentIndex].Value = item.StockCode;
                        sheet["F" + currentIndex].Value = item.TradeDate;
                        sheet["G" + currentIndex].Value = item.SettlementDate;
                        sheet["H" + currentIndex].Value = item.NominalAmount;
                        sheet["I" + currentIndex].Value = item.Price;
                        sheet["J" + currentIndex].Value = item.Rate;
                        sheet["K" + currentIndex].Value = item.Proceed;
                        sheet["L" + currentIndex].Value = item.Notes;

                        currentIndex++;
                    }

                    if (currentIndex != startIndex)
                    {
                        sheet["H" + currentIndex].Formula = "=SUM($H$" + startIndex + ":$H$" + (currentIndex - 1) + ")";
                        sheet["K" + currentIndex].Formula = "=SUM($K$" + startIndex + ":$K$" + (currentIndex - 1) + ")";
                    }
                }

                
            }
        }
    }

    public class MYR_DealCutOffData
    {
        public DateTime? SelectedDate { get; set; }
        public double RentasOb { get; set; }
        public double MmaOb { get; set; }
        public double TotalRhb { get; set; }
        public double InflowDepoPrincipal { get; set; }
        public double InflowDepoInterest { get; set; }
        public double InflowTotalDepoPrincipalInterest { get; set; }

        public double IF_Bond { get; set; }
        public double IF_CP { get; set; }
        public double IF_NP { get; set; }
        public double IF_REPO { get; set; }
        public double IF_Coupon { get; set; }
        public double IF_Fees { get; set; }
        public double IF_Mtm { get; set; }
        public double IF_Fx { get; set; }
        public double IF_Cn { get; set; }
        public double IF_Altid { get; set; }
        public double IF_Others { get; set; }
        public double IF_TotalFixedIncome { get; set; }

        public double IF_Equity { get; set; }
        public double IF_Net { get; set; }

        public double OF_MM_Rollover { get; set; }
        public double OF_MM_NewPlacement { get; set; }

        public double OF_Bond { get; set; }
        public double OF_CP { get; set; }
        public double OF_NP { get; set; }
        public double OF_REPO { get; set; }
        public double OF_Coupon { get; set; }
        public double OF_Fees { get; set; }
        public double OF_Mtm { get; set; }
        public double OF_Fx { get; set; }
        public double OF_Cn { get; set; }
        public double OF_Altid { get; set; }
        public double OF_Others { get; set; }
        public double OF_TotalFixedIncome { get; set; }

        public double OF_Equity { get; set; }
        public double OF_Net { get; set; }

        public List<MYR_DealCutOffData_Mmi> IF_MMI_Items { get; set; }
        public List<MYR_DealCutOffData_Mmi> OF_MMI_RolloverItems { get; set; }
        public List<MYR_DealCutOffData_Mmi> OF_MMI_NewPlacementItems { get; set; }

        public List<MYR_DealCutOffData_OthersItem> IF_OthersTab_Others { get; set; }

    }

    public class MYR_DealCutOffData_OthersItem
    {
        public string InstrumentType { get; set; }
        public string Bank { get; set; }
        public string StockCode { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime SettlementDate { get; set; }
        public double NominalAmount { get; set; }
        public double Price { get; set; }
        public double Rate { get; set; }
        public double Proceed { get; set; }
        public string Notes { get; set; }
    }

    public class MYR_DealCutOffData_Mmi
    {
        public string Bank { get; set; }
        public DateTime MaturityDate { get; set; }
        public DateTime ValueDate { get; set; }
        public double Principal { get; set; }
        public int Tenor { get; set; }
        public double Rate { get; set; }
        public double Interest { get; set; }
        public double PrincipalInterest { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }
    }

}