using System;
using System.Collections.Generic;
using DevExpress.Office.Utils;

namespace xDC_Web.ViewModels
{
    public class ViewTradeSettlementFormViewModel
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string Currency { get; set; }

        public List<OpeningBalanceTsFormVM> OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

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

    public class OpeningBalanceTsFormVM
    {
        public string Account { get; set; }
        public double Amount { get; set; }
    }

    public class EditTradeSettlementFormViewModel
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string Currency { get; set; }
        public decimal OpeningBalanceRentas { get; set; }
        public decimal OpeningBalanceMma { get; set; }

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
        public bool EnableResubmit { get; set; }
        public bool EnableSubmitForApproval { get; set; }


        public bool IsAdminEdited { get; set; }
        public bool ApprovePermission { get; set; }
        public bool AdminEditPermission { get; set; }


    }
}