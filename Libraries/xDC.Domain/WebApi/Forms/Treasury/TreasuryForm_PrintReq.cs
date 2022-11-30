using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Forms.Treasury
{
    public class TreasuryForm_PrintReq
    {
        public int formId { get; set; }
        public bool isExportAsExcel { get; set; }
    }
}
