using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Administration
{
    public class NewRoleReq
    {
        public string RoleName { get; set; }
        public List<NewPermissionReq> Permissions { get; set; }

        public NewRoleReq()
        {
            Permissions = new List<NewPermissionReq>();
        }
    }
}
