using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels
{
    public class NotificationConfigViewModel
    {
        public DateTime InflowFundCutOffTime { get; set; }
        public List<string> TradeSettlementContributionEmail { get; set; }

    }
}