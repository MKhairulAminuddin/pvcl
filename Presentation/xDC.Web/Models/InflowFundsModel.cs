using System.Collections.Generic;
using xDC.Infrastructure.Application;

namespace xDC_Web.Models
{
    public class InflowFundsModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public List<AMSD_InflowFund> AmsdInflowFunds { get; set; }
        public string Approver { get; set; }
    }

    public class FormApprovalModel
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string ApprovalNote { get; set; }
        public bool ApprovalStatus { get; set; }
    }

    public class ApprovalTradeSettlementModel
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string ApprovalNote { get; set; }
        public bool ApprovalStatus { get; set; }
    }
}