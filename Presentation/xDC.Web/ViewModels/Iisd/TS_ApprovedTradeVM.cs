using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Iisd
{
    public class TS_ApprovedTradeVM
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
        public int TotalContribution { get; set; }
        public int TotalAltid { get; set; }
        public int TotalOthers { get; set; }
    }
}