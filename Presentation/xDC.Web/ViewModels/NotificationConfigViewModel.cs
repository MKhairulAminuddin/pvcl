using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels
{
    public class NotificationConfigViewModel
    {
        public bool InflowFundEnableNotification { get; set; }
        public bool InflowFundEnableAdminModificationNotification { get; set; }
        public DateTime InflowFundCutOffTime { get; set; }

    }
}