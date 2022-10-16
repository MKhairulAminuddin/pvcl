using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using System.Data.Entity;
using xDC.Logging;
using xDC.Domain.WebApi.Administration;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net;
using System.Xml.Linq;
using System.Web.Security;
using System.Runtime.Remoting.Contexts;
using xDC.Utils;

namespace xDC.Services
{
    public class AuthService
    {
        #region User Management

        public bool IsUserExist(string username)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isUserExist = db.AspNetUsers.Any(x => x.UserName == username);
                    return isUserExist;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }

        }

        public bool IsUserExistInAd(string username)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isUserExist = db.AspNetActiveDirectoryUsers.Any(x => x.Username == username);
                    return isUserExist;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }

        }

        public string GetUserRoles(string username)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AspNetUsers.Include(a => a.AspNetRoles).FirstOrDefault(x => x.UserName == username);

                    if (result != null && result.AspNetRoles.Any())
                    {
                        return result.AspNetRoles.Select(x => x.Name).FirstOrDefault();
                    }
                    else
                    {
                        return "Anonymous";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return "Anonymous";
            }

        }

        public AspNetUsers GetUser(string username)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var user = db.AspNetUsers.Include(y => y.AspNetRoles).FirstOrDefault(x => x.UserName == username);

                    if (user != null)
                    {
                        return user;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public AspNetActiveDirectoryUsers GetUserFromAd(string username)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var userFromAd = db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == username);

                    if (userFromAd != null)
                    {
                        return userFromAd;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public AspNetUsers InsertUser(AspNetUsers user, string roleName,string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newUser = db.AspNetUsers.Add(user);

                    var role = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);
                    role.AspNetUsers.Add(newUser);

                    db.SaveChanges();

                    new AuditService().Capture_UMA(Common.UserManagementActionType.Add, null, user.UserName, performedBy);

                    return newUser;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public bool UpdateUser(string username, string roleName, bool? locked, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var existingUser = db.AspNetUsers.Include(y => y.AspNetRoles).FirstOrDefault(x => x.UserName == username);

                    if (existingUser != null)
                    {
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            new AuditService().Capture_UMA(Common.UserManagementActionType.ChangeRole, $"From {existingUser.AspNetRoles.First().Name} to {roleName}", existingUser.UserName, performedBy);

                            var getAspNetRole = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);
                            existingUser.AspNetRoles = new List<AspNetRoles>()
                            {
                                getAspNetRole
                            };
                        }

                        if (locked != null)
                        {
                            new AuditService().Capture_UMA(Common.UserManagementActionType.ChangeStatus, $"From {(existingUser.Locked?"Disabled":"Enabled")} to {((bool)locked ? "Disabled" : "Enabled")}", existingUser.UserName, performedBy);

                            existingUser.Locked = (bool)locked;
                        }

                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        public bool DeleteUser(string username, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getUser = db.AspNetUsers.FirstOrDefault(x => x.UserName == username);
                    db.AspNetUsers.Remove(getUser);
                    db.SaveChanges();

                    new AuditService().Capture_UMA(Common.UserManagementActionType.Delete, null, username, performedBy);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }

        }

        #endregion

        #region Role Management

        public AspNetRoles GetRole(string roleName)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getRole = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);

                    if (getRole != null)
                    {
                        return getRole;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public List<RolesRes> GetRoles(out bool status)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AspNetRoles.Select(y => new RolesRes
                    {
                        RoleId = y.Id,
                        RoleName = y.Name
                    }).ToList();

                    status = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                status = false;
                return new List<RolesRes>();
            }
        }

        public List<PermissionsRes> GetPermissions(out bool status)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    

                    var permissions = db.AspNetPermission.ToList();
                    var permissionTreeView = new List<PermissionsRes>();
                    foreach (var item in permissions)
                    {
                        permissionTreeView.Add(new PermissionsRes
                        {
                            PermissionId = item.Id,
                            PermissionName = item.PermissionName,
                            Selected = false,
                            ParentId = item.Parent
                        });
                    }
                    foreach (var item in permissionTreeView)
                    {
                        if (item.ParentId == 0)
                        {
                            item.TotalChild = permissionTreeView.Count(x => x.ParentId == item.PermissionId);
                        }
                    }

                    status = true;
                    return permissionTreeView;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);

                status = false;
                return new List<PermissionsRes>();
            }
        }

        public List<RolePermissionsRes> GetRolePermissions(int selectedRoleId, out bool status)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var adminRole = db.AspNetRoles.FirstOrDefault(x => x.Name == "Administrator");
                    var adminPermissionHeader = db.AspNetPermission.FirstOrDefault(x => x.PermissionName == "Administration");
                    var corePermissionUnderAdmin = db.AspNetPermission.Where(x => x.Parent == adminPermissionHeader.Id && x.PermissionLevel == 2).ToList();

                    var permissions = db.AspNetPermission.ToList();
                    var permissionTreeView = new List<RolePermissionsRes>();
                    foreach (var item in permissions)
                    {
                        permissionTreeView.Add(new RolePermissionsRes
                        {
                            PermissionId = item.Id,
                            PermissionName = item.PermissionName,
                            Selected = false,
                            ParentId = item.Parent,
                            RoleId = selectedRoleId,
                            Disabled = (selectedRoleId == adminRole.Id && corePermissionUnderAdmin.Select(x => x.Id).Contains(item.Id)) || (selectedRoleId == adminRole.Id && item.Id == adminPermissionHeader.Id)
                        });
                    }
                    foreach (var item in permissionTreeView)
                    {
                        if (item.ParentId == 0)
                        {
                            item.TotalChild = permissionTreeView.Count(x => x.ParentId == item.PermissionId);
                        }
                    }

                    var selectedRolePermission = db.AspNetRoles.Include(x => x.AspNetPermission).FirstOrDefault(x => x.Id == selectedRoleId);

                    foreach (var item in selectedRolePermission.AspNetPermission)
                    {
                        if (permissionTreeView.FirstOrDefault(x => x.PermissionId == item.Id) != null && item.PermissionLevel == 2)
                        {
                            if (permissionTreeView.FirstOrDefault(x => x.PermissionId == item.Parent) != null)
                            {
                                permissionTreeView.First(x => x.PermissionId == item.Parent).Expanded = true;
                                permissionTreeView.First(x => x.PermissionId == item.Parent).Selected = (selectedRolePermission.AspNetPermission.Count(x => x.Parent == item.Parent) == permissionTreeView.First(x => x.PermissionId == item.Parent).TotalChild);
                            }

                            permissionTreeView.FirstOrDefault(x => x.PermissionId == item.Id).Selected = true;
                        }
                    }

                    status = true;
                    return permissionTreeView;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);

                status = false;
                return new List<RolePermissionsRes>();
            }
        }

        public bool UpdateRolePermission(int roleId, UpdatePremissionReq req, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    if (req != null)
                    {
                        // delete first
                        var rolePermissions = db.AspNetRoles.Include(x => x.AspNetPermission).FirstOrDefault(a => a.Id == roleId);
                        if (rolePermissions != null)
                        {
                            var permissionToDelete = rolePermissions.AspNetPermission;
                            if (permissionToDelete != null)
                            {
                                foreach (var permission in permissionToDelete.ToList())
                                {
                                    rolePermissions.AspNetPermission.Remove(permission);
                                    new AuditService().Capture_RMA(Common.RoleManagementActionType.DeletePermission, $"Deleted {permission.PermissionName} permission from role {rolePermissions.Name}", rolePermissions.Name, performedBy);
                                }
                                db.SaveChanges();
                            }
                        }

                        // then add back new
                        if (req.data != null)
                        {
                            var newChildPermission = new List<AspNetPermission>();
                            var allNewPermission = new List<AspNetPermission>();

                            foreach (var item in req.data)
                            {
                                var newAssignedPermission = db.AspNetPermission.FirstOrDefault(x => x.Id == item.PermissionId);
                                if (newAssignedPermission != null)
                                {
                                    newChildPermission.Add(newAssignedPermission);
                                }
                            }

                            foreach (var item in newChildPermission.Select(x => x.Parent).Distinct())
                            {
                                var newAssignedParentPermission = db.AspNetPermission.FirstOrDefault(x => x.Id == item);
                                if (newAssignedParentPermission != null)
                                {
                                    allNewPermission.Add(newAssignedParentPermission);
                                }
                            }

                            foreach (var item in newChildPermission)
                            {
                                allNewPermission.Add(item);
                            }

                            foreach (var item in allNewPermission)
                            {
                                rolePermissions.AspNetPermission.Add(item);
                                new AuditService().Capture_RMA(Common.RoleManagementActionType.AddPermission, $"Added {item.PermissionName} permission from role {rolePermissions.Name}", rolePermissions.Name, performedBy);

                            }

                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        // delete first
                        var rolePermissions = db.AspNetRoles.Include(x => x.AspNetPermission).FirstOrDefault(a => a.Id == roleId);
                        if (rolePermissions != null)
                        {
                            var permissionToDelete = rolePermissions.AspNetPermission;
                            if (permissionToDelete != null)
                            {
                                foreach (var permission in permissionToDelete.ToList())
                                {
                                    rolePermissions.AspNetPermission.Remove(permission);
                                    new AuditService().Capture_RMA(Common.RoleManagementActionType.DeletePermission, $"Deleted {permission.PermissionName} permission from role {rolePermissions.Name}", rolePermissions.Name, performedBy);

                                }
                                db.SaveChanges();
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        public bool AddNewRole(string newRoleName, List<NewPermissionReq> req, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isSameRoleNameExist = db.AspNetRoles.FirstOrDefault(x => x.Name == newRoleName);
                    if (isSameRoleNameExist == null)
                    {
                        var newRole = new AspNetRoles() { Name = newRoleName };
                        var addNewRole = db.AspNetRoles.Add(newRole);
                        db.SaveChanges();
                        new AuditService().Capture_RMA(Common.RoleManagementActionType.Add, $"Added new role {newRole.Name}", newRole.Name, performedBy);


                        int addRoleId = newRole.Id;

                        if (addRoleId != 0)
                        {
                            if (req != null)
                            {
                                if (req.Any())
                                {
                                    foreach (var item in req)
                                    {
                                        var newAssignedPermission = db.AspNetPermission.FirstOrDefault(x => x.Id == item.PermissionId);
                                        if (newAssignedPermission != null)
                                        {
                                            addNewRole.AspNetPermission.Add(newAssignedPermission);
                                            new AuditService().Capture_RMA(Common.RoleManagementActionType.AddPermission, $"Added {newAssignedPermission.PermissionName} permission to role {newRole.Name}", newRole.Name, performedBy);
                                        }
                                    }
                                    db.SaveChanges();
                                }
                            }
                        }
                        
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        public bool UpdateRoleName(int roleId, string newRoleName)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var selectedRole = db.AspNetRoles.FirstOrDefault(x => x.Id == roleId);
                    if (selectedRole!=null)
                    {
                        selectedRole.Name = newRoleName;
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);

                return false;
            }
        }

        public bool DeleteRole(int roleId, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var selectedRole = db.AspNetRoles.FirstOrDefault(x => x.Id == roleId);
                    var isAdminRole = IsAdminRole(roleId);

                    if (selectedRole != null && !isAdminRole)
                    {
                        new AuditService().Capture_RMA(Common.RoleManagementActionType.Delete, $"Deleted role {selectedRole.Name}", selectedRole.Name, performedBy);

                        db.AspNetRoles.Remove(selectedRole);
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);

                return false;
            }
        }

        #endregion

        #region Access Permission

        public bool IsUserHaveAccess(string userName, string permissionName)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var theUser = db.AspNetUsers.FirstOrDefault(x => x.UserName == userName);
                    var permitted = false;
                    if (theUser != null)
                    {
                        var userRoles = theUser.AspNetRoles;
                        if (userRoles != null)
                        {
                            
                            foreach (var rolePermissions in userRoles)
                            {
                                if (rolePermissions.AspNetPermission.Any(x => x.PermissionName == permissionName))
                                {
                                    permitted = true;
                                }
                            }
                        }
                    }

                    return permitted;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return false;
            }
        }

        public bool IsRoleHaveAccess(string roleName, string permissionName)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var theRole = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);
                    var permitted = false;

                    if (theRole != null)
                    {
                        foreach (var rolePermissions in theRole.AspNetPermission)
                        {
                            if (rolePermissions.PermissionName == permissionName)
                            {
                                permitted = true;
                            }
                        }
                    }

                    return permitted;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return false;
            }
        }

        #endregion


        #region Private Functions

        private bool IsAdminRole(int roleId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var selectedRole = db.AspNetRoles.FirstOrDefault(x => x.Id == roleId);
                    if (selectedRole != null)
                    {
                        db.AspNetRoles.Remove(selectedRole);
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);

                return false;
            }
        }

        #endregion
    }
}
