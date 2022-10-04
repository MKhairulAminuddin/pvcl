using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.ISSD.TradeSettlementForm
{
    public class TsOpeningBalance
    {
        public string Account { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}
