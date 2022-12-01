using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.WebApi.Administration;
using xDC.Infrastructure.Application;

namespace xDC.Services.Membership
{
    public interface IUserManagementService
    {
        bool IsUserExist(string username);
        bool IsUserExistInAd(string username);
        string GetUserRoles(string username);
        AspNetUsers GetUser(string username);
        AspNetActiveDirectoryUsers GetUserFromAd(string username);
        bool InsertUser(string adUsername, string roleName, bool? lockedStatus, string performedBy);
        bool UpdateUser(string username, string roleName, bool? locked, string performedBy);
        bool DeleteUser(string username, string performedBy);
        List<UserWithRoleRes> GetUsersWithRole();
    }
}
