using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.WebApi.Administration;

namespace xDC.Services.Membership
{
    public interface IRoleManagementService
    {
        string GetUserRoles(string username);
        List<RolesRes> GetRoles(out bool status);
        List<PermissionsRes> GetPermissions(out bool status);
        List<RolePermissionsRes> GetRolePermissions(int selectedRoleId, out bool status);
        bool UpdateRolePermission(int roleId, UpdatePremissionReq req, string performedBy);
        bool AddNewRole(string newRoleName, List<NewPermissionReq> req, string performedBy);
        bool UpdateRoleName(int roleId, string newRoleName);
        bool DeleteRole(int roleId, string performedBy);
        bool IsUserHaveAccess(string userName, string permissionName);
    }
}
