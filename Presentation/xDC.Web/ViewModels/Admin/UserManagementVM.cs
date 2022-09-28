using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Admin
{
    public class UserManagementVM
    {
        public List<UserManagementVMKeyValue> TotalRegisteredUsers { get; set; }
        public List<UserManagementVMKeyValue> TotalDepartmentBreakdown { get; set; }
        public List<UserManagementVMKeyValue> TotalUserGroupBreakdown { get; set; }

        public UserManagementVM()
        {
            TotalRegisteredUsers = new List<UserManagementVMKeyValue>();
            TotalDepartmentBreakdown = new List<UserManagementVMKeyValue>();
            TotalUserGroupBreakdown = new List<UserManagementVMKeyValue>();
        }
    }

    public class UserManagementVMKeyValue
    {
        public string Key { get; set; }
        public int Value { get; set; }
    }
}