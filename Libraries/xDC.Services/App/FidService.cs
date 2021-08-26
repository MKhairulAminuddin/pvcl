using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;

namespace xDC.Services.App
{
    public static class FidService
    {
        public static bool IsTodayTs10AmExist(kashflowDBEntities db, string currency)
        {
            // check for same date and same currency exist
            var isExist = db.FID_TS10.Any(x =>
                DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(DateTime.Now) && x.Currency == currency);
            return isExist;
        }

        public static bool IsTs10AmExist(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            // check for same date and same currency exist
            var isExist = db.FID_TS10.Any(x =>
                DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate) && x.Currency == currency);
            return isExist;
        }

        public static List<EDW_FID_List> List_Issuer(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var issuerList = db.EDW_FID_List.Where(x => x.Type == "ISSUER").ToList();
            return issuerList;
        }

        public static List<EDW_FID_List> List_CounterParty(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var counterPartyList = db.EDW_FID_List.Where(x => x.Type == "COUNTER PARTY").ToList();
            return counterPartyList;
        }
    }
}
