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
        [Route("TS/Notifications")]
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

        public HttpResponseMessage UpdateTsNotificationSetting([FromBody] NotificationConfigViewModel req)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    if (req != null)
                    {
                        var config = db.Config_Application.ToList();
                        
                        var cnEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmail);
                        var cnEmail = String.Join(";", req.tsCnEmail);
                        if (cnEmailConfig.Value != cnEmail)
                        {
                            cnEmailConfig.Value = cnEmail;
                            cnEmailConfig.UpdatedBy = User.Identity.Name;
                            cnEmailConfig.UpdatedDate = DateTime.Now;
                        }

                        var cnEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_CnEmailCc);
                        var cnEmailCc = String.Join(";", req.tsCnEmailCc);
                        if (cnEmailCcConfig.Value != cnEmailCc)
                        {
                            cnEmailCcConfig.Value = cnEmail;
                            cnEmailCcConfig.UpdatedBy = User.Identity.Name;
                            cnEmailCcConfig.UpdatedDate = DateTime.Now;
                        }

                        var peEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmail);
                        var peEmail = String.Join(";", req.tsPeEmail);
                        if (peEmailConfig.Value != peEmail)
                        {
                            peEmailConfig.Value = cnEmail;
                            peEmailConfig.UpdatedBy = User.Identity.Name;
                            peEmailConfig.UpdatedDate = DateTime.Now;
                        }

                        var peEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PeEmailCc);
                        var peEmailCc = String.Join(";", req.tsPeEmailCc);
                        if (peEmailCcConfig.Value != peEmailCc)
                        {
                            peEmailCcConfig.Value = peEmailCc;
                            peEmailCcConfig.UpdatedBy = User.Identity.Name;
                            peEmailCcConfig.UpdatedDate = DateTime.Now;
                        }

                        var propertyEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmail);
                        var propertyEmail = String.Join(";", req.tsPropertyEmail);
                        if (propertyEmailConfig.Value != propertyEmail)
                        {   
                            propertyEmailConfig.Value = propertyEmail;
                            propertyEmailConfig.UpdatedBy = User.Identity.Name;
                            propertyEmailConfig.UpdatedDate = DateTime.Now;
                        }

                        var propertyEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_PropertyEmailCc);
                        var propertyEmailCc = String.Join(";", req.tsPropertyEmailCc);
                        if (propertyEmailCcConfig.Value != propertyEmailCc)
                        {
                            propertyEmailCcConfig.Value = propertyEmailCc;
                            propertyEmailCcConfig.UpdatedBy = User.Identity.Name;
                            propertyEmailCcConfig.UpdatedDate = DateTime.Now;
                        }

                        var loanEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmail);
                        var loanEmail = String.Join(";", req.tsLoanEmail);
                        if (loanEmailConfig.Value != loanEmail)
                        {
                            loanEmailConfig.Value = loanEmail;
                            loanEmailConfig.UpdatedBy = User.Identity.Name;
                            loanEmailConfig.UpdatedDate = DateTime.Now;
                        }

                        var loanEmailCcConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_LoanEmailCc);
                        var loanEmailCc = String.Join(";", req.tsLoanEmailCc);
                        if (loanEmailCcConfig.Value != loanEmailCc)
                        {
                            loanEmailCcConfig.Value = loanEmailCc;
                            loanEmailCcConfig.UpdatedBy = User.Identity.Name;
                            loanEmailCcConfig.UpdatedDate = DateTime.Now;
                        }

                        var fcaTaggingEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_FcaTagging);
                        var fcaTaggingEmail = String.Join(";", req.tsFcaTaggingEmail);
                        if (fcaTaggingEmailConfig.Value != fcaTaggingEmail)
                        {
                            fcaTaggingEmailConfig.Value = fcaTaggingEmail;
                            fcaTaggingEmailConfig.UpdatedBy = User.Identity.Name;
                            fcaTaggingEmailConfig.UpdatedDate = DateTime.Now;
                        }

                        var approvedTreasuryEmailConfig = config.FirstOrDefault(x => x.Key == Common.AppConfigKey.ISSD_TS_TreasuryApproval);
                        var approvedTreasuryEmail = String.Join(";", req.tsApprovedTreasury);
                        if (approvedTreasuryEmailConfig.Value != approvedTreasuryEmail)
                        {   
                            approvedTreasuryEmailConfig.Value = approvedTreasuryEmail;
                            approvedTreasuryEmailConfig.UpdatedBy = User.Identity.Name;
                            approvedTreasuryEmailConfig.UpdatedDate = DateTime.Now;
                        }

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
