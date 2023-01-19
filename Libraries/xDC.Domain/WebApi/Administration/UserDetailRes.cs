using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Administration
{
    public class UserDetailRes
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string TelNo { get; set; }
        public string Office { get; set; }
        public string AdType { get; set; }
        public string DistinguishedName { get; set; }

        public string AdAccountCreatedDate { get; set; }
        public string AdAccountChangedDate { get; set; }
        public string LastBadPasswordAttemptDate { get; set; }
        public string LastLogonDate { get; set; }
        public string LastPasswordSetDate { get; set; }
    }
}
