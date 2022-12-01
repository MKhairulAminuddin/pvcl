using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Administration
{
    public class UserWithRoleRes
    {
        public string UserName { get; set; }
        public bool Locked { get; set; }
        public string Email { get; set; }
        public string TelephoneNumber { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }

    }
}
