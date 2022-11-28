using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.AMSD.InflowFundForm
{
    public class IfItemFormPage
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FundType { get; set; }
        public string Bank { get; set; }
        public double Amount { get; set; }
        public Nullable<System.DateTime> LatestSubmission { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
