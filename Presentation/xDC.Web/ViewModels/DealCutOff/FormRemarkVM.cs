using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.DealCutOff
{
    public class FormListVM
    {
        public int FormId { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public string FormCurrency { get; set; }
        public string PreparedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? FormDate { get; set; }
        public DateTime? ApprovalDate { get; set; }

    }

    public class FormRemarkVM
    {
        public int FormId { get; set; }
        public string FormType { get; set; }
        public string ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }
        public string Remarks { get; set; }
        
    }
}