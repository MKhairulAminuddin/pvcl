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
        private Color _tableHeaderPrimaryColor = ColorTranslator.FromHtml("#5b8efb");
        private Color _inflowColor = ColorTranslator.FromHtml("#3498DB");
        private Color _outFlowColor = ColorTranslator.FromHtml("#E67E22");

        public IWorkbook GenerateWorkbook(DateTime? selectedDate, bool viewApproved)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var dataObj = ConstructData(db, selectedDate, viewApproved);

                    IWorkbook workbook = new Workbook();
                    workbook.Options.Culture = new CultureInfo("en-US");
                    workbook.LoadDocument(MapPath(Common.ExcelTemplateLocation.FID_DealCutOff_MYR));
                    workbook = GenerateDocument(workbook, dataObj, viewApproved);

                    return workbook;

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public string GenerateFile(DateTime selectedDate, bool isExportAsExcel, bool viewApproved)
        {
            try
            {
                IWorkbook workbook = GenerateWorkbook(selectedDate, viewApproved);
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

        private IWorkbook GenerateDocument(IWorkbook workbook, MYR_DealCutOffData dataItem, bool viewApproved)
        {
            workbook.BeginUpdate();
            try
            {

                var sheet = workbook.Worksheets[0];

                if (!viewApproved)
                {
                    sheet["B1:E1"].Merge();
                    sheet["B1"].Value = Config.FormViewAllSubmittedData;
                    sheet["B1"].Font.Italic = true;
                    sheet["B1"].Font.Bold = true;
                    sheet["B1"].Font.Color = Color.Red;
                    sheet["B1"].Font.Size = 10;
                }

                sheet["J2"].Value = (dataItem.SelectedDate != null)
                    ? dataItem.SelectedDate.Value.ToString("dd/MM/yyyy")
                    : null;

                #region Sheet 1 - Summary Cashflow
                
                #region IF: OB Rentas, MMA, RHB

                sheet["J4"].Value = dataItem.RentasOb;
                sheet["G8"].Value = dataItem.MmaOb;
                sheet["G10"].Value = dataItem.TotalRhb;

                #endregion

                #region IF: Money Market

                sheet["E13"].Value = dataItem.InflowDepoPrincipal;
                sheet["E14"].Value = dataItem.InflowDepoInterest;
                sheet["G15"].Value = dataItem.InflowTotalDepoPrincipalInterest;

                #endregion

                #region IF: Fixed Income

                sheet["E18"].Value = dataItem.IF_FixedIncome_Mgs;
                sheet["E19"].Value = dataItem.IF_FixedIncome_NonMgs;

                #endregion

                #region IF: Equity

                sheet["E23"].Value = dataItem.IF_Equity;

                #endregion

                #region IF: Others

                var startRowIndex = 27;
                var currentRowIndex = 27;

                IterateFixedIncomeItem(dataItem.IF_Others_CP, Common.TsItemCategory.Cp, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_NP, Common.TsItemCategory.NotesPapers, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_REPO, Common.TsItemCategory.Repo, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_Fees, Common.TsItemCategory.Fees, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_Mtm, Common.TsItemCategory.Mtm, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_Fx, Common.TsItemCategory.Fx, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_Cn, Common.TsItemCategory.Cn, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_Altid, Common.TsItemCategory.Altid, startRowIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.IF_Others_Others, Common.TsItemCategory.Others, startRowIndex, ref sheet, ref currentRowIndex);

                if (currentRowIndex == startRowIndex)
                {
                    currentRowIndex += 1;
                }
                sheet["G" + currentRowIndex].Value = dataItem.IF_Others_Total;

                #endregion

                #region IF: Net

                currentRowIndex += 2;
                sheet["G" + currentRowIndex].Value = dataItem.IF_Net;
                sheet["J" + currentRowIndex].Value = dataItem.IF_Net;

                #endregion

                #region OF: Money Market

                currentRowIndex += 10;
                sheet["E" + currentRowIndex].Value = dataItem.OF_MM_Rollover;
                currentRowIndex += 1;
                sheet["E" + currentRowIndex].Value = dataItem.OF_MM_NewPlacement;
                currentRowIndex += 1;
                sheet["G" + currentRowIndex].Value = dataItem.OF_MM_Rollover + dataItem.OF_MM_NewPlacement;

                #endregion

                #region OF: Fixed Income

                currentRowIndex += 3;
                sheet["E" + currentRowIndex].Value = dataItem.OF_FixedIncome_Mgs;
                currentRowIndex += 1;
                sheet["E" + currentRowIndex].Value = dataItem.OF_FixedIncome_NonMgs;

                #endregion

                #region OF: Equity

                currentRowIndex += 4;
                sheet["E" + currentRowIndex].Value = dataItem.OF_Equity;
                currentRowIndex += 1;
                sheet["G" + currentRowIndex].Value = dataItem.OF_Equity;

                #endregion

                #region OF: Others

                int startIndex = currentRowIndex + 3;
                currentRowIndex += 3;

                IterateFixedIncomeItem(dataItem.OF_Others_CP, Common.TsItemCategory.Cp, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_NP, Common.TsItemCategory.NotesPapers, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_REPO, Common.TsItemCategory.Repo, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_Fees, Common.TsItemCategory.Fees, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_Mtm, Common.TsItemCategory.Mtm, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_Fx, Common.TsItemCategory.Fx, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_Cn, Common.TsItemCategory.Cn, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_Altid, Common.TsItemCategory.Altid, startIndex, ref sheet, ref currentRowIndex);
                IterateFixedIncomeItem(dataItem.OF_Others_Others, Common.TsItemCategory.Others, startIndex, ref sheet, ref currentRowIndex);

                if (currentRowIndex == startIndex)
                {
                    currentRowIndex += 1;
                }
                sheet["G" + currentRowIndex].Value = dataItem.OF_Others_Total;

                #endregion

                #region OF: Net

                currentRowIndex += 2;
                sheet["G" + currentRowIndex].Value = dataItem.OF_Net;
                sheet["J" + currentRowIndex].Value = dataItem.OF_Net * -1;

                #endregion

                #region TOTAL - Rentas + Inflow - Outflow

                currentRowIndex += 3;
                sheet["J" + currentRowIndex].Value = dataItem.RentasOb + dataItem.IF_Net - dataItem.OF_Net;

                #endregion

                workbook.Calculate();

                #endregion

                #region Sheet 2 - Money Market

                var sheet2 = workbook.Worksheets[1];

                if (!viewApproved)
                {
                    sheet2["C1:E1"].Merge();
                    sheet2["C1"].Value = Config.FormViewAllSubmittedData;
                    sheet2["C1"].Font.Italic = true;
                    sheet2["C1"].Font.Bold = true;
                    sheet2["C1"].Font.Color = Color.Red;
                    sheet2["C1"].Font.Size = 10;
                }

                var title = "MONEY MARKET TRANSACTIONS WITH FINANCIAL INSTITUTIONS FOR ";
                sheet2["C3"].Value = title + dataItem.SelectedDate.Value.ToString("dd/MM/yyyy");

                var s2_maturity_startIndex = 5;
                var s2_maturity_endIndex = 9;

                MmiTab_Table(dataItem.SelectedDate.Value, dataItem.IF_MMI_Items, s2_maturity_startIndex, ref s2_maturity_endIndex, ref sheet2);

                var s2_rollover_startIndex = s2_maturity_endIndex + 2;
                var s2_rollover_endIndex = s2_maturity_endIndex + 6;

                MmiTab_Table(dataItem.SelectedDate.Value, dataItem.OF_MMI_RolloverItems, s2_rollover_startIndex, ref s2_rollover_endIndex, ref sheet2);

                var s2_newPlacement_startIndex = s2_rollover_endIndex + 2;
                var s2_newPlacement_endIndex = s2_rollover_endIndex + 6;

                MmiTab_Table(dataItem.SelectedDate.Value, dataItem.OF_MMI_NewPlacementItems, s2_newPlacement_startIndex, ref s2_newPlacement_endIndex, ref sheet2);

                
                workbook.Calculate();

                #endregion

                #region Sheet 3 - Others

                var sheet3 = workbook.Worksheets[2];

                if (!viewApproved)
                {
                    sheet3["C1:E1"].Merge();
                    sheet3["C1"].Value = Config.FormViewAllSubmittedData;
                    sheet3["C1"].Font.Italic = true;
                    sheet3["C1"].Font.Bold = true;
                    sheet3["C1"].Font.Color = Color.Red;
                    sheet3["C1"].Font.Size = 10;
                }

                sheet3["E6"].Value = dataItem.MmaOb;
                sheet3["E7"].Value = dataItem.TotalRhb;

                var s3_mgs_startIndex = 10;
                var s3_mgs_endIndex = 14;

                OthersTab_IfTable1(dataItem.IF_OthersTab_MGS_GII, s3_mgs_startIndex, ref s3_mgs_endIndex, ref sheet3);

                var s3_pds_startIndex = s3_mgs_endIndex + 2;
                var s3_pds_endIndex = s3_mgs_endIndex + 6;

                OthersTab_IfTable1(dataItem.IF_OthersTab_PssCpNidBaBnm, s3_pds_startIndex, ref s3_pds_endIndex, ref sheet3);

                var s3_equity_startIndex = s3_pds_endIndex + 2;
                var s3_equity_endIndex = s3_pds_endIndex + 6;

                sheet3["E"+ (s3_equity_startIndex+3)].Value = dataItem.IF_Equity;

                var s3_others_startIndex = s3_equity_endIndex + 2;
                var s3_others_endIndex = s3_equity_endIndex + 6;

                OthersTab_IfTable2(dataItem.IF_OthersTab_Others, s3_others_startIndex, ref s3_others_endIndex, ref sheet3);

                var s3_of_mgs_startIndex = s3_others_endIndex + 5;
                var s3_of_mgs_endIndex = s3_others_endIndex + 9;

                OthersTab_IfTable1(dataItem.OF_OthersTab_MGS_GII, s3_of_mgs_startIndex, ref s3_of_mgs_endIndex, ref sheet3);

                var s3_of_pds_startIndex = s3_of_mgs_endIndex + 2;
                var s3_of_pds_endIndex = s3_of_mgs_endIndex + 6;

                OthersTab_IfTable1(dataItem.OF_OthersTab_PssCpNidBaBnm, s3_of_pds_startIndex, ref s3_of_pds_endIndex, ref sheet3);

                var s3_of_equity_startIndex = s3_of_pds_endIndex + 2;
                var s3_of_equity_endIndex = s3_of_pds_endIndex + 6;

                sheet3["E" + (s3_of_equity_startIndex + 3)].Value = dataItem.OF_Equity;

                var s3_of_others_startIndex = s3_of_equity_endIndex + 2;
                var s3_of_others_endIndex = s3_of_equity_endIndex + 6;

                OthersTab_IfTable2(dataItem.OF_OthersTab_Others, s3_of_others_startIndex, ref s3_of_others_endIndex, ref sheet3);
                
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

        private MYR_DealCutOffData ConstructData(kashflowDBEntities db, DateTime? selectedDate, bool viewApproved = true)
        {
            //get data 
            var dataObj = new MYR_DealCutOffData()
            {
                SelectedDate = selectedDate
            };

            #region Sheet 1 - Summary

            #region 1 - IF OB Rentas, MMA, RHB

            //1. Opening Balance RENTAS from EDW
            var rentasOb = db.EDW_BankBalance
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                            && x.Currency == "MYR"
                            && x.InstrumentType == "RENTAS")
                .Select(x => x.Amount)
                .DefaultIfEmpty(0)
                .Sum();
            dataObj.RentasOb = rentasOb ?? 0;

            //2. Opening Balance MMA from EDW
            var mmaOb = db.EDW_BankBalance
                .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                            && x.Currency == "MYR"
                            && x.InstrumentType == "MMA")
                .Select(x => x.Amount)
                .DefaultIfEmpty(0)
                .Sum();
            dataObj.MmaOb = mmaOb ?? 0;

            //3. AMSD Inflow Fund RHB
            var amsdApprovedForms = (viewApproved)
                ? db.AMSD_IF
                    .Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.FormStatus == Common.FormStatus.Approved)
                    .Select(x => x.Id)
                    .ToList()
                : db.AMSD_IF
                    .Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.FormStatus != Common.FormStatus.Rejected)
                    .Select(x => x.Id)
                    .ToList();

            var totalRhb = db.AMSD_IF_Item
                .Where(x => amsdApprovedForms.Contains(x.FormId))
                .Select(x => x.Amount)
                .DefaultIfEmpty(0)
                .Sum();

            dataObj.TotalRhb = totalRhb;

            #endregion

            #region 2 - IF MMI

            var treasuryFormIds = (viewApproved)
                ? db.FID_Treasury
                    .Where(x => DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == "MYR"
                                && x.FormStatus == Common.FormStatus.Approved)
                    .Select(x => x.Id)
                    .ToList()
                : db.FID_Treasury
                    .Where(x => DbFunctions.TruncateTime(x.TradeDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == "MYR"
                                && x.FormStatus != Common.FormStatus.Rejected)
                    .Select(x => x.Id)
                    .ToList();

            var ifTotalPrincipal = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .Select(x => x.Principal)
                .DefaultIfEmpty(0)
                .Sum();

            var ifTotalInterest = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId) && x.CashflowType == Common.Cashflow.Inflow)
                .Select(x => x.IntProfitReceivable)
                .DefaultIfEmpty(0)
                .Sum();

            var inflowTotalDepoPrincipalInterest = ifTotalPrincipal + ifTotalInterest;

            dataObj.InflowDepoPrincipal = ifTotalPrincipal;
            dataObj.InflowDepoInterest = ifTotalInterest;
            dataObj.InflowTotalDepoPrincipalInterest = inflowTotalDepoPrincipalInterest;

            #endregion

            #region 3 - IF Fixed Income

            var tsFormIds = (viewApproved)
                ? db.ISSD_FormHeader
                    .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == "MYR"
                                && x.FormStatus == Common.FormStatus.Approved)
                    .Select(x => x.Id)
                    .ToList()
                : db.ISSD_FormHeader
                    .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(selectedDate)
                                && x.Currency == "MYR"
                                && x.FormStatus != Common.FormStatus.Rejected)
                    .Select(x => x.Id)
                    .ToList();

            var couponBondMgs = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond)
                            && (x.InstrumentCode.Contains("MGS") || x.InstrumentCode.Contains("MGII") || x.InstrumentCode.Contains("GII"))
                            && (x.InflowAmount) > 0)
                .Select(x => x.InflowAmount)
                .DefaultIfEmpty(0)
                .Sum();

            dataObj.IF_FixedIncome_Mgs = couponBondMgs;

            var couponNonMgs = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond)
                            && (!x.InstrumentCode.Contains("MGS") && !x.InstrumentCode.Contains("MGII") && !x.InstrumentCode.Contains("GII"))
                            && (x.InflowAmount) > 0)
                .Select(x => x.InflowAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var treasuryMmIf = db.FID_Treasury_MMI
                .Where(x => treasuryFormIds.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Inflow)
                .Select(x => x.Proceeds)
                .DefaultIfEmpty(0)
                .Sum();

            dataObj.IF_FixedIncome_NonMgs = couponNonMgs + treasuryMmIf;

            dataObj.IF_FixedIncome_Total = dataObj.IF_FixedIncome_Mgs + dataObj.IF_FixedIncome_NonMgs;

            #endregion

            #region 4 - IF Equity

            dataObj.IF_Equity = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Equity);

            #endregion

            #region 5 - IF Others

            dataObj.IF_Others_CP = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Cp);
            dataObj.IF_Others_NP = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.NotesPapers);
            dataObj.IF_Others_REPO = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Repo);
            dataObj.IF_Others_Fees = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Fees);
            dataObj.IF_Others_Mtm = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Mtm);
            dataObj.IF_Others_Fx = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Fx);
            dataObj.IF_Others_Cn = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Cn);
            dataObj.IF_Others_Altid = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Altid);
            dataObj.IF_Others_Others = TradeSettlementSvc.GetTotalInflowByCategory(db, tsFormIds, Common.TsItemCategory.Others);

            dataObj.IF_Others_Total = dataObj.IF_Others_CP + dataObj.IF_Others_NP + dataObj.IF_Others_REPO + dataObj.IF_Others_Fees +
                                           dataObj.IF_Others_Mtm + dataObj.IF_Others_Fx + dataObj.IF_Others_Cn + dataObj.IF_Others_Altid +
                                           dataObj.IF_Others_Others;

            #endregion

            #region 5 - IF Net

            dataObj.IF_Net = dataObj.TotalRhb + dataObj.MmaOb +
                             dataObj.InflowTotalDepoPrincipalInterest +
                             dataObj.IF_Equity + dataObj.IF_FixedIncome_Total +
                             dataObj.IF_Others_Total;

            #endregion

            #region 6 - OF MMI 

            dataObj.OF_MM_NewPlacement = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Outflow
                            && x.Notes == "New")
                .Select(x => x.Principal)
                .DefaultIfEmpty(0)
                .Sum();

            dataObj.OF_MM_Rollover = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Outflow
                            && x.Notes == "r/o p+i")
                .Select(x => x.IntProfitReceivable)
                .DefaultIfEmpty(0)
                .Sum();

            #endregion

            #region 7 - OF Fixed Income


            var of_couponBondMgs = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond)
                            && (x.InstrumentCode.Contains("MGS") || x.InstrumentCode.Contains("MGII") || x.InstrumentCode.Contains("GII"))
                            && (x.OutflowAmount) > 0)
                .Select(x => x.OutflowAmount)
                .DefaultIfEmpty(0)
                .Sum();

            dataObj.OF_FixedIncome_Mgs = of_couponBondMgs;

            var of_couponBondNonMgs = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond || x.InstrumentType == Common.TsItemCategory.Repo)
                            && (!x.InstrumentCode.Contains("MGS") && !x.InstrumentCode.Contains("MGII") && !x.InstrumentCode.Contains("GII"))
                            && (x.OutflowAmount) > 0)
                .Select(x => x.OutflowAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var treasuryMmOf = db.FID_Treasury_MMI
                .Where(x => treasuryFormIds.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Outflow
                            )
                .Select(x => x.Proceeds)
                .DefaultIfEmpty(0)
                .Sum();

            dataObj.OF_FixedIncome_NonMgs = of_couponBondNonMgs + treasuryMmOf;

            dataObj.OF_FixedIncome_Total = dataObj.OF_FixedIncome_Mgs + dataObj.OF_FixedIncome_NonMgs;

            #endregion

            #region 8 - OF Equity

            dataObj.OF_Equity = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Equity);

            #endregion

            #region 9 - OF Others

            dataObj.OF_Others_CP = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Cp);
            dataObj.OF_Others_NP = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.NotesPapers);
            dataObj.OF_Others_REPO = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Repo);
            dataObj.OF_Others_Fees = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Fees);
            dataObj.OF_Others_Mtm = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Mtm);
            dataObj.OF_Others_Fx = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Fx);
            dataObj.OF_Others_Cn = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Cn);
            dataObj.OF_Others_Altid = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Altid);
            dataObj.OF_Others_Others = TradeSettlementSvc.GetTotalOutflowByCategory(db, tsFormIds, Common.TsItemCategory.Others);

            dataObj.OF_Others_Total = dataObj.OF_Others_CP + dataObj.OF_Others_NP + dataObj.OF_Others_REPO + dataObj.OF_Others_Fees +
                                      dataObj.OF_Others_Mtm + dataObj.OF_Others_Fx + dataObj.OF_Others_Cn + dataObj.OF_Others_Altid +
                                      dataObj.OF_Others_Others;

            #endregion

            #region 10 - OF Net

            dataObj.OF_Net = dataObj.OF_Equity + dataObj.OF_FixedIncome_Total +
                             dataObj.OF_MM_NewPlacement + dataObj.OF_MM_Rollover +
                             dataObj.OF_Others_Total;

            #endregion

            #endregion

            #region Sheet 2 - Money Market

            #region IF: Deposit Maturity

            var ifInflowItems = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId)
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
                    Notes = x.Notes,
                    AssetType = x.AssetType
                })
                .OrderBy(x => x.MaturityDate)
                .ToList();

            dataObj.IF_MMI_Items = ifInflowItems;

            #endregion

            #region OF Rollover & New Placement

            var ofInflowRolloverItems = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId)
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
                    Notes = x.Notes,
                    AssetType = x.AssetType
                })
                .OrderBy(x => x.MaturityDate)
                .ToList();

            dataObj.OF_MMI_RolloverItems = ofInflowRolloverItems;

            var ofInflowNewPlacementItems = db.FID_Treasury_Deposit
                .Where(x => treasuryFormIds.Contains(x.FormId)
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
                    Notes = x.Notes,
                    AssetType = x.AssetType
                })
                .ToList();

            dataObj.OF_MMI_NewPlacementItems = ofInflowNewPlacementItems;

            #endregion

            #endregion

            #region Sheet 3 - Others

            #region Others Tab - IF - MGS & GII - Sales, maturity or coupon
            
            var othersTab_if_mgs = new List<MYR_DealCutOffData_OthersTab_Item1>();

            var ifCoupon = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond)
                            && (x.InstrumentCode.Contains("MGS") || x.InstrumentCode.Contains("MGII") || x.InstrumentCode.Contains("GII"))
                            && (x.InflowAmount) > 0)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item1
                {
                    Bank = x.InstrumentCode,
                    StockCode = x.StockCode,
                    TradeDate = null,
                    SettlementDate = dataObj.SelectedDate,
                    NominalAmount = null,
                    Price = null,
                    Rate = null,
                    Proceed = x.InflowAmount,
                    Notes = x.InstrumentType
                })
                .ToList();
            
            if (ifCoupon.Any())
            {
                othersTab_if_mgs.AddRange(ifCoupon);
            }

            dataObj.IF_OthersTab_MGS_GII = othersTab_if_mgs;

            #endregion

            #region Others Tab - IF - PDS / CP / NID / BA / BNMN - Sales, maturity or coupon

            var othersTab_if_pds = new List<MYR_DealCutOffData_OthersTab_Item1>();

            var othersTab_pds_itemCoupon = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond)
                            && (!x.InstrumentCode.Contains("MGS") && !x.InstrumentCode.Contains("MGII") && !x.InstrumentCode.Contains("GII"))
                            && (x.InflowAmount) > 0)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item1
                {
                    Bank = x.InstrumentCode,
                    StockCode = x.StockCode,
                    TradeDate = null,
                    SettlementDate = dataObj.SelectedDate,
                    NominalAmount = null,
                    Price = null,
                    Rate = null,
                    Proceed = x.AmountPlus,
                    Notes = x.Remarks
                })
                .ToList();

            var othersTab_pds_item = db.FID_Treasury_MMI
                .Where(x => treasuryFormIds.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Inflow)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item1
                {
                    Bank = x.Issuer,
                    StockCode = x.CertNoStockCode,
                    TradeDate = x.ValueDate,
                    SettlementDate = x.MaturityDate,
                    NominalAmount = x.Nominal,
                    Price = x.Price,
                    Rate = x.SellPurchaseRateYield,
                    Proceed = x.Proceeds,
                    Notes = x.ProductType
                })
                .ToList();
            
            if (othersTab_pds_item.Any())
            {
                othersTab_if_pds.AddRange(othersTab_pds_item);
            }

            if (othersTab_pds_itemCoupon.Any())
            {
                othersTab_if_pds.AddRange(othersTab_pds_itemCoupon);
            }

            dataObj.IF_OthersTab_PssCpNidBaBnm = othersTab_if_pds;

            #endregion

            #region Others Tab - IF - Others

            var othersTab_if_others = new List<MYR_DealCutOffData_OthersTab_Item2>();
            
            var othersTab_if_others_item = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType != Common.TsItemCategory.Coupon && x.InstrumentType != Common.TsItemCategory.Equity && x.InstrumentType != Common.TsItemCategory.Bond)
                            && x.InflowAmount > 0)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item2
                {
                    Items = x.InstrumentCode,
                    Amount = x.InflowAmount,
                    Notes = x.InstrumentType
                })
                .ToList();

            if (othersTab_if_others_item.Any())
            {
                othersTab_if_others.AddRange(othersTab_if_others_item);
            }

            dataObj.IF_OthersTab_Others = othersTab_if_others;

            #endregion

            #region Others Tab - OF - MGS/GII/Quasi Bond Purchase 

            var othersTab_of_mgs = new List<MYR_DealCutOffData_OthersTab_Item1>();

            var othersTab_of_mgs_itemBond = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond)
                            && (x.InstrumentCode.Contains("MGS") || x.InstrumentCode.Contains("MGII") || x.InstrumentCode.Contains("GII"))
                            && (x.OutflowAmount) > 0)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item1
                {
                    Bank = x.InstrumentCode,
                    StockCode = x.StockCode,
                    TradeDate = null,
                    SettlementDate = dataObj.SelectedDate,
                    NominalAmount = null,
                    Price = null,
                    Rate = null,
                    Proceed = x.OutflowAmount,
                    Notes = x.InstrumentType
                })
                .ToList();
            
            if (othersTab_of_mgs_itemBond.Any())
            {
                othersTab_of_mgs.AddRange(othersTab_of_mgs_itemBond);
            }

            dataObj.OF_OthersTab_MGS_GII = othersTab_of_mgs;

            #endregion

            #region Others Tab - OF - PDS / CP / NID / BA / BNMN/REPO - Purchase 

            var othersTab_of_pds = new List<MYR_DealCutOffData_OthersTab_Item1>();

            // Sales - from FID Treasury IF MMI tab
            var othersTab_of_pds_item = db.FID_Treasury_MMI
                .Where(x => treasuryFormIds.Contains(x.FormId)
                            && x.CashflowType == Common.Cashflow.Outflow
                            )
                .Select(x => new MYR_DealCutOffData_OthersTab_Item1
                {
                    Bank = x.Issuer,
                    StockCode = x.CertNoStockCode,
                    TradeDate = x.ValueDate,
                    SettlementDate = x.MaturityDate,
                    NominalAmount = x.Nominal,
                    Price = x.Price,
                    Rate = x.SellPurchaseRateYield,
                    Proceed = x.Proceeds,
                    Notes = x.ProductType
                })
                .ToList();

            // coupon - from ISSD TS coupon
            var othersTab_of_pds_itemRepo = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType == Common.TsItemCategory.Coupon || x.InstrumentType == Common.TsItemCategory.Bond || x.InstrumentType == Common.TsItemCategory.Repo)
                            && (!x.InstrumentCode.Contains("MGS") && !x.InstrumentCode.Contains("MGII") && !x.InstrumentCode.Contains("GII"))
                            && (x.OutflowAmount) > 0)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item1
                {
                    Bank = x.InstrumentCode,
                    StockCode = x.StockCode,
                    TradeDate = null,
                    SettlementDate = dataObj.SelectedDate,
                    NominalAmount = null,
                    Price = null,
                    Rate = null,
                    Proceed = x.OutflowAmount,
                    Notes = x.InstrumentType
                })
                .ToList();

            if (othersTab_of_pds_item.Any())
            {
                othersTab_of_pds.AddRange(othersTab_of_pds_item);
            }

            if (othersTab_of_pds_itemRepo.Any())
            {
                othersTab_of_pds.AddRange(othersTab_of_pds_itemRepo);
            }

            dataObj.OF_OthersTab_PssCpNidBaBnm = othersTab_of_pds;

            #endregion

            #region Others Tab - OF - Others

            var othersTab_of_others = new List<MYR_DealCutOffData_OthersTab_Item2>();
            
            var othersTab_of_others_item = db.ISSD_TradeSettlement
                .Where(x => tsFormIds.Contains(x.FormId)
                            && (x.InstrumentType != Common.TsItemCategory.Coupon && x.InstrumentType != Common.TsItemCategory.Equity && x.InstrumentType != Common.TsItemCategory.Bond && x.InstrumentType != Common.TsItemCategory.Repo)
                            && x.OutflowAmount > 0)
                .Select(x => new MYR_DealCutOffData_OthersTab_Item2
                {
                    Items = x.InstrumentCode,
                    Amount = x.OutflowAmount,
                    Notes = x.InstrumentType
                })
                .ToList();

            if (othersTab_of_others_item.Any())
            {
                othersTab_of_others.AddRange(othersTab_of_others_item);
            }

            dataObj.OF_OthersTab_Others = othersTab_of_others;

            #endregion

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

        private void MmiTab_Table(DateTime selectedDate, List<MYR_DealCutOffData_Mmi> items, int startIndex, ref int endIndex, ref Worksheet sheet)
        {
            sheet["E" + startIndex].Value = selectedDate;

            if (items.Any())
            {
                var currentIndex = startIndex += 3;
                
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
                    sheet["L" + currentIndex].Value = item.AssetType;
                    sheet["M" + currentIndex].Value = item.ContactPerson;
                    sheet["N" + currentIndex].Value = item.Notes;
                    
                    currentIndex++;
                }
                
                sheet["G" + currentIndex].Formula = "=SUM($G$" + startIndex + ":$G$" + (currentIndex - 1) + ")";
                sheet["J" + currentIndex].Formula = "=SUM($J$" + startIndex + ":$J$" + (currentIndex - 1) + ")";
                sheet["K" + currentIndex].Formula = "=SUM($K$" + startIndex + ":$K$" + (currentIndex - 1) + ")";

                endIndex = currentIndex;
            }
        }

        private void OthersTab_IfTable1(List<MYR_DealCutOffData_OthersTab_Item1> items, int startIndex, ref int endIndex, ref Worksheet sheet)
        {
            if (items.Any())
            {
                var currentIndex = startIndex += 3;

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

                endIndex = currentIndex;
            }
        }

        private void OthersTab_IfTable2(List<MYR_DealCutOffData_OthersTab_Item2> items, int startIndex, ref int endIndex, ref Worksheet sheet)
        {
            if (items.Any())
            {
                var currentIndex = startIndex += 3;

                foreach (var item in items)
                {
                    if (currentIndex != startIndex)
                    {
                        sheet.Rows[currentIndex - 1].Insert(InsertCellsMode.ShiftCellsDown);
                        sheet.Rows[currentIndex - 1].CopyFrom(sheet.Rows[startIndex - 1], PasteSpecial.All);
                    }

                    sheet["D" + currentIndex].Value = item.Items;
                    sheet["E" + currentIndex].Value = item.Amount;
                    sheet["F" + currentIndex].Value = item.Notes;

                    currentIndex++;
                }

                if (currentIndex != startIndex)
                {
                    sheet["E" + currentIndex].Formula = "=SUM($E$" + startIndex + ":$E$" + (currentIndex - 1) + ")";
                }

                endIndex = currentIndex;
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

        public double IF_FixedIncome_Mgs { get; set; }
        public double IF_FixedIncome_NonMgs { get; set; }
        public double IF_FixedIncome_Total { get; set; }
        

        public double IF_Others_CP { get; set; }
        public double IF_Others_NP { get; set; }
        public double IF_Others_REPO { get; set; }
        public double IF_Others_Fees { get; set; }
        public double IF_Others_Mtm { get; set; }
        public double IF_Others_Fx { get; set; }
        public double IF_Others_Cn { get; set; }
        public double IF_Others_Altid { get; set; }
        public double IF_Others_Others { get; set; }
        public double IF_Others_Total { get; set; }
        
        public double IF_Equity { get; set; }
        public double IF_Net { get; set; }

        public double OF_MM_Rollover { get; set; }
        public double OF_MM_NewPlacement { get; set; }

        public double OF_FixedIncome_Mgs { get; set; }
        public double OF_FixedIncome_NonMgs { get; set; }
        public double OF_FixedIncome_Total { get; set; }

        public double OF_Others_CP { get; set; }
        public double OF_Others_NP { get; set; }
        public double OF_Others_REPO { get; set; }
        public double OF_Others_Fees { get; set; }
        public double OF_Others_Mtm { get; set; }
        public double OF_Others_Fx { get; set; }
        public double OF_Others_Cn { get; set; }
        public double OF_Others_Altid { get; set; }
        public double OF_Others_Others { get; set; }
        public double OF_Others_Total { get; set; }

        public double OF_Equity { get; set; }
        public double OF_Net { get; set; }

        public List<MYR_DealCutOffData_Mmi> IF_MMI_Items { get; set; }
        public List<MYR_DealCutOffData_Mmi> OF_MMI_RolloverItems { get; set; }
        public List<MYR_DealCutOffData_Mmi> OF_MMI_NewPlacementItems { get; set; }

        public List<MYR_DealCutOffData_OthersTab_Item1> IF_OthersTab_MGS_GII { get; set; }
        public List<MYR_DealCutOffData_OthersTab_Item1> IF_OthersTab_PssCpNidBaBnm { get; set; }
        public List<MYR_DealCutOffData_OthersTab_Item2> IF_OthersTab_Others { get; set; }

        public List<MYR_DealCutOffData_OthersTab_Item1> OF_OthersTab_MGS_GII { get; set; }
        public List<MYR_DealCutOffData_OthersTab_Item1> OF_OthersTab_PssCpNidBaBnm { get; set; }
        public List<MYR_DealCutOffData_OthersTab_Item2> OF_OthersTab_Others { get; set; }

    }

    public class MYR_DealCutOffData_OthersTab_Item1
    {
        public string Bank { get; set; }
        public string StockCode { get; set; }
        public DateTime? TradeDate { get; set; }
        public DateTime? SettlementDate { get; set; }
        public double? NominalAmount { get; set; }
        public double? Price { get; set; }
        public double? Rate { get; set; }
        public double? Proceed { get; set; }
        public string Notes { get; set; }
    }

    public class MYR_DealCutOffData_OthersTab_Item2
    {
        public string Items { get; set; }
        public double? Amount { get; set; }
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
        public string AssetType { get; set; }
    }

}