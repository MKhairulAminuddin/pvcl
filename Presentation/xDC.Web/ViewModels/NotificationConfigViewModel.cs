using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels
{
    public class NotificationConfigViewModel
    {
        public DateTime InflowFundCutOffTime { get; set; }

        #region ISSD Notification
        public string issd_TsCn { get; set; }
        public string issd_TsCnCc { get; set; }
        public string issd_TsPe { get; set; }
        public string issd_TsPeCc { get; set; }
        public string issd_TsProperty { get; set; }
        public string issd_TsPropertyCc { get; set; }
        public string issd_TsLoan { get; set; }
        public string issd_TsLoanCc { get; set; }
        public string issd_FcaTagging { get; set; }
        public string issd_TApproved { get; set; }

        public bool issd_Enable_TsCn { get; set; }
        public bool issd_Enable_TsCnCc { get; set; }
        public bool issd_Enable_TsPe { get; set; }
        public bool issd_Enable_TsPeCc { get; set; }
        public bool issd_Enable_TsProperty { get; set; }
        public bool issd_Enable_TsPropertyCc { get; set; }
        public bool issd_Enable_TsLoan { get; set; }
        public bool issd_Enable_TsLoanCc { get; set; }
        public bool issd_Enable_FcaTagging { get; set; }
        public bool issd_Enable_TApproved { get; set; }

        #endregion

        #region FID Notification
        public string fid_T_SubmissionCc { get; set; }
        public string fid_T_ApprovalCc { get; set; }

        public bool fid_Enable_T_SubmissionCc { get; set; }
        public bool fid_Enable_T_ApprovalCc { get; set; }

        #endregion

    }
}