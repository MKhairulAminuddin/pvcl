using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;

namespace xDC_Web.Extension.SchedulerTask
{
    public class FidTask
    {
        public static void FetchToday10AmTradeSettlement()
        {
            try
            {
                Logger.LogInfo("Fetch 10am Trade Settlement Started!");

                using (var db = new kashflowDBEntities())
                {
                    var today = DateTime.Now.Date;

                    var tsForms = db.ISSD_FormHeader.Where(x =>
                        x.FormStatus == Common.FormStatus.Approved &&
                        DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(today)).ToList();

                    Fetch10AmTradeSettlement(db, tsForms);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                Logger.LogInfo("Fetch 10am Trade Settlement Completed!");
            }
        }

        public static void Fetch10AmTradeSettlementOnDemand()
        {
            try
            {
                Logger.LogInfo("Fetch 10am Trade Settlement Started!");

                using (var db = new kashflowDBEntities())
                {
                    var today = DateTime.Now.Date;

                    var tsForms = db.ISSD_FormHeader.Where(x =>
                        x.FormStatus == Common.FormStatus.Approved &&
                        DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(today)).ToList();

                    Fetch10AmTradeSettlement(db, tsForms);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                Logger.LogInfo("Fetch 10am Trade Settlement Completed!");
            }
        }

        private static void Fetch10AmTradeSettlement(kashflowDBEntities db,List<ISSD_FormHeader> tsForms)
        {
            if (tsForms.Any())
            {
                var tsFormsGrouped = tsForms.Where(x => x.SettlementDate != null).GroupBy(x => new { x.SettlementDate, x.Currency }).ToList();

                foreach (var tsForm in tsFormsGrouped)
                {
                    if (!FidService.IsTs10AmExist(db, tsForm.Key.SettlementDate.Value, tsForm.Key.Currency))
                    {
                        var currentSettlementDate = tsForm.Key.SettlementDate.Value;
                        var currentCurrency = tsForm.Key.Currency;

                        var ts10am = new FID_TS10()
                        {
                            FormStatus = Common.FormStatus.Approved,
                            Currency = currentCurrency,
                            FormType = Common.FormTypeMapping(2),
                            SettlementDate = currentSettlementDate
                        };
                        db.FID_TS10.Add(ts10am);
                        db.SaveChanges();

                        foreach (var form in tsForms.Where(x => x.Currency == currentCurrency && x.SettlementDate.Value.Date == currentSettlementDate))
                        {
                            var tsTradeItems = db.ISSD_TradeSettlement.Where(x => x.FormId == form.Id).ToList();
                            if (tsTradeItems.Any())
                            {
                                foreach (var item in tsTradeItems)
                                {
                                    var tradeItem = new FID_TS10_TradeItem
                                    {
                                        FormId = ts10am.Id,
                                        InstrumentType = item.InstrumentType,
                                        InstrumentCode = item.InstrumentCode,
                                        StockCode = item.StockCode,
                                        Maturity = item.Maturity,
                                        Sales = item.Sales,
                                        Purchase = item.Purchase,
                                        FirstLeg = item.FirstLeg,
                                        SecondLeg = item.SecondLeg,
                                        AmountPlus = item.AmountPlus,
                                        AmountMinus = item.AmountMinus,
                                        Remarks = item.Remarks,
                                        CreatedBy = item.ModifiedBy,
                                        CreatedDate = item.ModifiedDate,

                                        InflowTo = (currentCurrency == "MYR") ? "RENTAS" : null,
                                        OutflowFrom = (currentCurrency == "MYR") ? "RENTAS" : null
                                    };
                                    db.FID_TS10_TradeItem.Add(tradeItem);
                                }
                                db.SaveChanges();
                            }

                            var tsApprovals = db.Form_Workflow
                                .Where(x => x.FormId == form.Id && x.WorkflowStatus == Common.FormStatus.Approved)
                                .OrderByDescending(x => x.RecordedDate).FirstOrDefault();
                            if (tsApprovals != null)
                            {
                                var approval = new FID_TS10_Approval
                                {
                                    FormId = ts10am.Id,
                                    FormType = tsApprovals.FormType,
                                    ApprovedBy = tsApprovals.RequestBy,
                                    ApprovedDate = tsApprovals.RecordedDate
                                };
                                db.FID_TS10_Approval.Add(approval);
                                db.SaveChanges();
                            }

                        }
                    }
                    else
                    {
                        var existing = db.FID_TS10.FirstOrDefault(x =>
                            DbFunctions.TruncateTime(x.SettlementDate) ==
                            DbFunctions.TruncateTime(tsForm.Key.SettlementDate.Value) &&
                            x.Currency == tsForm.Key.Currency);

                        //cleanup
                        if (existing != null)
                        {
                            var existingTradeItems = db.FID_TS10_TradeItem.Where(x => x.FormId == existing.Id);
                            if (existingTradeItems != null)
                            {
                                db.FID_TS10_TradeItem.RemoveRange(existingTradeItems);
                            }

                            db.FID_TS10.Remove(existing);
                            db.SaveChanges();
                        }

                        // add balik
                        var currentSettlementDate = tsForm.Key.SettlementDate.Value;
                        var currentCurrency = tsForm.Key.Currency;

                        var ts10am = new FID_TS10()
                        {
                            FormStatus = Common.FormStatus.Approved,
                            Currency = currentCurrency,
                            FormType = Common.FormType.ISSD_TS,
                            SettlementDate = currentSettlementDate
                        };
                        db.FID_TS10.Add(ts10am);
                        db.SaveChanges();

                        foreach (var form in tsForms.Where(x => x.Currency == currentCurrency && x.SettlementDate.Value.Date == currentSettlementDate))
                        {
                            var tsTradeItems = db.ISSD_TradeSettlement.Where(x => x.FormId == form.Id).ToList();
                            if (tsTradeItems.Any())
                            {
                                foreach (var item in tsTradeItems)
                                {
                                    var tradeItem = new FID_TS10_TradeItem
                                    {
                                        FormId = ts10am.Id,
                                        InstrumentType = item.InstrumentType,
                                        InstrumentCode = item.InstrumentCode,
                                        StockCode = item.StockCode,
                                        Maturity = item.Maturity,
                                        Sales = item.Sales,
                                        Purchase = item.Purchase,
                                        FirstLeg = item.FirstLeg,
                                        SecondLeg = item.SecondLeg,
                                        AmountPlus = item.AmountPlus,
                                        AmountMinus = item.AmountMinus,
                                        Remarks = item.Remarks,
                                        CreatedBy = item.ModifiedBy,
                                        CreatedDate = item.ModifiedDate,

                                        InflowTo = (currentCurrency == "MYR") ? "RENTAS" : null,
                                        OutflowFrom = (currentCurrency == "MYR") ? "RENTAS" : null
                                    };
                                    db.FID_TS10_TradeItem.Add(tradeItem);
                                }
                                db.SaveChanges();
                            }

                            var tsApprovals = db.Form_Workflow
                                .Where(x => x.FormId == form.Id && x.WorkflowStatus == Common.FormStatus.Approved)
                                .OrderByDescending(x => x.RecordedDate).FirstOrDefault();
                            if (tsApprovals != null)
                            {
                                var approval = new FID_TS10_Approval
                                {
                                    FormId = ts10am.Id,
                                    FormType = tsApprovals.FormType,
                                    ApprovedBy = tsApprovals.RequestBy,
                                    ApprovedDate = tsApprovals.RecordedDate
                                };
                                db.FID_TS10_Approval.Add(approval);
                                db.SaveChanges();
                            }

                        }
                    }
                }
            }
        }

        public static void FetchAssetType()
        {
            try
            {
                Logger.LogInfo("Fetch Asset Type Started!");

                using (var db = new kashflowDBEntities())
                {
                    var assetTypeFromEdw = db.EDW_Maturity.Select(x => x.Asset_Type).Distinct().ToList();

                    foreach (var assetType in assetTypeFromEdw)
                    {
                        if (!db.Config_Dropdown.Any(x => x.Key == Common.DropdownConfigKey.FID_Treasury_AssetType && x.Value == assetType))
                        {
                            var newAssetType = new Config_Dropdown
                            {
                                Key = Common.DropdownConfigKey.FID_Treasury_AssetType,
                                Value = assetType,
                                CreatedBy = "System",
                                CreatedDate = DateTime.Now,
                            };
                            db.Config_Dropdown.Add(newAssetType);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                Logger.LogInfo("Fetch Asset Type Completed!");
            }
        }
    }
}