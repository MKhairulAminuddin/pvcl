using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Infrastructure.Application;
using xDC.Logging;
using Microsoft.Extensions.Logging;
using xDC.Utils;
using xDC_Web.ViewModels;
using Org.BouncyCastle.Ocsp;
using System.Security.Principal;

namespace xDC_Web.Controllers.Api
{
    [Authorize]
    [Route("api/setting/{action}", Name = "setting")]
    public class SettingController : ApiController
    {

        #region Dropdown Configuration

        [HttpGet]
        public HttpResponseMessage GetDropdownConfig(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Dropdown.ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPut]

        public HttpResponseMessage UpdateDropdownConfig(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var values = form.Get("values");
                    var existingRecord = db.Config_Dropdown.SingleOrDefault(o => o.Id == key);
                    
                    JsonConvert.PopulateObject(values, existingRecord);


                    if (existingRecord != null)
                    {
                        existingRecord.UpdatedBy = User.Identity.Name;
                        existingRecord.UpdatedDate = DateTime.Now;
                        
                        Validate(existingRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage InsertDropdownConfig(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var values = form.Get("values");

                    var newRecord = new Config_Dropdown();
                    JsonConvert.PopulateObject(values, newRecord);

                    newRecord.CreatedBy = User.Identity.Name;
                    newRecord.CreatedDate = DateTime.Now;

                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);


                    db.Config_Dropdown.Add(newRecord);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, newRecord);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteDropdownConfig(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var foundRecord = db.Config_Dropdown.First(x => x.Id == key);

                    db.Config_Dropdown.Remove(foundRecord);
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

        #endregion

        #region Application Config

        [HttpGet]
        public HttpResponseMessage GetApplicationConfig(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Application.ToList();
                    
                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]

        public HttpResponseMessage UpdateApplicationConfig(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var values = form.Get("values");
                    var existingRecord = db.Config_Application.SingleOrDefault(o => o.Id == key);

                    JsonConvert.PopulateObject(values, existingRecord);


                    if (existingRecord != null)
                    {
                        existingRecord.UpdatedBy = User.Identity.Name;
                        existingRecord.UpdatedDate = DateTime.Now;

                        Validate(existingRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage InsertApplicationConfig(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var values = form.Get("values");

                    var newRecord = new Config_Application();
                    JsonConvert.PopulateObject(values, newRecord);

                    newRecord.CreatedBy = User.Identity.Name;
                    newRecord.CreatedDate = DateTime.Now;

                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);


                    db.Config_Application.Add(newRecord);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, newRecord);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteApplicationConfig(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var foundRecord = db.Config_Application.First(x => x.Id == key);

                    db.Config_Application.Remove(foundRecord);
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

        #endregion

        #region Approver List
        
        [HttpGet]
        public HttpResponseMessage GetApprover(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Approver.ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]

        public HttpResponseMessage UpdateApprover(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var values = form.Get("values");
                    var existingRecord = db.Config_Approver.SingleOrDefault(o => o.Id == key);

                    JsonConvert.PopulateObject(values, existingRecord);


                    if (existingRecord != null)
                    {
                        var userDetailsInAd =
                            db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == existingRecord.Username);

                        if (userDetailsInAd == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User not found in AD");
                        }

                        existingRecord.DisplayName = userDetailsInAd.DisplayName;
                        existingRecord.Email = userDetailsInAd.Email;
                        existingRecord.Title = userDetailsInAd.Title;
                        existingRecord.Department = userDetailsInAd.Department;
                        existingRecord.UpdatedBy = User.Identity.Name;
                        existingRecord.UpdatedDate = DateTime.Now;

                        Validate(existingRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage InsertApprover(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var values = form.Get("values");

                    var newRecord = new Config_Approver();
                    JsonConvert.PopulateObject(values, newRecord);
                    
                    Validate(newRecord);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                    
                    var userDetailsInAd =
                        db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == newRecord.Username);

                    if (userDetailsInAd != null)
                    {
                        // check if already existed
                        var existingRecord = db.Config_Approver.FirstOrDefault(o => o.Username == newRecord.Username);
                        if (existingRecord != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Approver already exist! " + userDetailsInAd.DisplayName);
                        }


                        newRecord.DisplayName = userDetailsInAd.DisplayName;
                        newRecord.Email = userDetailsInAd.Email;
                        newRecord.Title = userDetailsInAd.Title;
                        newRecord.Department = userDetailsInAd.Department;
                        newRecord.CreatedDate = DateTime.Now;
                        newRecord.CreatedBy = User.Identity.Name;

                        db.Config_Approver.Add(newRecord);
                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.Created, newRecord);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "User Not Exist!");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteApprover(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var foundRecord = db.Config_Approver.First(x => x.Id == key);

                    db.Config_Approver.Remove(foundRecord);
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

        #endregion

        #region Notification

        [HttpGet]
        public HttpResponseMessage GetInflowFundFormCutOffTime()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Application.FirstOrDefault(x => x.Key == Common.AppConfigKey.AMSD_IF_CutOffTime);

                    if (result != null)
                    {
                        DateTime.TryParseExact(result.Value, "HH:mm",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var tryParseValue);

                        return Request.CreateResponse(HttpStatusCode.Accepted, tryParseValue);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Cut Off Time not defined");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]

        public HttpResponseMessage UpdateInflowFundFormNotificationSetting([FromBody] NotificationConfigViewModel req)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var config = db.Config_Application.ToList();
                    
                    var infFundCutOffTime = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.AMSD_IF_CutOffTime);
                    infFundCutOffTime.Value = req.InflowFundCutOffTime.ToString("HH:mm");

                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Accepted, req);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("TS/EmailNotification")]
        public HttpResponseMessage GetTsNotificationSetting()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_Application.FirstOrDefault(x => x.Key == Common.AppConfigKey.AMSD_IF_CutOffTime);

                    if (result != null)
                    {
                        DateTime.TryParseExact(result.Value, "HH:mm",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var tryParseValue);

                        return Request.CreateResponse(HttpStatusCode.Accepted, tryParseValue);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Cut Off Time not defined");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private Config_Application UpdateEmailNotiConfigValue(Config_Application itemToUpdate, string updatedValue, string updatedBy)
        {
            if (itemToUpdate != null)
            {
                var emailString = String.Join(",", updatedValue ?? null);
                if (itemToUpdate.Value != emailString)
                {
                    itemToUpdate.Value = emailString;
                    itemToUpdate.UpdatedBy = updatedBy;
                    itemToUpdate.UpdatedDate = DateTime.Now;
                }
                return itemToUpdate;
            }
            else
            {
                return itemToUpdate;
            }
        }

        private Config_Application UpdateEnableEmailNotiConfigValue(Config_Application itemToUpdate, bool updatedValue, string updatedBy)
        {
            if (itemToUpdate != null)
            {
                if (itemToUpdate.Value != null)
                {
                    itemToUpdate.Value = updatedValue.ToString().ToLower();
                    itemToUpdate.UpdatedBy = updatedBy;
                    itemToUpdate.UpdatedDate = DateTime.Now;
                }
                return itemToUpdate;
            }
            else
            {
                return itemToUpdate;
            }
        }

        [HttpPost]

        public HttpResponseMessage UpdIssdNotification(NotificationConfigViewModel req)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    if (req != null)
                    {
                        var config = db.Config_Application.ToList();

                        var issd_Ts_CnEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_CnEmail), req.issd_TsCn, User.Identity.Name);
                        var issd_Ts_CnEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_CnEmail_Cc), req.issd_TsCnCc, User.Identity.Name);
                        var issd_Ts_PeEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PeEmail), req.issd_TsPe, User.Identity.Name);
                        var issd_Ts_PeEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PeEmail_Cc), req.issd_TsPeCc, User.Identity.Name);
                        var issd_Ts_PropertyEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PropertyEmail), req.issd_TsProperty, User.Identity.Name);
                        var issd_Ts_PropertyEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_PropertyEmail_Cc), req.issd_TsPropertyCc, User.Identity.Name);
                        var issd_Ts_LoanEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_LoanEmail), req.issd_TsLoan, User.Identity.Name);
                        var issd_Ts_LoanEmailCcConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_TS_LoanEmail_Cc), req.issd_TsLoanCc, User.Identity.Name);
                        var issd_FcaTaggingEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_FcaTagging), req.issd_FcaTagging, User.Identity.Name);
                        var issd_T_ApprovedEmailConfig = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.ISSD_T_Approval), req.issd_TApproved, User.Identity.Name);

                        var enable_issd_Ts_cnEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_CnEmail), req.issd_Enable_TsCn, User.Identity.Name);
                        var enable_issd_Ts_cnEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_CnEmail_Cc), req.issd_Enable_TsCnCc, User.Identity.Name);
                        var enable_issd_Ts_peEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PeEmail), req.issd_Enable_TsPe, User.Identity.Name);
                        var enable_issd_Ts_peEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PeEmail_Cc), req.issd_Enable_TsPeCc, User.Identity.Name);
                        var enable_issd_Ts_propertyEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PropertyEmail), req.issd_Enable_TsProperty, User.Identity.Name);
                        var enable_issd_Ts_propertyEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_PropertyEmail_Cc), req.issd_Enable_TsPropertyCc, User.Identity.Name);
                        var enable_issd_Ts_loanEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_LoanEmail), req.issd_Enable_TsLoan, User.Identity.Name);
                        var enable_issd_Ts_loanEmailCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_TS_LoanEmail_Cc), req.issd_Enable_TsLoanCc, User.Identity.Name);
                        var enable_issd_fcaTagging = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_FcaTagging), req.issd_Enable_FcaTagging, User.Identity.Name);
                        var enable_issd_T_ApprovalEmail = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_ISSD_T_Approval), req.issd_Enable_TApproved, User.Identity.Name);


                        var fid_T_ApprovalCc = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Approval_Cc), req.fid_T_ApprovalCc, User.Identity.Name);
                        var fid_T_SubmissionCc = UpdateEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.FID_T_Submission_Cc), req.fid_T_SubmissionCc, User.Identity.Name);

                        var enable_fid_T_ApprovalCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Approval_Cc), req.fid_Enable_T_ApprovalCc, User.Identity.Name);
                        var enable_fid_T_SubmissionCc = UpdateEnableEmailNotiConfigValue(config.FirstOrDefault(x => x.Key == Common.EmailNotiKey.Enable_FID_T_Submission_Cc), req.fid_Enable_T_SubmissionCc, User.Identity.Name);

                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.Accepted, req);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, req);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
