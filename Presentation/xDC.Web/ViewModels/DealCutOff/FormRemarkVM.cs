using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.DealCutOff
{
    public class FormRemarkVM
    {
        public int FormId { get; set; }
        public string FormType { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? FormDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Remarks { get; set; }
        
    }
}