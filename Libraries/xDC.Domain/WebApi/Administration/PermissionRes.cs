using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Administration
{
    public class PermissionsRes
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public bool Selected { get; set; }
        public bool? Expanded { get; set; }
        public int ParentId { get; set; }
        public int? TotalChild { get; set; }
        public bool Disabled { get; set; }
    }
}
