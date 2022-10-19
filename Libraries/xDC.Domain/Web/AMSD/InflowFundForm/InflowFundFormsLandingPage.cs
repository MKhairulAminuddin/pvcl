using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.AMSD.InflowFundForm
{
    public class InflowFundFormsLandingPage
    {
        public int CountTodaySubmission { get; set; }
        public int CountTodayPendingApproval { get; set; }
        public int CountTodayApproved { get; set; }
        public int CountTodayRejected { get; set; }
        public bool EnableCreateForm { get; set; }
        public bool EnableRetractSubmission { get; set; }
    }
}
