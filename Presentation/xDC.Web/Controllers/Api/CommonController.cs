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
using xDC.Services;
using xDC.Utils;
using xDC_Web.Models;

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
                    
                    var systemUser = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(Config.Acl.Amsd)).ToList();
                    
                    var issdUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(Config.Acl.Issd)).ToList();
                    systemUser.AddRange(issdUsers);
                    
                    var fidUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(Config.Acl.Fid)).ToList();
                    systemUser.AddRange(fidUsers);
                    
                    var puUsers = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(Config.Acl.PowerUser)).ToList();
                    systemUser.AddRange(puUsers);

                    var finalResult = new List<AspNetActiveDirectoryUsers>();

                    foreach (var user in systemUser)
                    {
                        var findAd = result.FirstOrDefault(x => x.Username == user.UserName);
                        if (findAd != null)
                        {
                            finalResult.Add(findAd);
                        }
                    }

                    finalResult = finalResult.OrderBy(x => x.DisplayName).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(finalResult, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("GetActiveDirectoryUsersByDepartment/{department}")]
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
                    var result = db.Config_Dropdown.Where(x => x.Key == Common.DropdownConfigKey.ISSD_TS_Currency).ToList();

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
                    var result = db.Config_Dropdown.Where(x => x.Key == Common.DropdownConfigKey.AMSD_IF_FundType).ToList();

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
                    var result = db.Config_Dropdown.Where(x => x.Key == Common.DropdownConfigKey.AMSD_IF_Bank).ToList();

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
                    var result = db.Config_Key
                        .Where(x => x.KeyType == Common.ConfigKeyType.Dropdown)
                        .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("Treasury/Notes")]
        public HttpResponseMessage TreasuryNotes(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Dropdown
                        .Where(x => x.Key == Common.DropdownConfigKey.FID_Treasury_Notes)
                        .OrderBy(x => x.Value)
                        .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("Treasury/AssetType")]
        public HttpResponseMessage TreasuryAssetType(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Dropdown
                        .Where(x => x.Key == Common.DropdownConfigKey.FID_Treasury_AssetType)
                        .OrderBy(x => x.Value)
                        .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("Treasury/ProductType")]
        public HttpResponseMessage TreasuryProductType(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Dropdown
                        .Where(x => x.Key == Common.DropdownConfigKey.FID_Treasury_ProductType)
                        .OrderBy(x => x.Value)
                        .ToList();

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
        public HttpResponseMessage Approver_AMSD_IF(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Approver
                        .Where(x => x.FormType == Common.FormType.AMSD_IF && x.Username != User.Identity.Name)
                        .OrderBy(x => x.DisplayName)
                        .ToList();

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
        public HttpResponseMessage Approver_ISSD_TS(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Approver
                        .Where(x => x.FormType == Common.FormType.ISSD_TS && x.Username != User.Identity.Name)
                        .OrderBy(x => x.DisplayName)
                        .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("approverList/treasury")]
        public HttpResponseMessage Approver_FID_Treasury(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Approver
                        .Where(x => x.FormType == Common.FormType.FID_TREASURY && x.Username != User.Identity.Name)
                        .OrderBy(x => x.DisplayName).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("reassignApprover")]
        public HttpResponseMessage ReassignApproval([FromBody] NewApprover input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formType = Common.FormTypeMapping(input.FormType);
                    var permittedApprover = db.Config_Approver
                        .FirstOrDefault(x =>
                            x.FormType == formType && x.Username != User.Identity.Name && x.Username == input.Approver);

                    if (formType!= null && permittedApprover != null)
                    {
                        switch (formType)
                        {
                            case Common.FormType.AMSD_IF:
                                var amsdForm = db.AMSD_IF.FirstOrDefault(x => x.Id == input.FormId);
                                if (amsdForm != null)
                                {
                                    amsdForm.ApprovedBy = permittedApprover.Username;
                                    amsdForm.ApprovedDate = null;
                                    db.SaveChanges();

                                    new WorkflowService().ReassignWorkflow(input.FormId, amsdForm.FormType);
                                    new MailService().SubmitForApproval(input.FormId, amsdForm.FormType, permittedApprover.Username, null);
                                    new NotificationService().NotifyApprovalRequest(permittedApprover.Username, input.FormId, User.Identity.Name, amsdForm.FormType);
                                    return Request.CreateResponse(HttpStatusCode.Accepted);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Reassignment failed...");
                                }
                            case Common.FormType.FID_TREASURY:
                                var treasuryForm = db.FID_Treasury.FirstOrDefault(x => x.Id == input.FormId);
                                if (treasuryForm != null)
                                {
                                    treasuryForm.ApprovedBy = permittedApprover.Username;
                                    treasuryForm.ApprovedDate = null;
                                    db.SaveChanges();

                                    new WorkflowService().ReassignWorkflow(input.FormId, treasuryForm.FormType);
                                    new MailService().SubmitForApproval(input.FormId, treasuryForm.FormType, permittedApprover.Username, null);
                                    new NotificationService().NotifyApprovalRequest(permittedApprover.Username, input.FormId, User.Identity.Name, treasuryForm.FormType);
                                    return Request.CreateResponse(HttpStatusCode.Accepted);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Reassignment failed...");
                                }
                            case Common.FormType.ISSD_TS:
                                var tsForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == input.FormId);
                                if (tsForm != null)
                                {
                                    tsForm.ApprovedBy = permittedApprover.Username;
                                    tsForm.ApprovedDate = null;
                                    db.SaveChanges();

                                    new WorkflowService().ReassignWorkflow(input.FormId, tsForm.FormType);
                                    new MailService().SubmitForApproval(input.FormId, tsForm.FormType, permittedApprover.Username, null);
                                    new NotificationService().NotifyApprovalRequest(permittedApprover.Username, input.FormId, User.Identity.Name, tsForm.FormType);
                                    return Request.CreateResponse(HttpStatusCode.Accepted);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Reassignment failed...");
                                }
                            default:
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Reassignment failed...");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Reassignment failed...");
                    }
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
        [Route("GetWorkflow/{formTypeId}/{formId}")]
        public HttpResponseMessage GetWorkflow(int formTypeId, int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formTypeParsed = Common.FormTypeMapping(formTypeId);
                    
                    var result = db.Form_Workflow
                        .Where(x => x.FormId == formId && x.FormType == formTypeParsed)
                        .OrderByDescending(x => x.Id).ToList();
                    
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


        #region Private Function

        #endregion
    }
}
