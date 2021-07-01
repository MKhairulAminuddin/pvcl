using System;

namespace xDC.Domain.Auth
{
    public class AspNetUsersWithRole
    {
        public string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual string TelephoneNumber { get; set; }
        public virtual string FullName { get; set; }
        public virtual string Title { get; set; }
        public virtual string Department { get; set; }
        public virtual bool Locked { get; set; }
        public virtual DateTime? LastLogin { get; set; }
        public virtual DateTime? CreatedDate { get; set; }

        public virtual string RoleName { get; set; }
        public virtual int RoleId { get; set; }
    }
}
