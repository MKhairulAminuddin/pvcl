using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.ISSD.TradeSettlementForm
{
    public class TsConsolidatedPage
    {
        public string Currency { get; set; }
        public DateTime SettlementDate { get; set; }

        public int TotalEquity { get; set; }
        public int TotalBond { get; set; }
        public int TotalCp { get; set; }
        public int TotalNotesPapers { get; set; }
        public int TotalRepo { get; set; }
        public int TotalCoupon { get; set; }
        public int TotalFees { get; set; }
        public int TotalMtm { get; set; }
        public int TotalFx { get; set; }
        public int TotalCn { get; set; }
        public int TotalAltid { get; set; }
        public int TotalOthers { get; set; }

        public int? FormIdEquity { get; set; }
        public int? FormIdBond { get; set; }
        public int? FormIdCp { get; set; }
        public int? FormIdNotesPapers { get; set; }
        public int? FormIdRepo { get; set; }
        public int? FormIdCoupon { get; set; }
        public int? FormIdFees { get; set; }
        public int? FormIdMtm { get; set; }
        public int? FormIdFx { get; set; }
        public int? FormIdCn { get; set; }
        public int? FormIdAltid { get; set; }
        public int? FormIdOthers { get; set; }
    }
}
