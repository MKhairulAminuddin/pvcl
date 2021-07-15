using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.Models
{
    public class ViewInflowFundStatusForm
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public bool ApprovePermission { get; set; }
    }

    public class ViewTradeSettlementForm
    {
        public int Id { get; set; }
        public string FormStatus { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public bool ApprovePermission { get; set; }
    }
}