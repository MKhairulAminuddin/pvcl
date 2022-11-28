using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.ISSD.TradeSettlementForm
{
    public class TsForm
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public string FormType { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string Currency { get; set; }

        public List<TsOpeningBalance> OpeningBalance { get; set; }
        public double ClosingBalance { get; set; }

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
        public bool EnableReassign { get; set; }


        public bool IsAdminEdited { get; set; }
    }
}
