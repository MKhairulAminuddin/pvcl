using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels
{
    public class NotificationConfigViewModel
    {
        public DateTime InflowFundCutOffTime { get; set; }
        public string tsCnEmail { get; set; }
        public string tsCnEmailCc { get; set; }
        public string tsPeEmail { get; set; }
        public string tsPeEmailCc { get; set; }
        public string tsPropertyEmail { get; set; }
        public string tsPropertyEmailCc { get; set; }
        public string tsLoanEmail { get; set; }
        public string tsLoanEmailCc { get; set; }
        public string tsFcaTaggingEmail { get; set; }
        public string tsApprovedTreasury { get; set; }


        public bool tsCnEmailEnable { get; set; }
        public bool tsCnEmailCcEnable { get; set; }
        public bool tsPeEmailEnable { get; set; }
        public bool tsPeEmailCcEnable { get; set; }
        public bool tsPropertyEmailEnable { get; set; }
        public bool tsPropertyEmailCcEnable { get; set; }
        public bool tsLoanEmailEnable { get; set; }
        public bool tsLoanEmailCcEnable { get; set; }
        public bool tsFcaTaggingEmailEnable { get; set; }
        public bool tsApprovedTreasuryEnable { get; set; }

        public string tSubmissionEmailCc { get; set; }
        public string tApprovedEmailCc { get; set; }

        public bool tSubmissionEmailCcEnable { get; set; }
        public bool tApprovedEmailCcEnable { get; set; }

    }
}