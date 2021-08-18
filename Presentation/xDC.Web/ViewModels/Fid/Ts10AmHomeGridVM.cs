﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Fid
{
    public class Ts10AmHomeGridVM
    {
        public int FormId { get; set; }
        public string Currency { get; set; }
        public DateTime SettlementDate { get; set; }
        public int CountPendingEquity { get; set; }
        public int CountPendingBond { get; set; }
        public int CountPendingCp { get; set; }
        public int CountPendingNotesPapers { get; set; }
        public int CountPendingRepo { get; set; }
        public int CountPendingCoupon { get; set; }
        public int CountPendingFees { get; set; }
        public int CountPendingMtm { get; set; }
        public int CountPendingFx { get; set; }
        public int CountPendingContribution { get; set; }
        public int CountPendingAltid { get; set; }
        public int CountPendingOthers { get; set; }
    }
}