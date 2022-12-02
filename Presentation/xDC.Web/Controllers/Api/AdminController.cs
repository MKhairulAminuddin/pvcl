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
using static xDC.Utils.Common;
using xDC_Web.Extension.CustomAttribute;
using xDC.Services.Membership;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(PermissionKey.Administration)]
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        #region Fields

        private readonly IUserManagementService _userMgmtService;
        private readonly IRoleManagementService _roleService;

        #endregion

        #region Ctor

        public AdminController(IUserManagementService userMgmtService, IRoleManagementService roleService)
        {
            _userMgmtService = userMgmtService;
            _roleService = roleService;
        }

        #endregion

        #region User Management


        [HttpGet]
        [Route("GetUsers")]
        [KflowApiAuthorize(PermissionKey.Administration_UserManagement)]
        public HttpResponseMessage GetUsers(DataSourceLoadOptions loadOptions)
        {
            return Request.CreateResponse(DataSourceLoader.Load(_userMgmtService.GetUsersWithRole(), loadOptions));
        }
        
        [HttpPost]
        [Route("InsertUser")]
        [KflowApiAuthorize(PermissionKey.Administration_UserManagement)]
        public HttpResponseMessage InsertUser(FormDataCollection form)
        {
            var values = form.Get("values");
            var userVm = new UserVM();
            JsonConvert.PopulateObject(values, userVm);
                

            var isUserExist = _userMgmtService.IsUserExist(userVm.UserName);
            if (!isUserExist) return Request.CreateResponse(HttpStatusCode.BadRequest, "User already exist!");

            var isUserExistInAd = _userMgmtService.IsUserExistInAd(userVm.UserName);
            if (isUserExistInAd) return Request.CreateResponse(HttpStatusCode.BadRequest, "User not exist in KWAP AD!");

            var result = _userMgmtService.InsertUser(userVm.UserName, userVm.RoleName, userVm.Locked, User.Identity.Name);
            if (!result) return Request.CreateResponse(HttpStatusCode.BadRequest, "System failed to create new user!");

            return Request.CreateResponse(HttpStatusCode.Created, userVm);
        }

        [HttpPut]
        [Route("UpdateUser")]
        [KflowApiAuthorize(PermissionKey.Administration_UserManagement)]
        public HttpResponseMessage UpdateUser(FormDataCollection form)
        {
            var userName = form.Get("key");
            var values = form.Get("values");
            var userVm = new UserVM();
            JsonConvert.PopulateObject(values, userVm);

            var isUserExist = _userMgmtService.IsUserExist(userName);
            if (!isUserExist) return Request.CreateResponse(HttpStatusCode.BadRequest, "User not found");

            var result = _userMgmtService.UpdateUser(userName, userVm.RoleName, !userVm.Locked, User.Identity.Name);
            if (!result) return Request.CreateResponse(HttpStatusCode.BadRequest, "System failed to update user profile");

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("DeleteUser")]
        [KflowApiAuthorize(PermissionKey.Administration_UserManagement)]
        public HttpResponseMessage DeleteUser(FormDataCollection form)
        {
            var key = form.Get("key");

            var isUserExist = _userMgmtService.IsUserExist(key);
            if (!isUserExist) return Request.CreateResponse(HttpStatusCode.BadRequest, "User not exist!");

            var result = _userMgmtService.DeleteUser(key, User.Identity.Name);
            if (!result) return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete user failed!");
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        #endregion

        #region Role Management

        [HttpGet]
        [Route("GetRoles")]
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage GetRoles(DataSourceLoadOptions loadOptions)
        {
            var response = _roleService.GetRoles(out bool statusRequest);

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
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage UpdateRole(FormDataCollection form)
        {
            try
            {
                var key = int.Parse(form.Get("key"));
                var values = form.Get("values");
                var roleReq = new RoleReq();
                JsonConvert.PopulateObject(values, roleReq);

                var result = _roleService.UpdateRoleName(key, roleReq.RoleName);

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
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage DeleteRole(FormDataCollection form)
        {
            try
            {
                var key = int.Parse(form.Get("key"));

                var result = _roleService.DeleteRole(key, User.Identity.Name);

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
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage GetPermissions(DataSourceLoadOptions loadOptions)
        {
            var result = _roleService.GetPermissions(out bool statusRequest);
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
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage GetRolePermissions(int roleId, DataSourceLoadOptions loadOptions)
        {
            var result = _roleService.GetRolePermissions(roleId, out bool statusRequest);
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
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage UpdateRolePermissions(int roleId,[FromBody] UpdatePremissionReq req)
        {
            if (_roleService.UpdateRolePermission(roleId, req, User.Identity.Name))
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
        [KflowApiAuthorize(PermissionKey.Administration_RolesManagement)]
        public HttpResponseMessage AddNewRole([FromBody] NewRoleReq req)
        {
            if (_roleService.AddNewRole(req.RoleName, req.Permissions, User.Identity.Name))
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
        [KflowApiAuthorize(PermissionKey.Administration_Utility)]
        public HttpResponseMessage TestEmail(FormDataCollection form)
        {
            try
            {
                var recipient = form.Get("emailRecipient");

                if (!string.IsNullOrEmpty(recipient))
                {
                    EmailNotificationService.TestSendEmail(recipient);
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
                    var result = db.Audit_UserAccess.ToList();

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
                    var result = db.Audit_UserAccess.ToList();

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
