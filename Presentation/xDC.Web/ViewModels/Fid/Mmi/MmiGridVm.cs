using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Fid.Mmi
{
    public class MmiGridVm
    {
        public int FormId { get; set; }
        public DateTime? TradeDate { get; set; }
        public string Currency { get; set; }
        public string FormStatus { get; set; }
        public string Preparer { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string Approver { get; set; }
        public DateTime? ApprovedDate { get; set; }


        public bool IsEditAllowed { get; set; }
        public bool IsDeleteAllowed { get; set; }
        public bool IsViewAllowed { get; set; }


        public bool IsPendingMyApproval { get; set; }
        public bool IsMyFormRejected { get; set; }
        public bool IsPendingApproval { get; set; }
    }
}