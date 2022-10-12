using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Audit
{
    public class AuditReq
    {
        public long FromDateUnix { get; set; }
        public long ToDateUnix { get; set; }
        public string UserId { get; set; }
    }
}
