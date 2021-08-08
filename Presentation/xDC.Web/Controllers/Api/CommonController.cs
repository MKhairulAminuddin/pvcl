using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC_Web.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/common")]
    public class CommonController : ApiController
    {
        #region Active Directory

        [HttpGet]
        [Route("GetActiveDirectoryUsers")]
        public HttpResponseMessage GetActiveDirectoryUsers(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AspNetActiveDirectoryUsers.Where(x => x.Title != null).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetActiveDirectoryUsersRegisteredIntoSystem")]
        public HttpResponseMessage GetActiveDirectoryUsersRegisteredIntoSystem(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AspNetActiveDirectoryUsers.Where(x => x.Title != null).ToList();

                    var amsdRoleName = Config.AclAmsd;
                    var systemUser = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(amsdRoleName)).ToList();
                    var issdRoleName = Config.AclIssd;
                    var issdUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(issdRoleName)).ToList();
                    systemUser.AddRange(issdUsers);
                    
                    var finalResult = new List<AspNetActiveDirectoryUsers>();

                    foreach (var user in systemUser)
                    {
                        var findAd = result.FirstOrDefault(x => x.Username == user.UserName);
                        if (findAd != null)
                        {
                            finalResult.Add(findAd);
                        }
                    }


                    return Request.CreateResponse(DataSourceLoader.Load(finalResult, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/common/GetActiveDirectoryUsersByDepartment/{department}")]
        public HttpResponseMessage GetActiveDirectoryUsersByDepartment(string department, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AspNetActiveDirectoryUsers.Where(x => x.Title != null && x.Department == department).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region User Roles

        [HttpGet]
        [Route("GetRoles")]
        public HttpResponseMessage GetRoles(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.AspNetRoles.ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Dropdown List
        
        [HttpGet]
        [Route("GetTradeSettlementCurrencies")]
        public HttpResponseMessage GetTradeSettlementCurrencies(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Common.DropdownConfigKeyMapping(3);
                    var result = db.Config_Dropdown.Where(x => x.Key == key).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetInflowFundsFundType")]
        public HttpResponseMessage GetInflowFundsFundType(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Common.DropdownConfigKeyMapping(1);
                    var result = db.Config_Dropdown.Where(x => x.Key == key).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetInflowFundsBank")]
        public HttpResponseMessage GetInflowFundsBank(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Common.DropdownConfigKeyMapping(2);
                    var result = db.Config_Dropdown.Where(x => x.Key == key).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetConfigDropdownKey")]
        public HttpResponseMessage GetConfigDropdownKey(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Key.Where(x => x.KeyType == "Dropdown").ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Approver List

        [HttpGet]
        [Route("GetApproverAmsdInflowFunds")]
        public HttpResponseMessage GetApproverAmsdInflowFunds(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Common.FormTypeMapping(1);
                    var result = db.Config_Approver.Where(x => x.FormType == key && x.Username != User.Identity.Name).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTradeSettlementApprover")]
        public HttpResponseMessage GetTradeSettlementApprover(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Common.FormTypeMapping(2);
                    var result = db.Config_Approver.Where(x => x.FormType == key && x.Username != User.Identity.Name).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        #endregion

        #region My Notification

        [HttpGet]
        [Route("GetMyNotification")]
        public HttpResponseMessage GetMyNotification(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var currentUsername = User.Identity.Name;
                    var result = db.App_Notification.Where(x => x.UserId == currentUsername)
                        .OrderByDescending(x => x.CreatedOn).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteMyNotification")]
        public HttpResponseMessage DeleteMyNotification(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var foundRecord = db.App_Notification.First(x => x.Id == key);

                    db.App_Notification.Remove(foundRecord);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("ClearMyNotification")]
        public HttpResponseMessage ClearMyNotification()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var foundRecord = db.App_Notification.Where(x => x.UserId == User.Identity.Name);

                    db.App_Notification.RemoveRange(foundRecord);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.Accepted, "Notification Cleared!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        #endregion

        #region Workflow Information

        [HttpGet]
        [Route("GetWorkflow")]
        public HttpResponseMessage GetWorkflow(string id, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(id);
                    var result = db.Form_Workflow.Where(x => x.FormId == formId).OrderByDescending(x => x.Id).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
