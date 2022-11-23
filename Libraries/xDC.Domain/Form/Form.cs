using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Form
{
    public class Form
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public string Currency { get; set; }

        public Nullable<System.DateTime> FormDate { get; set; }
        public Nullable<System.DateTime> PreparedDate { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public Nullable<System.DateTime> AdminEdittedDate { get; set; }

        public string PreparedBy { get; set; }
        public string ApprovedBy { get; set; }
        public bool AdminEditted { get; set; }
        public string AdminEdittedBy { get; set; }
        
    }
}
