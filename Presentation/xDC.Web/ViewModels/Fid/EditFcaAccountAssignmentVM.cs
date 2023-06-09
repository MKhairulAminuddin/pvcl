﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.ISSD.TradeSettlementForm;

namespace xDC_Web.ViewModels.Fid
{
    public class EditFcaAccountAssignmentVM
    {
        public string Currency { get; set; }
        public DateTime SettlementDate { get; set; }
        public List<TsOpeningBalance> OpeningBalance { get; set; }
        public double ClosingBalance { get; set; }
    }
}