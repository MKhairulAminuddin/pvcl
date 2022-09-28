using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC.Domain.WebApi.Administration
{
    public class UpdatePremissionReq
    {
        public List<UpdatePremissionData> data { get; set; }

        public UpdatePremissionReq()
        {
            data = new List<UpdatePremissionData>();
        }
    }

    public class UpdatePremissionData
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public bool Selected { get; set; }
        public bool? Expanded { get; set; }
        public int ParentId { get; set; }
        public int? TotalChild { get; set; }
    }
}