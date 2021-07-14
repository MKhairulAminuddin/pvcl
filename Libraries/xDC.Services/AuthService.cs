using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using System.Data.Entity;
using xDC.Logging;

namespace xDC.Services
{
    public class AuthService
    {
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

        public AspNetUsers InsertUser(AspNetUsers user)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newUser = db.AspNetUsers.Add(user);
                    db.SaveChanges();
                    return newUser;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        /*public AspNetRoles InsertUserRole(sapnet user)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newUser = db.AspNetRoles.Add(user);
                    db.SaveChanges();
                    return newUser;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }*/

        public bool UpdateUser(string username, string roleName, bool? locked)
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
                            var getAspNetRole = db.AspNetRoles.FirstOrDefault(x => x.Name == roleName);
                            existingUser.AspNetRoles = new List<AspNetRoles>()
                            {
                                getAspNetRole
                            };
                        }

                        if (locked != null)
                        {
                            existingUser.Locked = (bool) locked;
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

        public bool DeleteUser(string username)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getUser = db.AspNetUsers.FirstOrDefault(x => x.UserName == username);
                    db.AspNetUsers.Remove(getUser);
                    db.SaveChanges();


                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }

        }
    }
}
