using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.TenAmCutOff
{
    public class UpdateClosingBalanceVM
    {
        public string Id { get; set; }
        public string Currency { get; set; }
        public string Account { get; set; }
        public double ClosingBalance { get; set; }
    }
}