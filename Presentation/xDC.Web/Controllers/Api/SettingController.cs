using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Application;
using xDC.Services.Form;
using xDC_Web.Extension.CustomAttribute;
using static xDC.Utils.Common;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(PermissionKey.Settings)]
    [Route("api/setting/{action}", Name = "setting")]
    public class SettingController : ApiController
    {
        #region Fields

        private readonly ISettingService _settingService = Startup.Container.GetInstance<ISettingService>();

        #endregion


        #region Dropdown Configuration

        [HttpGet]
        public HttpResponseMessage GetDropdownConfig(DataSourceLoadOptions loadOptions)
        {
            var result = _settingService.DropdownConfig();
            return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
        }


        [HttpPut]
        public HttpResponseMessage UpdateDropdownConfig(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");

            var updateDropdownStatus = _settingService.DropdownConfig_Update(key, values, User.Identity.Name);
            if (!updateDropdownStatus) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "System unable to update the setting. Please seek system admin.");

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage InsertDropdownConfig(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");

                var newRecord = new Config_Dropdown();
                JsonConvert.PopulateObject(values, newRecord);
                Validate(newRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                var saveStatus = _settingService.DropdownConfig_Add(newRecord, User.Identity.Name);
                if (!saveStatus) return Request.CreateResponse(HttpStatusCode.BadRequest, "System unable to save record. Please check with system admin.");

                return Request.CreateResponse(HttpStatusCode.Created, newRecord);
            }
            catch (Exception ex)
            {
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #endregion
    }
}
