using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.ISSD.TradeSettlementForm;

namespace xDC.Domain.Web.FID.FcaTaggingForm
{
    public class FcaAccountAssignmentFormVM
    {
        public string Currency { get; set; }
        public DateTime SettlementDate { get; set; }
        public List<TsOpeningBalance> OpeningBalance { get; set; }
        public double ClosingBalance { get; set; }
    }
}
