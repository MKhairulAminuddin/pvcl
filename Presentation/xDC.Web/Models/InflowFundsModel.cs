using System.Collections.Generic;

namespace xDC_Web.Models
{
    public class InflowFundsModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public List<AmsdInflowFunds> AmsdInflowFunds { get; set; }
        public string Approver { get; set; }
    }

    public class ApprovalInflowFundsModel
    {
        public int FormId { get; set; }
        public string ApprovalNote { get; set; }
        public bool ApprovalStatus { get; set; }
    }
}