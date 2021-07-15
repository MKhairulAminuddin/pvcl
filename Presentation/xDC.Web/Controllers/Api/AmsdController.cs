using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xDC_Web.Models;
using System.Data.Entity;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using xDC.Utils;
using xDC.Infrastructure.Application;
using xDC.Services;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles="Administrator, Power User, AMSD")]
    [RoutePrefix("api/amsd")]
    public class AmsdController : ApiController
    {
        [HttpGet]
        [Route("GetAmsdForms")]
        public HttpResponseMessage GetAmsdForms(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var amdFormTypes = new List<string>()
                    {
                        Common.FormTypeMapping(1)
                    };

                    var result = db.FormHeader
                        .Where(x => amdFormTypes.Contains(x.FormType)).ToList();

                    var resultVM = new List<ViewFormHeaderModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new ViewFormHeaderModel
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,
                            AdminEditted = item.AdminEditted,
                            AdminEdittedBy = item.AdminEdittedBy,
                            AdminEdittedDate = item.AdminEdittedDate,
                            IsFormOwner = User.Identity.Name == item.PreparedBy
                        });
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(resultVM, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            
        }

        


        [HttpPost]
        [Route("NewInflowFundsForm")]
        public HttpResponseMessage NewInflowFundsForm([FromBody] InflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var newRecord = new FormHeader()
                    {
                        FormType = Common.FormTypeMapping(1),
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = Common.FormStatusMapping(2),
                        ApprovedBy = inputs.Approver
                    };

                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.FormHeader.Add(newRecord);
                    db.SaveChanges();

                    var newRecordInflowFunds = new List<Amsd_InflowFunds>();

                    foreach (var item in inputs.AmsdInflowFunds)
                    {
                        newRecordInflowFunds.Add(new Amsd_InflowFunds()
                        {
                            FormId = newRecord.Id,
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount,
                            CreatedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now
                        });
                    }
                    
                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                    
                    db.Amsd_InflowFunds.AddRange(newRecordInflowFunds);
                    db.SaveChanges();

                    new NotificationService().PushSubmitForApprovalNotification(newRecord.Id);
                    return Request.CreateResponse(HttpStatusCode.Created, newRecord.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            
        }

        [HttpPost]
        [Route("NewInflowFundsFormDraft")]
        public HttpResponseMessage NewInflowFundsFormDraft([FromBody] InflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isExistingDraft = db.FormHeader.FirstOrDefault(x => x.Id == inputs.Id);

                    var newRecord = new FormHeader();

                    if (isExistingDraft != null)
                    {
                        newRecord = new FormHeader()
                        {
                            Id = isExistingDraft.Id,
                            FormType = isExistingDraft.FormType,
                            PreparedBy = User.Identity.Name,
                            PreparedDate = DateTime.Now,
                            FormStatus = Common.FormStatusMapping(0)
                        };

                        Validate(newRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                        
                        db.SaveChanges();
                    }
                    else
                    {
                        newRecord = new FormHeader()
                        {
                            FormType = Common.FormTypeMapping(1),
                            PreparedBy = User.Identity.Name,
                            PreparedDate = DateTime.Now,
                            FormStatus = Common.FormStatusMapping(0)
                        };

                        Validate(newRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.FormHeader.Add(newRecord);
                        db.SaveChanges();

                        var newRecordInflowFunds = new List<Amsd_InflowFunds>();

                        foreach (var item in inputs.AmsdInflowFunds)
                        {
                            newRecordInflowFunds.Add(new Amsd_InflowFunds()
                            {
                                FormId = newRecord.Id,
                                FundType = item.FundType,
                                Bank = item.Bank,
                                Amount = item.Amount,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }

                        Validate(newRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.Amsd_InflowFunds.AddRange(newRecordInflowFunds);
                        db.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.Created, newRecord.Id);
                    
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpPost]
        [Route("InflowFundsFormApproval")]
        public HttpResponseMessage InflowFundsFormApproval([FromBody] ApprovalInflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(inputs.FormId);
                    var form = db.FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form!=null)
                    {
                        if (form.ApprovedBy == User.Identity.Name)
                        {
                            form.ApprovedDate = DateTime.Now;
                            form.FormStatus = (inputs.ApprovalStatus)
                                ? Common.FormStatusMapping(3)
                                : Common.FormStatusMapping(4);

                            // placeholder to keep approval note

                            db.SaveChanges();

                            new NotificationService().PushApprovalStatusNotification(formId);
                            return Request.CreateResponse(HttpStatusCode.Accepted, formId);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Approver");
                        }
                        
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Form");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("GetInflowFunds")]
        public HttpResponseMessage GetInflowFunds(string id, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<Amsd_InflowFunds>();

                    if (!string.IsNullOrEmpty(id))
                    {
                        var formId = Convert.ToInt32(id);
                        result = db.Amsd_InflowFunds
                            .Where(x => x.FormId == formId).ToList();
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }


                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpPut]
        public HttpResponseMessage UpdateInflowFund(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");
                var existingRecord = db.Amsd_InflowFunds.SingleOrDefault(o => o.Id == key);

                JsonConvert.PopulateObject(values, existingRecord);

                if (existingRecord != null)
                {
                    existingRecord.UpdatedBy = User.Identity.Name;
                    existingRecord.UpdatedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        [HttpPost]
        public HttpResponseMessage InsertInflowFund(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var values = form.Get("values");

                var newRecord = new Amsd_InflowFunds();
                JsonConvert.PopulateObject(values, newRecord);
                
                newRecord.CreatedBy = User.Identity.Name;
                newRecord.CreatedDate = DateTime.Now;

                Validate(newRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.Amsd_InflowFunds.Add(newRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, newRecord);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteInflowFund(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var foundRecord = db.Amsd_InflowFunds.First(x => x.Id == key);

                db.Amsd_InflowFunds.Remove(foundRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }

}
