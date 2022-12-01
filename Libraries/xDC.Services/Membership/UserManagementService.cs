using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using xDC.Domain.WebApi.Administration;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Audit;
using xDC.Utils;

namespace xDC.Services.Membership
{
    public class UserManagementService : IUserManagementService
    {
        #region Fields

        private readonly IXDcLogger _logger;
        private readonly IAuditService _auditService;

        #endregion

        #region Ctor

        public UserManagementService(IXDcLogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Methods
        public bool InsertUser(string adUsername, string roleName, bool? lockedStatus, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var adUser = db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == adUsername);
                    if (adUser == null) return false;

                    var newUser = new AspNetUsers()
                    {
                        UserName = adUsername,
                        Email = adUser.Email,
                        Department = adUser.Department,
                        Title = adUser.Title,
                        TelephoneNumber = adUser.TelNo,
                        FullName = adUser.DisplayName,
                        CreatedDate = DateTime.Now,
                        Locked = lockedStatus != null ? (bool)lockedStatus : false
                    };

                    var addedUser = db.AspNetUsers.Add(newUser);
                    var role = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);
                    role.AspNetUsers.Add(addedUser);

                    var insertStatus = db.SaveChanges();

                    if (insertStatus < 1) return false;

                    _auditService.Capture_UMA(Common.UserManagementActionType.Add, null, newUser.UserName, performedBy);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }
        public bool UpdateUser(string username, string roleName, bool? locked, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var existingUser = db.AspNetUsers.Include(y => y.AspNetRoles).FirstOrDefault(x => x.UserName == username);
                    if (existingUser == null) return false;

                    
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        _auditService.Capture_UMA(Common.UserManagementActionType.ChangeRole, $"From {existingUser.AspNetRoles?.First().Name} to {roleName}", existingUser.UserName, performedBy);

                        var getAspNetRole = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);
                        existingUser.AspNetRoles = new List<AspNetRoles>()
                        {
                            getAspNetRole
                        };
                    }

                    if (locked != null)
                    {
                        _auditService.Capture_UMA(Common.UserManagementActionType.ChangeStatus, $"From {(existingUser.Locked ? "Disabled" : "Enabled")} to {((bool)locked ? "Disabled" : "Enabled")}", existingUser.UserName, performedBy);

                        existingUser.Locked = (bool)locked;
                    }

                    var updateStatus = db.SaveChanges();
                    return updateStatus > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
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
                    var deletionStatus = db.SaveChanges();

                    if (deletionStatus < 1) return false;
                    
                    _auditService.Capture_UMA(Common.UserManagementActionType.Delete, null, username, performedBy);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }
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
                _logger.LogError(ex);
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
                _logger.LogError(ex);
                return false;
            }

        }
        public List<UserWithRoleRes> GetUsersWithRole()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.AspNetUsers.Include(x => x.AspNetRoles).Select(y => new UserWithRoleRes()
                    {
                        UserName = y.UserName,
                        Locked = !y.Locked,
                        Email = y.Email,
                        TelephoneNumber = y.TelephoneNumber,
                        Title = y.Title,
                        Department = y.Department,
                        RoleName = y.AspNetRoles.Select(z => z.Name).FirstOrDefault(),
                        CreatedDate = y.CreatedDate,
                        LastLogin = y.LastLogin
                    }).ToList();

                    return result;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        #endregion



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
                _logger.LogError(ex);
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
                _logger.LogError(ex);
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
                _logger.LogError(ex);
                return null;
            }
        }

        

        

        
    }
}
