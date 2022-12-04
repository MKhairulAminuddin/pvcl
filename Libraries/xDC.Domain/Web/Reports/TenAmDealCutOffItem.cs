using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.Reports
{
    public class TenAmDealCutOffItem
    {
        public string Id { get; set; }
        public string Currency { get; set; }
        public string Account { get; set; }
        public string AccountX { get; set; }
        public double OpeningBalance { get; set; }
        public double TotalInflow { get; set; }
        public double TotalOutflow { get; set; }
        public double Net { get; set; }

        public double ClosingBalance { get; set; }
        public DateTime? ClosingBalanceModifiedDate { get; set; }
        public string ClosingBalanceModifiedBy { get; set; }
    }
}
