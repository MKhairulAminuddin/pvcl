using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.Models
{
    public class InflowFundStatusFormVM
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        

        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        

        public bool IsAdminEdited { get; set; }
        public string AdminEditedBy { get; set; }
        public DateTime? AdminEditedDate { get; set; }

        public bool ApprovePermission { get; set; }
        public bool EnableAdminEditBtn { get; set; }
        public bool EnableSaveAsDraftBtn { get; set; }
        public bool EnableReassign { get; set; }

        public bool IsApprovedOrRejected { get; set; }
        public string ApprovalOrRejectionNotes { get; set; }
    }
}