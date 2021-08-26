using System;

namespace xDC_Web.ViewModels.Fid.Treasury
{
    public class TreasuryGridVm
    {
        public int Id { get; set; }
        public DateTime? TradeDate { get; set; }
        public string Currency { get; set; }
        public string FormStatus { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }


        public bool IsEditAllowed { get; set; }
        public bool IsDeleteAllowed { get; set; }
        public bool IsViewAllowed { get; set; }


        public bool IsPendingMyApproval { get; set; }
        public bool IsMyFormRejected { get; set; }
        public bool IsPendingApproval { get; set; }
    }
}