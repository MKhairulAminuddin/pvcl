using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExpress.XtraSpreadsheet.Model;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using xDC.Infrastructure.Application;
using xDC.Utils;
using xDC_Web.Models;
using xReport.Web.ViewModels;
using System.Data.Entity;
using xDC.Services;
using xDC.Logging;
using xDC.Domain.WebApi.Administration;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        #region User Management
        

        [HttpGet]
        [Route("GetUsers")]
        public HttpResponseMessage GetUsers(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.AspNetUsers.Include(x => x.AspNetRoles).Select(y => new
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

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }
        
        
        [HttpPost]
        [Route("InsertUser")]
        public HttpResponseMessage InsertUser(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key");
                var values = form.Get("values");
                var userVm = new UserVM();
                JsonConvert.PopulateObject(values, userVm);
                

                var isUserExist = new AuthService().IsUserExist(userVm.UserName);
                var isUserExistInAd = new AuthService().IsUserExistInAd(userVm.UserName);

                if (!isUserExist && isUserExistInAd)
                {
                    var getUserInfoFromAd = new AuthService().GetUserFromAd(userVm.UserName);

                    using (var db = new kashflowDBEntities())
                    {
                        var newUser = new AspNetUsers()
                        {
                            UserName = userVm.UserName,
                            Email = getUserInfoFromAd.Email,
                            Department = getUserInfoFromAd.Department,
                            Title = getUserInfoFromAd.Title,
                            TelephoneNumber = getUserInfoFromAd.TelNo,
                            FullName = getUserInfoFromAd.DisplayName,
                            CreatedDate = DateTime.Now,
                            Locked = userVm.Locked != null ? (bool)userVm.Locked : false
                        };

                        var result = new AuthService().InsertUser(newUser, userVm.RoleName, User.Identity.Name);
                        
                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.Created, userVm);
                    }
                    
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "User already exist!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateUser")]
        public HttpResponseMessage UpdateUser(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key");
                var values = form.Get("values");
                var userVm = new UserVM();
                JsonConvert.PopulateObject(values, userVm);

                var isUserExist = new AuthService().GetUser(key);

                if (isUserExist != null)
                {
                    if (userVm.UserName != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Editing username is not allowed as username treated as primary key. Kindly create new record instead.");
                    }
                    
                    var result = new AuthService().UpdateUser(key, userVm.RoleName, !userVm.Locked, User.Identity.Name);

                    if (result)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Update failed");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "User not found");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpDelete]
        [Route("DeleteUser")]
        public HttpResponseMessage DeleteUser(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key");
                var isUserExist = new AuthService().IsUserExist(key);

                if (!isUserExist)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "User not exist!");
                }

                var result = new AuthService().DeleteUser(key, User.Identity.Name);

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete user failed!");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        #endregion

        #region Role Management

        [HttpGet]
        [Route("GetRoles")]
        public HttpResponseMessage GetRoles(DataSourceLoadOptions loadOptions)
        {
            var response = new AuthService().GetRoles(out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpPut]
        [Route("UpdateRole")]
        public HttpResponseMessage UpdateRole(FormDataCollection form)
        {
            try
            {
                var key = int.Parse(form.Get("key"));
                var values = form.Get("values");
                var roleReq = new RoleReq();
                JsonConvert.PopulateObject(values, roleReq);

                var result = new AuthService().UpdateRoleName(key, roleReq.RoleName);

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error. Check application logs.");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpDelete]
        [Route("DeleteRole")]
        public HttpResponseMessage DeleteRole(FormDataCollection form)
        {
            try
            {
                var key = int.Parse(form.Get("key"));

                var result = new AuthService().DeleteRole(key, User.Identity.Name);

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "rror. Check application logs.");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetPermissions")]
        public HttpResponseMessage GetPermissions(DataSourceLoadOptions loadOptions)
        {
            var result = new AuthService().GetPermissions(out bool statusRequest);
            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }

        }

        [HttpGet]
        [Route("GetRolePermissions/{roleId}")]
        public HttpResponseMessage GetRolePermissions(int roleId, DataSourceLoadOptions loadOptions)
        {
            var result = new AuthService().GetRolePermissions(roleId, out bool statusRequest);
            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
            
        }

        [HttpPost]
        [Route("UpdateRolePermission/{roleId}")]
        public HttpResponseMessage UpdateRolePermissions(int roleId,[FromBody] UpdatePremissionReq req)
        {
            if (new AuthService().UpdateRolePermission(roleId, req, User.Identity.Name))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error. Check application logs.");
            }
        }

        [HttpPost]
        [Route("AddNewRole")]
        public HttpResponseMessage AddNewRole([FromBody] NewRoleReq req)
        {
            if (new AuthService().AddNewRole(req.RoleName, req.Permissions, User.Identity.Name))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error. Check application logs.");
            }
        }

        #endregion

        #region Utility

        [HttpPost]
        [Route("TestEmail")]
        public HttpResponseMessage TestEmail(FormDataCollection form)
        {
            try
            {
                var recipient = form.Get("emailRecipient");

                if (!string.IsNullOrEmpty(recipient))
                {
                    new MailService().TestSendEmailToSmtp(recipient);
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        

        #region User Activity Log

        [HttpGet]
        [Route("UserActivityLog")]
        public HttpResponseMessage UserActivityLog(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Log_UserAccess.ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #endregion

        #region Admin Log

        [HttpGet]
        [Route("AdminLog")]
        public HttpResponseMessage AdminLog(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Log_UserAccess.ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #endregion
    }
}
