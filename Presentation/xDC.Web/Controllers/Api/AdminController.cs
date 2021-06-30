using System;
using System.Collections.Generic;
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

namespace xDC_Web.Controllers.Api
{
    /*[Authorize(Roles = "Administrator")]*/
    [Route("api/admin/{action}", Name = "admin")]
    public class AdminController : ApiController
    {
        #region User Management
        

        [HttpGet]
        public HttpResponseMessage GetUsers(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                var xdfad = UserManager.FindByName("abdulkhaliq.h");

                using (var db = new kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.AspNetUsers.Select(x => new
                    {
                        UserName = x.UserName,
                        Locked = x.Locked,
                        Email = x.Email,
                        TelNo = x.TelNo,
                        LastLogin = x.LastLogin,
                        CreatedDate = x.CreatedDate,

                        roleName = x.AspNetRoles.FirstOrDefault().Name
                    }).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }
        
        /*
        [HttpPost]
        public HttpResponseMessage InsertUser(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key");
                var values = form.Get("values");
                var userVm = new UserVM();
                JsonConvert.PopulateObject(values, userVm);

                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                UserManager.FindByName(userVm.UserName);
                var isUserExist = new AuthService().IsUserExist(userVm.UserName);
                var detailsFromAd = new SupportingService().GetAdUser(userVm.UserName);

                if (!isUserExist && detailsFromAd != null)
                {
                    var result = new AuthService().InsertUser(userVm.UserName, detailsFromAd.Email, userVm.RoleName, detailsFromAd.TelNo);

                    if (result)
                    {
                        new ActivityLogService().LogActivity(User.Identity.Name, Config.LogType.Create,
                            "Created a user record: " + userVm.UserName);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "User creation failed!");
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
        }*/
/*
        [HttpPut]
        [Route("admin/updateUser")]
        public HttpResponseMessage UpdateUser(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key");
                var values = form.Get("values");
                var userVm = new UserVM();
                JsonConvert.PopulateObject(values, userVm);

                var isUserExist = new AuthService().IsUserExist(key);

                if (isUserExist)
                {
                    if (userVm.UserName != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Editing username is not allowed as username treated as primary key. Kindly create new record instead.");
                    }

                    var result = new AuthService().UpdateUser(key, userVm.Email, userVm.RoleName, !userVm.Locked);

                    if (result)
                    {
                        new ActivityLogService().LogActivity(User.Identity.Name, Config.LogType.Update, "Updated a user record: " + key);
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
        [Route("admin/deleteUser")]
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

                var result = new AuthService().DeleteUser(key);

                if (result)
                {
                    new ActivityLogService().LogActivity(User.Identity.Name, Config.LogType.Delete,
                        "Delete a user record: " + key);
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
        */

        #endregion

    }
}
