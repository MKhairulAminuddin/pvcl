using System;
using System.Collections.Generic;
using DevExpress.Office.Utils;
using xDC.Domain.ISSD_TS;

namespace xDC_Web.ViewModels
{
    public class TradeSettlementFormVM
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string Currency { get; set; }

        public List<TS_OpeningBalance> OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalNote { get; set; }
        public string AdminEditedBy { get; set; }
        public DateTime? AdminEditedDate { get; set; }
        
        public bool EnableDraftButton { get; set; }
        public bool EnableSaveAdminChanges { get; set; }
        public bool EnableResubmit { get; set; }
        public bool EnableSubmitForApproval { get; set; }
        public bool EnableApproveRejectBtn { get; set; }

        
        public bool IsAdminEdited { get; set; }
        public bool ApprovePermission { get; set; }
        public bool AdminEditPermission { get; set; }
    }
}