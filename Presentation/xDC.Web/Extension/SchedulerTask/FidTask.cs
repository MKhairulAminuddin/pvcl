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
                    
                    var todayDate = DateTime.Now.Date;
                    TimeSpan cutOffTime = new TimeSpan(10, 00, 0);

                    var tsForms = db.ISSD_FormHeader.Where(x =>
                        x.FormStatus == "Approved" && DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(todayDate) &&
                        DbFunctions.CreateTime(x.ApprovedDate.Value.Hour, x.ApprovedDate.Value.Minute, x.ApprovedDate.Value.Second) <= cutOffTime).ToList();

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
                    TimeSpan cutOffTime = new TimeSpan(10, 00, 0);

                    var tsForms = db.ISSD_FormHeader.Where(x =>
                        x.FormStatus == "Approved" &&
                        DbFunctions.CreateTime(x.ApprovedDate.Value.Hour, x.ApprovedDate.Value.Minute, x.ApprovedDate.Value.Second) <= cutOffTime).ToList();

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
                            FormStatus = "Approved",
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


                            var tsOpBalance = db.ISSD_Balance.Where(x => x.FormId == form.Id).ToList();
                            if (tsOpBalance.Any())
                            {
                                foreach (var item in tsOpBalance)
                                {
                                    var opBalance = new FID_TS10_OpeningBalance
                                    {
                                        FormId = ts10am.Id,
                                        Account = item.BalanceCategory,
                                        Amount = item.Amount,
                                        CreatedBy = item.CreatedBy,
                                        CreatedDate = item.CreatedDate,
                                    };
                                    db.FID_TS10_OpeningBalance.Add(opBalance);
                                }
                                db.SaveChanges();
                            }

                            var tsApprovals = db.Form_Workflow
                                .Where(x => x.FormId == form.Id && x.WorkflowStatus == "Approved")
                                .OrderByDescending(x => x.EndDate).FirstOrDefault();
                            if (tsApprovals != null)
                            {
                                var approval = new FID_TS10_Approval
                                {
                                    FormId = ts10am.Id,
                                    FormType = tsApprovals.FormType,
                                    ApprovedBy = tsApprovals.RequestBy,
                                    ApprovedDate = tsApprovals.EndDate
                                };
                                db.FID_TS10_Approval.Add(approval);
                                db.SaveChanges();
                            }

                        }
                    }
                }
            }
        }
    }
}