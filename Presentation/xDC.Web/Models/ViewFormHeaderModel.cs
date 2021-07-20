using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.Models
{
    public class ViewFormHeaderModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public string Currency { get; set; }
        public string PreparedBy { get; set; }
        public Nullable<System.DateTime> PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public bool AdminEditted { get; set; }
        public string AdminEdittedBy { get; set; }
        public Nullable<System.DateTime> AdminEdittedDate { get; set; }

        public bool IsFormOwner { get; set; }

        public bool IsCanAdminEdit { get; set; }
        public bool IsResubmitEnabled { get; set; }
    }
}