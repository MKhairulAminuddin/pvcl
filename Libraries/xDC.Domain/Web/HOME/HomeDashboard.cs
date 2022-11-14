using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;

namespace xDC.Domain.Web.HOME
{
    public class HomeDashboard
    {
        public List<IfFormSummaryList> IfFormSummary { get; set; }
        public List<IfAmountSummary> IfAmountSummary { get; set; }
        public List<TsOpeningBalance> TsOpeningBalanceSummary { get; set; }
        public List<TsFormSummary> TsFormSummary { get; set; }

        public HomeDashboard()
        {
            this.IfFormSummary = new List<IfFormSummaryList>();
            this.IfAmountSummary = new List<IfAmountSummary>();
            this.TsOpeningBalanceSummary = new List<TsOpeningBalance>();
            this.TsFormSummary = new List<TsFormSummary>();
        }
    }
}
