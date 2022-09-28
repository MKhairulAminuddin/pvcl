using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Administration
{
    public class NewPermissionReq
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public int ParentId { get; set; }
    }
}
