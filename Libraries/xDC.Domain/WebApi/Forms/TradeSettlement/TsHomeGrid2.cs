using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Forms.TradeSettlement
{
    public class TsHomeGrid2
    {
        public DateTime? FormDate { get; set; }
        public string Currency { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool EnablePrint { get; set; }
    }
}
