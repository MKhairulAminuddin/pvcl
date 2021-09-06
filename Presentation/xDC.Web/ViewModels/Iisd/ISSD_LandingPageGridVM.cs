using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Iisd
{
    public class ISSD_LandingPageGridVM
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public DateTime? FormDate { get; set; }
        public string Currency { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }


        public bool EnableEdit { get; set; }
        public bool EnableDelete { get; set; }
        public bool EnablePrint { get; set; }

        public bool IsPendingMyApproval { get; set; }
        public bool IsRejected { get; set; }
        public bool IsPendingApproval { get; set; }

    }
}