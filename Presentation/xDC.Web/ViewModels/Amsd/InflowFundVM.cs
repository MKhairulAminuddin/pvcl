using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Amsd
{
    public class InflowFundVM
    {
        public int Id { get; set; }
        public List<AmsdInflowFundVM> AmsdInflowFunds { get; set; }

        public InflowFundVM()
        {
            this.AmsdInflowFunds = new List<AmsdInflowFundVM>();
        }
    }

    public class AmsdInflowFundVM
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FundType { get; set; }
        public string Bank { get; set; }
        public double Amount { get; set; }
        public DateTime LatestSubmission { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}