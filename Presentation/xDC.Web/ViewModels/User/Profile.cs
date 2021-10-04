using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.User
{
    public class Profile
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        public string Title { get; set; }
        public string Department { get; set; }
        public string TelNo { get; set; }
        public string Office { get; set; }

        public string KashflowUserGroup { get; set; }
        public bool KashflowApprover { get; set; }
        
    }
}