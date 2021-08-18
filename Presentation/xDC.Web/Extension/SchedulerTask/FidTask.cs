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
        public static void Fetch10AmTradeSettlement()
        {
            try
            {
                Logger.LogInfo("Fetch 10am Trade Settlement Started!");

                using (var db = new kashflowDBEntities())
                {
                    
                    var todayDate = DateTime.Now.Date;
                    TimeSpan cutOffTime = new TimeSpan(10, 00, 0);

                    var todayTradeSettlementForm = db.ISSD_FormHeader.Where(x =>
                        x.FormStatus == "Approved" && DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(todayDate) &&
                        DbFunctions.CreateTime(x.ApprovedDate.Value.Hour, x.ApprovedDate.Value.Minute, x.ApprovedDate.Value.Second) <= cutOffTime).ToList();

                    if (todayTradeSettlementForm.Any())
                    {
                        var availableCurrencies = todayTradeSettlementForm.Select(x => x.Currency).Distinct();

                        foreach (var currentCurrency in availableCurrencies)
                        {
                            if (!FidService.IsTodayTs10AmExist(db, currentCurrency))
                            {
                                var ts10am = new FID_TS10()
                                {
                                    FormStatus = "Approved",
                                    Currency = todayTradeSettlementForm.First(x => x.Currency == currentCurrency).Currency,
                                    FormType = Common.FormTypeMapping(2),
                                    SettlementDate = todayTradeSettlementForm.First(x => x.Currency == currentCurrency).SettlementDate
                                };
                                db.FID_TS10.Add(ts10am);
                                db.SaveChanges();

                                foreach (var form in todayTradeSettlementForm.Where(x => x.Currency == currentCurrency))
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
                                    }
                                    db.SaveChanges();

                                    var tsApprovals = db.Form_Workflow
                                        .Where(x => x.FormId == form.Id && x.WorkflowStatus == "Approved")
                                        .OrderByDescending(x => x.EndDate).FirstOrDefault();
                                    if (tsApprovals != null)
                                    {
                                        var approval = new FID_T10_Approval
                                        {
                                            FormId = ts10am.Id,
                                            FormType = tsApprovals.FormType,
                                            ApprovedBy = tsApprovals.RequestBy,
                                            ApprovedDate = tsApprovals.EndDate
                                        };
                                        db.FID_T10_Approval.Add(approval);
                                        db.SaveChanges();
                                    }

                                }
                            }
                        }
                    }
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
    }
}