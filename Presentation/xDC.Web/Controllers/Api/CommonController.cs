﻿using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using xDC.Domain.WebApi.Administration;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.TaskScheduler;
using xDC.Utils;
using xDC_Web.ViewModels.DealCutOff;

namespace xDC_Web.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/common")]
    public class CommonController : ApiController
    {
        #region Fields

        private readonly IxDcTask _xdcTask = Startup.Container.GetInstance<IxDcTask>();


        #endregion


        #region Active Directory

        [HttpGet]
        [Route("GetActiveDirectory")]
        public HttpResponseMessage GetActiveDirectory(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var users = db.AspNetActiveDirectoryUsers.ToList();

                    var result = users.ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

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

        [HttpPost]
        [Route("GetActiveDirectoryUser")]
        public HttpResponseMessage GetActiveDirectoryUsers([FromBody] UserDetailReq req, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AspNetActiveDirectoryUsers
                        .Where(x => x.Title != null && x.Email == req.Email).FirstOrDefault();

                    if (result != null)
                    {
                        var respObj = new UserDetailRes()
                        {
                            Username = result.Username,
                            Email = result.Email,
                            Department = result.Department,
                            AdType = result.AdType,
                            DisplayName = result.DisplayName,
                            DistinguishedName = result.DistinguishedName,
                            Office = result.Office,
                            TelNo = result.TelNo,
                            Title = result.Title,
                            AdAccountChangedDate = result.AdAccountChanged.HasValue ? result.AdAccountChanged.Value.ToString("dd-MM-yyyy hh:mm tt") : null,
                            LastBadPasswordAttemptDate = result.LastBadPasswordAttempt.HasValue ? result.LastBadPasswordAttempt.Value.ToString("dd-MM-yyyy hh:mm tt") : null,
                            LastLogonDate = result.LastLogon.HasValue ? result.LastLogon.Value.ToString("dd-MM-yyyy hh:mm tt") : null,
                            AdAccountCreatedDate = result.AdAccountCreated.HasValue ? result.AdAccountCreated.Value.ToString("dd-MM-yyyy hh:mm tt") : null,
                            LastPasswordSetDate = result.LastPasswordSet.HasValue ? result.LastPasswordSet.Value.ToString("dd-MM-yyyy hh:mm tt") : null,
                        };


                        return Request.CreateResponse(respObj);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "User not exist!");
                    }

                    
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("SyncActiveDirectory")]
        public HttpResponseMessage SyncActiveDirectory()
        {
            try
            {
                BackgroundJob.Enqueue(() => _xdcTask.SyncKwapAdData());
                return Request.CreateResponse(HttpStatusCode.OK);
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
                    
                    var systemUser = db.AspNetUsers.ToList();
                    
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

        [HttpGet]
        [Route("Users")]
        public HttpResponseMessage GetSystemUsers(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.AspNetUsers
                        .OrderBy(x => x.UserName)
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

                    result.ForEach(c => c.NotificationUrl = (c.NotificationUrl??=""));

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

        #region Audit Trail Information

        [HttpGet]
        [Route("FormAuditTrail/{formTypeId}/{formId}")]
        public HttpResponseMessage GetFormAuditTrail(int formTypeId, int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formTypeParsed = Common.FormTypeMapping(formTypeId);

                    var result = db.Audit_Form
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

        #region Forms Remarks Information

        [HttpGet]
        [Route("FormList/{currencyType}/{formStatus}/{selectedDateEpoch}")]
        public HttpResponseMessage GetFormsList(string currencyType, string formStatus, long selectedDateEpoch, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formsList = new List<FormListVM>() { };
                    var selectedDate = Common.ConvertEpochToDateTime(selectedDateEpoch);
                    var selectedStatus = (formStatus == Common.FormStatus.Approved ? Common.FormStatus.Approved : string.Empty);
                    var selectedCurrency = (currencyType.ToUpper() == "MYR" ? "MYR" : string.Empty);

                    var approvedAmsdForms = db.AMSD_IF.Where(x => DbFunctions.TruncateTime(x.FormDate.Value) == DbFunctions.TruncateTime(selectedDate.Value)).ToList();
                    var approvedIssdForms = db.ISSD_FormHeader.Where(x => DbFunctions.TruncateTime(x.SettlementDate.Value) == DbFunctions.TruncateTime(selectedDate.Value)).ToList();
                    var approvedFidForms = db.FID_Treasury.Where(x => DbFunctions.TruncateTime(x.ValueDate.Value) == DbFunctions.TruncateTime(selectedDate.Value)).ToList();

                    foreach (var approvedAmsdForm in approvedAmsdForms)
                    {
                            formsList.Add(new FormListVM
                            {
                                FormId = approvedAmsdForm.Id,
                                FormType = approvedAmsdForm.FormType,
                                FormStatus = approvedAmsdForm.FormStatus, 
                                FormCurrency = approvedAmsdForm.Currency,
                                PreparedBy = approvedAmsdForm.PreparedBy,
                                ApprovedBy = approvedAmsdForm.ApprovedBy,
                                FormDate = approvedAmsdForm.FormDate,
                                ApprovalDate = approvedAmsdForm.ApprovedDate
                            });
                    }

                    foreach (var approvedIssdForm in approvedIssdForms)
                    {
                        formsList.Add(new FormListVM
                        {
                            FormId = approvedIssdForm.Id,
                            FormType = approvedIssdForm.FormType,
                            FormStatus = approvedIssdForm.FormStatus,
                            FormCurrency = approvedIssdForm.Currency,
                            PreparedBy = approvedIssdForm.PreparedBy,
                            ApprovedBy = approvedIssdForm.ApprovedBy,
                            FormDate = approvedIssdForm.SettlementDate,
                            ApprovalDate = approvedIssdForm.ApprovedDate
                        });
                    }

                    foreach (var approvedFidForm in approvedFidForms)
                    {
                        formsList.Add(new FormListVM
                        {
                            FormId = approvedFidForm.Id,
                            FormType = approvedFidForm.FormType,
                            FormStatus = approvedFidForm.FormStatus,
                            FormCurrency = approvedFidForm.Currency,
                            PreparedBy = approvedFidForm.PreparedBy,
                            ApprovedBy = approvedFidForm.ApprovedBy,
                            FormDate = approvedFidForm.ValueDate,
                            ApprovalDate = approvedFidForm.ApprovedDate
                        });
                    }

                    if (!string.IsNullOrEmpty(selectedStatus))
                    {
                        formsList = formsList.Where(x => x.FormStatus == Common.FormStatus.Approved).ToList();
                    }

                    if (!string.IsNullOrEmpty(selectedCurrency))
                    {
                        formsList = formsList.Where(x => x.FormCurrency == selectedCurrency).ToList();
                    }
                    else
                    {
                        formsList = formsList.Where(x => x.FormCurrency != "MYR").ToList();
                    }

                    formsList.OrderBy(x => x.FormType).ThenBy(x => x.FormDate);

                    return Request.CreateResponse(DataSourceLoader.Load(formsList, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("FormRemarks/{formType}/{formId}")]
        public HttpResponseMessage GetFormsRemarks(string formType, int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formsRemarks = new List<FormRemarkVM>() { };

                    var workflowDetails =
                            (from w in db.Form_Workflow
                             where w.FormId == formId
                             where w.FormType == formType
                             select w).ToList();

                    foreach (var i in workflowDetails)
                    {
                        formsRemarks.Add(new FormRemarkVM()
                        {
                            FormId = i.FormId,
                            FormType = i.FormType,
                            ActionBy = i.RequestBy,
                            ActionDate = i.RecordedDate,
                            Remarks = (!string.IsNullOrEmpty(i.WorkflowNotes) ? i.WorkflowNotes : "N/A")
                        });
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(formsRemarks, loadOptions));
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
