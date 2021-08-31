using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.ISSD_TS
{
    public class TS_TotalFlow
    {
        public DateTime SettlementDate { get; set; }
        public string Currency { get; set; }
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
    }
}
