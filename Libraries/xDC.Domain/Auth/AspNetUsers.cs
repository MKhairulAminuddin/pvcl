using System;

namespace xDC.Domain.Auth
{
    public class AspNetUsers
    {
        public string UserName { get; set; }
        public virtual string TelephoneNumber { get; set; }
        public virtual string FullName { get; set; }
        public virtual string Title { get; set; }
        public virtual string Department { get; set; }
        public virtual string Email { get; set; }

        // <summary>
        //     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        // </summary>
        // public virtual DateTime? LockoutEndDateUtc { get; set; }

        // <summary>
        //     Is lockout enabled for this user
        // </summary>
        // public virtual bool LockoutEnabled { get; set; }

        // <summary>
        //     Used to record failures for the purposes of lockout
        // </summary>
        // public virtual int AccessFailedCount { get; set; }

        public virtual bool Locked { get; set; }

        public virtual DateTime? LastLogin { get; set; }

        public virtual DateTime CreatedDate { get; set; }
        
    }
}
