using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Forms.TradeSettlement
{
    public class TsGenerateFileRequest
    {
        public int formId { get; set; }
        public long settlementDate { get; set; }
        public string currency { get; set; }
        public bool isExportAsExcel { get; set; }
    }
}
