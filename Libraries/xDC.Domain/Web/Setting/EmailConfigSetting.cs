using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace xDC.Domain.Web.Setting
{
    public class EmailConfigSetting
    {
        public DateTime InflowFundCutOffTime { get; set; }

        #region ISSD Notification
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsCn { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsCnCc { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsPe { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsPeCc { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsProperty { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsPropertyCc { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsLoan { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_TsLoanCc { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string issd_FcaTagging { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
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

        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string fid_T_SubmittedCc { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string fid_T_ApprovedCc { get; set; }
        [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4}[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]{2,4})[\W]*$", ErrorMessage = "Invalid email entered.")]
        public string fid_TS_ApprovedCc { get; set; }

        public bool fid_Enable_T_SubmittedCc { get; set; }
        public bool fid_Enable_T_ApprovedCc { get; set; }
        public bool fid_Enable_TS_ApprovedCc { get; set; }

        #endregion
    }
}
