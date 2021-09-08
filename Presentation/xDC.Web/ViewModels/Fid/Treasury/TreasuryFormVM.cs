﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Fid.Treasury
{
    public class TreasuryFormVM
    {
        public int Id { get; set; }
        public DateTime? TradeDate { get; set; }
        public string Currency { get; set; }
        public string FormStatus { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalNotes { get; set; }


        public bool EnableApproveBtn { get; set; }
        public bool EnableRejectBtn { get; set; }

        public bool EnableSubmitForApproval { get; set; }
        public bool EnableSaveAsDraftBtn { get; set; }
    }
}