using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Utils;

namespace xDC.Services.Form
{
    public static class FcaTaggingSvc
    {
        public static List<TsOpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var result = new List<TsOpeningBalance>();

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
                result.Add(new TsOpeningBalance()
                {
                    Account = item.account,
                    Amount = item.total
                });
            }

            return result;
        }

        public static List<TsOpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate)
        {
            var result = new List<TsOpeningBalance>();

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
                result.Add(new TsOpeningBalance()
                {
                    Account = item.account,
                    Currency = item.currency,
                    Amount = item.total
                });
            }

            return result;
        }

        public static double GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency, string account)
        {
            var result = new List<TsOpeningBalance>();

            var ob = db.EDW_BankBalance
                .AsNoTracking()
                .Where(x =>
                    DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                    && x.Currency == currency
                    && x.InstrumentType == account)
                .Select(x => x.Amount ?? 0)
                .DefaultIfEmpty(0)
                .Sum();

            return ob;
        }

        public static double TotalInflow(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var approvedFormIds = db.ISSD_FormHeader
                .Where(x => DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(settlementDate)
                            && x.Currency == currency)
                .Select(x => x.Id)
                .ToList();

            var tradeSettlement = db.ISSD_TradeSettlement
                .Where(x => approvedFormIds.Contains(x.FormId))
                .Select(x => x.InflowAmount)
                .DefaultIfEmpty(0)
                .Sum();

            return tradeSettlement;
        }

        public static double TotalOutflow(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var approvedFormIds = db.ISSD_FormHeader
                .Where(x => DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(settlementDate)
                            && x.Currency == currency)
                .Select(x => x.Id)
                .ToList();

            var tradeSettlement = db.ISSD_TradeSettlement
                .Where(x => approvedFormIds.Contains(x.FormId))
                .Select(x => x.OutflowAmount)
                .DefaultIfEmpty(0)
                .Sum();

            return tradeSettlement;
        }
    }
}
