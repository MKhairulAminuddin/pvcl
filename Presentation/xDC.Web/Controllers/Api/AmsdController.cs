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
using DevExpress.Utils;
using Newtonsoft.Json;
using xDC.Utils;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC_Web.Extension.CustomAttribute;

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

                    var result = db.Form_Header
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

                            IsMyFormRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatusMapping(4)),
                            IsFormPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatusMapping(2)),
                            IsFormOwner = (User.Identity.Name == item.PreparedBy),
                            IsCanAdminEdit = (User.IsInRole(Config.AclPowerUser)),
                            IsResubmitEnabled = (item.FormStatus == "Rejected" && User.IsInRole(Config.AclAmsd) && User.Identity.Name != item.ApprovedBy)
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

        [HttpGet]
        [Route("IsTodayInflowFormExisted")]
        public HttpResponseMessage IsTodayInflowFormExisted()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todaysDate = DateTime.Now.Date;
                    var existingRecord = db.Form_Header.ToList();
                    existingRecord = existingRecord.Where(x => x.PreparedDate.Value.Date == todaysDate.Date).ToList();

                    if (existingRecord.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("IsViolatedCutOffTime")]
        public HttpResponseMessage IsViolatedCutOffTime()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var configKey = Common.ApplicationConfigKeyMapping(1);
                    var cutOffTime = db.Config_Application.FirstOrDefault(x => x.Key == configKey);

                    if (cutOffTime != null)
                    {
                        var cutOffTimeParsed = TimeSpan.Parse(cutOffTime.Value);
                        var isViolateCutOffTime = DateTime.Now.TimeOfDay > cutOffTimeParsed;

                        if (isViolateCutOffTime)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, isViolateCutOffTime);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, isViolateCutOffTime);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Hmm.. I don't know the cut off time tho...");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        #region Inflow Fund Form

        /*
         * Function to cater : 
         * 1) new form submission
         * 2) Resubmission from rejected form
         *
         */
        [HttpPost]
        [Route("NewInflowFundsForm")]
        [Authorize(Roles = "AMSD")]
        public HttpResponseMessage NewInflowFundsForm([FromBody] InflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isFormResubmission = inputs.Id != 0;
                    
                    if (isFormResubmission)
                    {
                        var existingForm = db.Form_Header.FirstOrDefault(x => x.Id == inputs.Id);

                        if (existingForm == null)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Form not found!");

                        existingForm.PreparedBy = User.Identity.Name;
                        existingForm.PreparedDate = DateTime.Now;
                        existingForm.FormStatus = Common.FormStatusMapping(2);
                        existingForm.ApprovedBy = inputs.Approver;
                        existingForm.ApprovedDate = null;
                        existingForm.AdminEditted = false;
                        existingForm.AdminEdittedBy = null;
                        existingForm.AdminEdittedDate = null;
                        
                        db.SaveChanges();


                        new NotificationService().PushSubmitForApprovalNotification(existingForm.Id);
                        new NotificationService().PushInflowFundAfterCutOffSubmissionNotification(existingForm.Id);
                        new WorkflowService().SubmitForApprovalWorkflow(existingForm.Id);

                        return Request.CreateResponse(HttpStatusCode.Created, existingForm.Id);
                    }
                    else
                    {
                        var newRecord = new Form_Header
                        {
                            FormType = Common.FormTypeMapping(1),
                            PreparedBy = User.Identity.Name,
                            PreparedDate = DateTime.Now,
                            FormStatus = Common.FormStatusMapping(2),
                            ApprovedBy = inputs.Approver,
                            Currency = Common.FormCurrencyMapping(1)
                        };

                        Validate(newRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.Form_Header.Add(newRecord);
                        db.SaveChanges();

                        var newRecordInflowFunds = new List<AMSD_InflowFund>();

                        foreach (var item in inputs.AmsdInflowFunds)
                        {
                            newRecordInflowFunds.Add(new AMSD_InflowFund()
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

                        db.AMSD_InflowFund.AddRange(newRecordInflowFunds);
                        db.SaveChanges();

                        new NotificationService().PushSubmitForApprovalNotification(newRecord.Id);
                        new NotificationService().PushInflowFundAfterCutOffSubmissionNotification(newRecord.Id);
                        new WorkflowService().SubmitForApprovalWorkflow(newRecord.Id);

                        return Request.CreateResponse(HttpStatusCode.Created, newRecord.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /*
         * To save current form as Draft. Only applicable for form that is New or Rejected (for resubmission)
         *
         */
        [HttpPost]
        [Route("NewInflowFundsFormDraft")]
        [Authorize(Roles = "AMSD")]
        public HttpResponseMessage NewInflowFundsFormDraft([FromBody] InflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var isExistingDraft = db.Form_Header.FirstOrDefault(x => x.Id == inputs.Id);

                    var newRecord = new Form_Header();

                    if (isExistingDraft != null)
                    {
                        newRecord = new Form_Header()
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
                        newRecord = new Form_Header()
                        {
                            FormType = Common.FormTypeMapping(1),
                            PreparedBy = User.Identity.Name,
                            PreparedDate = DateTime.Now,
                            FormStatus = Common.FormStatusMapping(0)
                        };

                        Validate(newRecord);

                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                        db.Form_Header.Add(newRecord);
                        db.SaveChanges();

                        var newRecordInflowFunds = new List<AMSD_InflowFund>();

                        foreach (var item in inputs.AmsdInflowFunds)
                        {
                            newRecordInflowFunds.Add(new AMSD_InflowFund()
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

                        db.AMSD_InflowFund.AddRange(newRecordInflowFunds);
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

        /*
         * To delete draft form
         *
         */
        [HttpDelete]
        [Authorize(Roles = "Administrator, AMSD")]
        [Route("DeleteInflowFundDraftForm")]
        public HttpResponseMessage DeleteInflowFundDraftForm(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("id"));

                    var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == key);
                    var inflowFunds = db.AMSD_InflowFund.Where(x => x.FormId == formHeader.Id);

                    //only authorized person to delete
                    if (formHeader.PreparedBy != User.Identity.Name)
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }

                    db.Form_Header.Remove(formHeader);
                    db.AMSD_InflowFund.RemoveRange(inflowFunds);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }


        [HttpPost]
        [Route("InflowFundsFormApproval")]
        [Authorize(Roles = "AMSD")]
        public HttpResponseMessage InflowFundsFormApproval([FromBody] ApprovalInflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(inputs.FormId);
                    var form = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        if (form.ApprovedBy == User.Identity.Name)
                        {
                            form.ApprovedDate = DateTime.Now;
                            form.FormStatus = (inputs.ApprovalStatus)
                                ? Common.FormStatusMapping(3)
                                : Common.FormStatusMapping(4);
                            
                            db.SaveChanges();

                            new NotificationService().PushApprovalStatusNotification(formId);
                            new NotificationService().PushInflowFundAfterCutOffSubmissionNotification(formId);
                            new WorkflowService().ApprovalFeedbackWorkflow(formId, inputs.ApprovalStatus, inputs.ApprovalNote);

                            return Request.CreateResponse(HttpStatusCode.Accepted, formId);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Unauthorized Approver!");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not Found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        #endregion

        #region Inflow Funds Grid

        [HttpGet]
        [Route("GetInflowFunds")]
        public HttpResponseMessage GetInflowFunds(string id, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<AMSD_InflowFund>();

                    if (!string.IsNullOrEmpty(id))
                    {
                        var formId = Convert.ToInt32(id);
                        result = db.AMSD_InflowFund
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
        [Authorize(Roles = "Power User, AMSD")]
        public HttpResponseMessage UpdateInflowFund(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("key"));
                    var values = form.Get("values");
                    var existingRecord = db.AMSD_InflowFund.SingleOrDefault(o => o.Id == key);

                    JsonConvert.PopulateObject(values, existingRecord);

                    if (existingRecord != null)
                    {
                        existingRecord.UpdatedBy = User.Identity.Name;
                        existingRecord.UpdatedDate = DateTime.Now;
                    }

                    var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == existingRecord.FormId);
                    var isAdminEdit = User.IsInRole(Config.AclPowerUser);
                    if (isAdminEdit)
                    {
                        formHeader.AdminEditted = true;
                        formHeader.AdminEdittedBy = User.Identity.Name;
                        formHeader.AdminEdittedDate = DateTime.Now;
                    }

                    Validate(existingRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Power User, AMSD")]
        public HttpResponseMessage InsertInflowFund(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var values = form.Get("values");

                var newRecord = new AMSD_InflowFund();
                JsonConvert.PopulateObject(values, newRecord);

                newRecord.CreatedBy = User.Identity.Name;
                newRecord.CreatedDate = DateTime.Now;

                var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == newRecord.FormId);
                var isAdminEdit = User.IsInRole(Config.AclPowerUser);
                if (isAdminEdit)
                {
                    formHeader.AdminEditted = true;
                    formHeader.AdminEdittedBy = User.Identity.Name;
                    formHeader.AdminEdittedDate = DateTime.Now;
                }

                Validate(newRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.AMSD_InflowFund.Add(newRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, newRecord);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Power User, AMSD")]
        public HttpResponseMessage DeleteInflowFund(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var foundRecord = db.AMSD_InflowFund.First(x => x.Id == key);

                var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == foundRecord.FormId);
                var isAdminEdit = User.IsInRole(Config.AclPowerUser);
                if (isAdminEdit)
                {
                    formHeader.AdminEditted = true;
                    formHeader.AdminEdittedBy = User.Identity.Name;
                    formHeader.AdminEdittedDate = DateTime.Now;
                }

                db.AMSD_InflowFund.Remove(foundRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion



    }

}
