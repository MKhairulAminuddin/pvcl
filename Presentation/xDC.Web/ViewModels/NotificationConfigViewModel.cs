using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels
{
    public class NotificationConfigViewModel
    {
        public DateTime InflowFundCutOffTime { get; set; }
        public List<string> tsCnEmail { get; set; }
        public List<string> tsCnEmailCc { get; set; }
        public List<string> tsPeEmail { get; set; }
        public List<string> tsPeEmailCc { get; set; }
        public List<string> tsPropertyEmail { get; set; }
        public List<string> tsPropertyEmailCc { get; set; }
        public List<string> tsLoanEmail { get; set; }
        public List<string> tsLoanEmailCc { get; set; }
        public List<string> tsFcaTaggingEmail { get; set; }
        public List<string> tsApprovedTreasury { get; set; }

    }
}