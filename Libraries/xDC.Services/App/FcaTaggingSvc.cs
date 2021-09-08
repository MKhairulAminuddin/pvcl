using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.ISSD_TS;
using xDC.Infrastructure.Application;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class FcaTaggingSvc
    {
        public static List<TS_OpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var result = new List<TS_OpeningBalance>();

            var ob = db.EDW_BankBalance
                .AsNoTracking()
                .Where(x =>
                    DbFunctions.TruncateTime(x.SettlementDate) ==
                    DbFunctions.TruncateTime(settlementDate) && x.Currency == currency)
                .GroupBy(x => new { x.SettlementDate, x.InstrumentType })
                .Select(x => new
                {
                    account = x.Key.InstrumentType,
                    total = x.Sum(y => y.Amount ?? 0)
                });

            foreach (var item in ob)
            {
                result.Add(new TS_OpeningBalance()
                {
                    Account = item.account,
                    Amount = (decimal)item.total
                });
            }

            return result;
        }

        public static List<TS_OpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate)
        {
            var result = new List<TS_OpeningBalance>();

            var ob = db.EDW_BankBalance
                .AsNoTracking()
                .Where(x =>
                    DbFunctions.TruncateTime(x.SettlementDate) ==
                    DbFunctions.TruncateTime(settlementDate))
                .GroupBy(x => new { x.SettlementDate, x.InstrumentType, x.Currency })
                .Select(x => new
                {
                    account = x.Key.InstrumentType,
                    currency = x.Key.Currency,
                    total = x.Sum(y => y.Amount ?? 0)
                });

            foreach (var item in ob)
            {
                result.Add(new TS_OpeningBalance()
                {
                    Account = item.account,
                    Currency = item.currency,
                    Amount = (decimal)item.total
                });
            }

            return result;
        }

        public static TS_TotalFlow GetTotalFlow(kashflowDBEntities db, int formId, DateTime settlementDate, string currency)
        {
            var trades = new List<FID_TS10_TradeItem>();
            
            var tradesFromId = db.FID_TS10_TradeItem.Where(x => x.FormId == formId).ToList();
            if (tradesFromId.Any())
            {
                trades.AddRange(tradesFromId);
            }

            var totalFlow = trades.GroupBy(x => 1)
                .Select(x => new
                {
                    totalMaturity = x.Sum(y => y.Maturity??0),
                    totalSales = x.Sum(y => y.Sales ?? 0),
                    totalFirstLeg = x.Sum(y => y.FirstLeg ?? 0),
                    totalAmountPlus = x.Sum(y => y.AmountPlus ?? 0),

                    totalPurchase = x.Sum(y => y.Purchase ?? 0),
                    totalSecondLeg = x.Sum(y => y.SecondLeg ?? 0),
                    totalAmountMinus = x.Sum(y => y.AmountMinus ?? 0)
                })
                .FirstOrDefault();

            decimal cbInflow = 0;
            decimal cbOutflow = 0;

            if (totalFlow != null)
            {
                cbInflow = totalFlow.totalMaturity + totalFlow.totalSales + totalFlow.totalFirstLeg +
                           totalFlow.totalAmountPlus;
                cbOutflow = totalFlow.totalPurchase + totalFlow.totalSecondLeg + totalFlow.totalAmountMinus;
            }
             
            var result = new TS_TotalFlow()
            {
                Currency = currency,
                SettlementDate = settlementDate,
                Inflow = cbInflow,
                Outflow = cbOutflow
            };

            return result;
        }
    }
}
