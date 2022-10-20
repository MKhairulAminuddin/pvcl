using System.Collections.Generic;
using xDC.Infrastructure.Application;

namespace xDC_Web.Models
{
    public class InflowFundsModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public bool IsSaveAsDraft { get; set; }
        public bool IsSaveAdminEdit { get; set; }

        public List<AMSD_IF_Item> AmsdInflowFunds { get; set; }
        public string Approver { get; set; }
        public string ApprovalNotes { get; set; }
    }
}