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


namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [Route("api/admin/{action}", Name = "admin")]
    public class AdminController : ApiController
    {
        #region User Management
        

        [HttpGet]
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
                            Locked = userVm.Locked != null ? (bool)userVm.Locked : false,
                            /*AspNetRoles = new List<AspNetRoles>()
                            {
                                new AuthService().GetRole(userVm.RoleName)
                            }*/
                        };
                        db.AspNetUsers.Add(newUser);
                        db.SaveChanges();

                        var role = db.AspNetRoles.FirstOrDefault(x => x.Name == userVm.RoleName);
                        role.AspNetUsers.Add(newUser);
                        
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
                    
                    var result = new AuthService().UpdateUser(key, userVm.RoleName, !userVm.Locked);

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

        #region Utility

        [HttpPost]
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

        #region User Access Log

        [HttpGet]
        public HttpResponseMessage UserAccessLog(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Log_UserAccess.OrderByDescending(x => x.RecordedDate).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
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
