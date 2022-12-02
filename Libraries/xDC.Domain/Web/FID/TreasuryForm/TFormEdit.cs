using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.FID.TreasuryForm
{
    public class TFormEdit
    {
        public int Id { get; set; }
        public DateTime? ValueDate { get; set; }
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
        public bool EnableResubmitBtn { get; set; }
        public bool EnableSaveAsDraftBtn { get; set; }
        public bool EnableReassign { get; set; }
    }
}
