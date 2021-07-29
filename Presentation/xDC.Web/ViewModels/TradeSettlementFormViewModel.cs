using System;

namespace xDC_Web.ViewModels
{
    public class TradeSettlementFormViewModel
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public DateTime? FormDate { get; set; }
        public string FormCurrency { get; set; }

        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string AdminEditedBy { get; set; }
        public DateTime? AdminEditedDate { get; set; }

        public bool IsApprovedOrRejected { get; set; }
        public string ApprovalOrRejectionNotes { get; set; }



        public bool EnableDraftButton { get; set; }
        public bool EnableSaveAdminChanges { get; set; }



        public bool IsAdminEdited { get; set; }
        public bool ApprovePermission { get; set; }
        public bool AdminEditPermission { get; set; }

        
    }
}