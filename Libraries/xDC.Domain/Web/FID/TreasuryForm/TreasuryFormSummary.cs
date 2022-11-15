using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.ISSD.TradeSettlementForm
{
    public class TreasuryFormSummary
    {
        public int FormId { get; set; }
        public string PreparedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string Currency { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string FormStatus { get; set; }
    }
}
