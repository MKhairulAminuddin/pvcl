using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Forms
{
    public class ReassignNewApproverReq
    {
        public int formId { get; set; }
        public string newApprover { get; set; }
    }
}
