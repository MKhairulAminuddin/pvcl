using System.Collections.Generic;

namespace xDC.Domain.Web.AMSD.InflowFundForm
{
    public class IfFormPage
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public bool IsSaveAsDraft { get; set; }
        public bool IsSaveAdminEdit { get; set; }

        public List<IfItemFormPage> IfItems { get; set; }
        public string Approver { get; set; }
        public string ApprovalNotes { get; set; }
    }
}
