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

        [HttpPost]

        public HttpResponseMessage UpdateTsNotificationSetting(NotificationConfigViewModel req)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    if (req != null)
                    {
                        var config = db.Config_Application.ToList();
                        
                        var cnEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail);
                        if (cnEmailConfig != null)
                        {
                            var cnEmail = String.Join(",", req.tsCnEmail != null ? req.tsCnEmail : new List<string> ());
                            if (cnEmailConfig.Value != cnEmail)
                            {
                                cnEmailConfig.Value = cnEmail;
                                cnEmailConfig.UpdatedBy = User.Identity.Name;
                                cnEmailConfig.UpdatedDate = DateTime.Now;
                            }
                        }

                        var cnEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmailCc);
                        if (cnEmailCcConfig != null)
                        {
                            var cnEmailCc = String.Join(",", req.tsCnEmailCc != null ? req.tsCnEmailCc : new List<string>());
                            if (cnEmailCcConfig.Value != cnEmailCc)
                            {
                                cnEmailCcConfig.Value = cnEmailCc;
                                cnEmailCcConfig.UpdatedBy = User.Identity.Name;
                                cnEmailCcConfig.UpdatedDate = DateTime.Now;
                            }
                        }

                        var peEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmail);
                        if (peEmailConfig != null)
                        {
                            var peEmail = String.Join(",", req.tsPeEmail != null ? req.tsPeEmail : new List<string>());
                            if (peEmailConfig.Value != peEmail)
                            {
                                peEmailConfig.Value = peEmail;
                                peEmailConfig.UpdatedBy = User.Identity.Name;
                                peEmailConfig.UpdatedDate = DateTime.Now;
                            }
                        }
                        
                        var peEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmailCc);
                        if (peEmailCcConfig != null)
                        {
                            var peEmailCc = String.Join(",", req.tsPeEmailCc != null ? req.tsPeEmailCc : new List<string>());
                            if (peEmailCcConfig.Value != peEmailCc)
                            {
                                peEmailCcConfig.Value = peEmailCc;
                                peEmailCcConfig.UpdatedBy = User.Identity.Name;
                                peEmailCcConfig.UpdatedDate = DateTime.Now;
                            }
                        }
                        
                        var propertyEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmail);
                        if (propertyEmailConfig != null)
                        {
                            var propertyEmail = String.Join(",", req.tsPropertyEmail != null ? req.tsPropertyEmail : new List<string>());
                            if (propertyEmailConfig.Value != propertyEmail)
                            {
                                propertyEmailConfig.Value = propertyEmail;
                                propertyEmailConfig.UpdatedBy = User.Identity.Name;
                                propertyEmailConfig.UpdatedDate = DateTime.Now;
                            }
                        }
                        
                        var propertyEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmailCc);
                        if (propertyEmailCcConfig != null)
                        {
                            var propertyEmailCc = String.Join(",", req.tsPropertyEmailCc != null ? req.tsPropertyEmailCc : new List<string>());
                            if (propertyEmailCcConfig.Value != propertyEmailCc)
                            {
                                propertyEmailCcConfig.Value = propertyEmailCc;
                                propertyEmailCcConfig.UpdatedBy = User.Identity.Name;
                                propertyEmailCcConfig.UpdatedDate = DateTime.Now;
                            }
                        }
                        
                        var loanEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmail);
                        if (loanEmailConfig != null)
                        {
                            var loanEmail = String.Join(",", req.tsLoanEmail != null ? req.tsLoanEmail : new List<string>());
                            if (loanEmailConfig.Value != loanEmail)
                            {
                                loanEmailConfig.Value = loanEmail;
                                loanEmailConfig.UpdatedBy = User.Identity.Name;
                                loanEmailConfig.UpdatedDate = DateTime.Now;
                            }
                        }

                        var loanEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmailCc);
                        if (loanEmailCcConfig != null)
                        {
                            var loanEmailCc = String.Join(",", req.tsLoanEmailCc != null ? req.tsLoanEmailCc : new List<string>());
                            if (loanEmailCcConfig.Value != loanEmailCc)
                            {
                                loanEmailCcConfig.Value = loanEmailCc;
                                loanEmailCcConfig.UpdatedBy = User.Identity.Name;
                                loanEmailCcConfig.UpdatedDate = DateTime.Now;
                            }
                        }

                        var fcaTaggingEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_FcaTagging);
                        if (fcaTaggingEmailConfig != null)
                        {
                            var fcaTaggingEmail = String.Join(",", req.tsFcaTaggingEmail != null ? req.tsFcaTaggingEmail : new List<string>());
                            if (fcaTaggingEmailConfig.Value != fcaTaggingEmail)
                            {
                                fcaTaggingEmailConfig.Value = fcaTaggingEmail;
                                fcaTaggingEmailConfig.UpdatedBy = User.Identity.Name;
                                fcaTaggingEmailConfig.UpdatedDate = DateTime.Now;
                            }
                        }
                        
                        var approvedTreasuryEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_TreasuryApproval);
                        if(approvedTreasuryEmailConfig != null)
                        {
                            var approvedTreasuryEmail = String.Join(",", req.tsApprovedTreasury != null ? req.tsApprovedTreasury : new List<string>());
                            if (approvedTreasuryEmailConfig.Value != approvedTreasuryEmail)
                            {
                                approvedTreasuryEmailConfig.Value = approvedTreasuryEmail;
                                approvedTreasuryEmailConfig.UpdatedBy = User.Identity.Name;
                                approvedTreasuryEmailConfig.UpdatedDate = DateTime.Now;
                            }
                        }
                        

                        var cnEmailEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail_Enable);
                        if (cnEmailEnable!=null) cnEmailEnable.Value = req.tsCnEmailEnable.ToString().ToLower();
                        var cnEmailCcEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmailCc_Enable);
                        if (cnEmailCcEnable != null) cnEmailCcEnable.Value = req.tsCnEmailCcEnable.ToString().ToLower();
                        var peEmailEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmail_Enable);
                        if (peEmailEnable != null) peEmailEnable.Value = req.tsPeEmailEnable.ToString().ToLower();
                        var peEmailCcEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmailCc_Enable);
                        if (peEmailCcEnable != null) peEmailCcEnable.Value = req.tsPeEmailCcEnable.ToString().ToLower();
                        var propertyEmailEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmail_Enable);
                        if (propertyEmailEnable != null) propertyEmailEnable.Value = req.tsPropertyEmailEnable.ToString().ToLower();
                        var propertyEmailCcEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmailCc_Enable);
                        if (propertyEmailCcEnable != null) propertyEmailCcEnable.Value = req.tsPropertyEmailCcEnable.ToString().ToLower();
                        var loanEmailEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmail_Enable);
                        if (loanEmailEnable != null) loanEmailEnable.Value = req.tsLoanEmailEnable.ToString().ToLower();
                        var loanEmailCcEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmailCc_Enable);
                        if (loanEmailCcEnable != null) loanEmailCcEnable.Value = req.tsLoanEmailCcEnable.ToString().ToLower();
                        var fcaTaggingEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_FcaTagging_Enable);
                        if (fcaTaggingEnable != null) fcaTaggingEnable.Value = req.tsFcaTaggingEmailEnable.ToString().ToLower();
                        var treasuryApprovalEmailEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_TreasuryApproval_Enable);
                        if (treasuryApprovalEmailEnable != null) treasuryApprovalEmailEnable.Value = req.tsApprovedTreasuryEnable.ToString().ToLower();

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

        [HttpPost]

        public HttpResponseMessage UpdTreasuryFormNotificationSetting(NotificationConfigViewModel req)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    if (req != null)
                    {
                        var config = db.Config_Application.ToList();

                        var tSubmissionEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasurySubmissionCc);
                        if (tSubmissionEmailCcConfig != null)
                        {
                            var tSubmissionEmailCc = String.Join(",", req.tSubmissionEmailCc != null ? req.tSubmissionEmailCc : new List<string>());
                            if (tSubmissionEmailCcConfig.Value != tSubmissionEmailCc)
                            {
                                tSubmissionEmailCcConfig.Value = tSubmissionEmailCc;
                                tSubmissionEmailCcConfig.UpdatedBy = User.Identity.Name;
                                tSubmissionEmailCcConfig.UpdatedDate = DateTime.Now;
                            }
                        }

                        var tApprovedEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasurySubmissionCc);
                        if (tApprovedEmailCcConfig != null)
                        {
                            var tApprovedEmailCc = String.Join(",", req.tApprovedEmailCc != null ? req.tApprovedEmailCc : new List<string>());
                            if (tApprovedEmailCcConfig.Value != tApprovedEmailCc)
                            {
                                tApprovedEmailCcConfig.Value = tApprovedEmailCc;
                                tApprovedEmailCcConfig.UpdatedBy = User.Identity.Name;
                                tApprovedEmailCcConfig.UpdatedDate = DateTime.Now;
                            }
                        }

                        var tSubmissionEmailCcEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasurySubmissionCc_Enable);
                        if (tSubmissionEmailCcEnable != null) tSubmissionEmailCcEnable.Value = req.tSubmissionEmailCcEnable.ToString().ToLower();

                        var tApprovedEmailCcEnable = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.FID_T_TreasuryApprovalCc_Enable);
                        if (tApprovedEmailCcEnable != null) tApprovedEmailCcEnable.Value = req.tApprovedEmailCcEnable.ToString().ToLower();

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
