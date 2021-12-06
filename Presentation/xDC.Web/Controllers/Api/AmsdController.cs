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
using xDC.Services.App;
using xDC_Web.ViewModels.Amsd;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles="Administrator, Power User, AMSD")]
    [RoutePrefix("api/amsd")]
    public class AmsdController : ApiController
    {
        [HttpGet]
        [Route("inflowfund")]
        public HttpResponseMessage InflowFund_Form(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AMSD_IF
                        .Where(x => x.FormType == Common.FormType.AMSD_IF).ToList();

                    var model = new List<AmsdInflowFundGridModel>();

                    foreach (var item in result)
                    {
                        model.Add(new AmsdInflowFundGridModel
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = AMSD_InflowFundSvc.EnableEdit(item.FormStatus, item.ApprovedBy, User.Identity.Name),
                            EnableDelete = AMSD_InflowFundSvc.EnableDelete(item.FormStatus),
                            EnablePrint = AMSD_InflowFundSvc.EnablePrint(item.FormStatus),
                            
                            IsRejected = (item.FormStatus == Common.FormStatus.Rejected),
                            IsPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),
                            
                        });
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(model, loadOptions));
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
                    var existingRecord = db.AMSD_IF.ToList();
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
                    var cutOffTime = db.Config_Application.FirstOrDefault(x => x.Key == Common.AppConfigKey.AMSD_IF_CutOffTime);

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
        
        [HttpPost]
        [Route("InflowFund/New")]
        [Authorize(Roles = "AMSD")]
        public HttpResponseMessage InflowFund_NewForm([FromBody] InflowFundsModel input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = new AMSD_IF
                    {
                        FormType = Common.FormType.AMSD_IF,
                        Currency = "MYR",
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormDate = DateTime.Now
                    };

                    if (!string.IsNullOrEmpty(input.Approver))
                    {
                        form.ApprovedBy = input.Approver;
                        form.FormStatus = Common.FormStatus.PendingApproval;
                    }
                    else
                    {
                        form.FormStatus = Common.FormStatus.Draft;
                    }
                    
                    db.AMSD_IF.Add(form);
                    db.SaveChanges();

                    var newRecordInflowFunds = new List<AMSD_IF_Item>();

                    foreach (var item in input.AmsdInflowFunds)
                    {
                        newRecordInflowFunds.Add(new AMSD_IF_Item()
                        {
                            FormId = form.Id,
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount,
                            ModifiedBy = User.Identity.Name,
                            ModifiedDate = DateTime.Now
                        });
                    }
                    
                    db.AMSD_IF_Item.AddRange(newRecordInflowFunds);
                    db.SaveChanges();

                    new AuditService().AuditForm_Create(form.Id, form.FormType, form.FormDate, User.Identity.Name);

                    if (form.FormStatus == Common.FormStatus.PendingApproval)
                    {
                        new NotificationService().NotifyApprovalRequest(form.ApprovedBy, form.Id, form.PreparedBy, form.FormType);
                        new MailService().SubmitForApproval(form.Id, form.FormType, form.ApprovedBy, input.ApprovalNotes);
                        new WorkflowService().SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);

                        new AuditService().AuditForm_RequestApproval(form.Id, form.FormType, form.FormDate, User.Identity.Name);
                    }

                    
                    
                    return Request.CreateResponse(HttpStatusCode.Created, form.Id);
                    
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        
        [HttpPost]
        [Route("InflowFund/Edit/{formId}")]
        [Authorize(Roles = "AMSD")]
        public HttpResponseMessage InflowFund_EditForm(int formId, [FromBody] InflowFundsModel input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                    
                    if (form != null)
                    {
                        if (User.IsInRole(Config.Acl.PowerUser))
                        {
                            form.AdminEditted = true;
                            form.AdminEdittedBy = User.Identity.Name;
                            form.AdminEdittedDate = DateTime.Now;
                        }
                        else
                        {
                            form.PreparedBy = User.Identity.Name;
                            form.PreparedDate = DateTime.Now;

                            if (!string.IsNullOrEmpty(input.Approver))
                            {
                                form.ApprovedBy = input.Approver;
                                form.FormStatus = Common.FormStatus.PendingApproval;
                                
                            }
                        }
                        
                        if (input.AmsdInflowFunds.Any())
                        {
                            var existingItemInDb = db.AMSD_IF_Item.Where(x => x.FormId == form.Id);
                            var itemInGrid = input.AmsdInflowFunds;

                            //delete existing
                            var existingItemInGrid = itemInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                existingItemInDb.Where(x => !existingItemInGrid.Contains(x.Id));

                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.FormDate, User.Identity.Name,
                                        $"{item.FundType}, {item.Bank}, {item.Amount}");
                                }
                                db.AMSD_IF_Item.RemoveRange(removedItems);
                            }

                            foreach (var item in itemInGrid)
                            {
                                if (item.Id != 0)
                                {
                                    //edit existing
                                    var itemInDb = existingItemInDb.FirstOrDefault(x => x.Id == item.Id);
                                    if (itemInDb != null)
                                    {
                                        if (itemInDb.Amount != item.Amount)
                                        {
                                            new AuditService().AuditForm_EditRow(form.Id, form.FormType,
                                                form.FormDate, User.Identity.Name, itemInDb.Amount.ToString(),
                                                item.Amount.ToString(), "Amount");

                                            itemInDb.Amount = item.Amount;
                                        }
                                        if (itemInDb.Bank != item.Bank)
                                        {
                                            new AuditService().AuditForm_EditRow(form.Id, form.FormType,
                                                form.FormDate, User.Identity.Name, itemInDb.Bank,
                                                item.Bank, "Bank");

                                            itemInDb.Bank = item.Bank;
                                        }
                                        if (itemInDb.FundType != item.FundType)
                                        {
                                            new AuditService().AuditForm_EditRow(form.Id, form.FormType,
                                                form.FormDate, User.Identity.Name, itemInDb.FundType,
                                                item.FundType, "Fund Type");

                                            itemInDb.FundType = item.FundType;
                                        }

                                        itemInDb.ModifiedBy = User.Identity.Name;
                                        itemInDb.ModifiedDate = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    // add new
                                    db.AMSD_IF_Item.Add(new AMSD_IF_Item
                                    {
                                        FormId = form.Id,
                                        FundType = item.FundType,
                                        Bank = item.Bank,
                                        Amount = item.Amount,
                                        ModifiedBy = User.Identity.Name,
                                        ModifiedDate = DateTime.Now
                                    });
                                    new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                                form.FormDate, User.Identity.Name,
                                                $"{item.FundType}, {item.Bank}, {item.Amount}");
                                }
                            }

                        }
                        
                        db.SaveChanges();

                        if (form.FormStatus == Common.FormStatus.PendingApproval)
                        {
                            new NotificationService().NotifyApprovalRequest(form.ApprovedBy, form.Id, form.PreparedBy, form.FormType);
                            new MailService().SubmitForApproval(form.Id, form.FormType, form.ApprovedBy, input.ApprovalNotes);
                            new WorkflowService().SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);

                            new AuditService().AuditForm_RequestApproval(form.Id, form.FormType, form.FormDate, User.Identity.Name);
                        }

                        return Request.CreateResponse(HttpStatusCode.Accepted, form.Id);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpDelete]
        [Authorize(Roles = "Administrator, AMSD")]
        [Route("InflowFund")]
        public HttpResponseMessage InflowFund_DeleteForm(FormDataCollection input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(input.Get("id"));

                    var form = db.AMSD_IF.FirstOrDefault(x =>
                        x.Id == key && x.FormStatus != Common.FormStatus.PendingApproval &&
                        x.FormStatus != Common.FormStatus.Approved);
                    
                    if (form != null)
                    {
                        var inflowFunds = db.AMSD_IF_Item.Where(x => x.FormId == form.Id);
                        if (inflowFunds.Any())
                        {
                            db.AMSD_IF_Item.RemoveRange(inflowFunds);
                        }
                        
                        db.AMSD_IF.Remove(form);
                        db.SaveChanges();

                        new AuditService().AuditForm_Delete(form.Id, form.FormType, form.FormDate, User.Identity.Name);

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }


        [HttpPost]
        [Route("InflowFund/Approval")]
        [Authorize(Roles = "AMSD")]
        public HttpResponseMessage InflowFund_ApproveForm([FromBody] FormApprovalModel input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == input.FormId);

                    if (form != null)
                    {
                        if (form.ApprovedBy == User.Identity.Name)
                        {
                            form.ApprovedDate = DateTime.Now;
                            form.FormStatus = (input.ApprovalStatus)
                                ? Common.FormStatus.Approved
                                : Common.FormStatus.Rejected;
                            
                            db.SaveChanges();

                            new NotificationService().NotifyApprovalResult(form.PreparedBy, form.Id, form.ApprovedBy, form.FormType, form.FormStatus);
                            new MailService().SendApprovalStatus(form.Id, form.FormType, form.FormStatus, form.PreparedBy, input.ApprovalNote);
                            new WorkflowService().ApprovalResponse(form.Id, form.FormStatus, input.ApprovalNote, form.FormType, form.PreparedBy, form.ApprovedBy);
                            new AuditService().AuditForm_Approval(form.Id, form.FormType, form.FormStatus, form.FormDate, User.Identity.Name);

                            if (form.FormStatus == Common.FormStatus.Approved)
                            {
                                new NotificationService().NotifyNewApproveInflowFund(form.Id, form.FormType);
                            }
                            
                            return Request.CreateResponse(HttpStatusCode.Accepted, input.FormId);
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
        [Route("GetInflowFunds/{formId}")]
        public HttpResponseMessage GetInflowFunds(int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.AMSD_IF_Item
                        .Where(x => x.FormId == formId).ToList();
                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
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
                    var existingRecord = db.AMSD_IF_Item.SingleOrDefault(o => o.Id == key);

                    JsonConvert.PopulateObject(values, existingRecord);

                    if (existingRecord != null)
                    {
                        existingRecord.ModifiedBy = User.Identity.Name;
                        existingRecord.ModifiedDate = DateTime.Now;
                    }

                    var formHeader = db.AMSD_IF.FirstOrDefault(x => x.Id == existingRecord.FormId);
                    var isAdminEdit = User.IsInRole(Config.Acl.PowerUser);
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
        public HttpResponseMessage InsertInflowFund([FromBody] InflowFundVM request)
        {
            using (var db = new kashflowDBEntities())
            {
                var form = db.AMSD_IF.FirstOrDefault(x => x.Id == request.Id);

                var existingItems = db.AMSD_IF_Item.Where(x => x.FormId == request.Id).ToList();

                //delete existing item
                var removedItems = existingItems.Where(x => !request.AmsdInflowFunds.Select(y => y.Id).Contains(x.Id)).ToList();
                if (removedItems.Any())
                {
                    foreach (var item in removedItems)
                    {
                        new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name,
                                Common.FlattenStrings(item.FundType, item.Bank, item.Amount.ToString()));
                    }

                    db.AMSD_IF_Item.RemoveRange(removedItems);
                }
                db.SaveChanges();

                foreach (var item in request.AmsdInflowFunds)
                {
                    var matchingItem = existingItems.FirstOrDefault(x => x.Id == item.Id);

                    if (matchingItem != null)
                    {
                        //edit existing item
                        if (matchingItem.FundType != item.FundType)
                        {
                            matchingItem.FundType = item.FundType;
                            new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, matchingItem.FundType, item.FundType, "Fund Type");
                        }

                        if (matchingItem.Bank != item.Bank)
                        {
                            matchingItem.Bank = item.Bank;
                            new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, matchingItem.Bank, item.Bank, "Bank");
                        }

                        if (matchingItem.Amount != item.Amount)
                        {
                            matchingItem.Amount = item.Amount;
                            new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, matchingItem.Amount.ToString(), item.Amount.ToString(), "Amount");
                        }

                        matchingItem.ModifiedBy = User.Identity.Name;
                        matchingItem.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        //new
                        var newItem = new AMSD_IF_Item()
                        {
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount,
                            ModifiedBy = User.Identity.Name,
                            ModifiedDate = DateTime.Now,
                            FormId = form.Id
                        };
                        db.AMSD_IF_Item.Add(newItem);
                        new AuditService().AuditForm_AddRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, 
                                Common.FlattenStrings(newItem.FundType, newItem.Bank, newItem.Amount.ToString()));
                    }
                }
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Power User, AMSD")]
        public HttpResponseMessage DeleteInflowFund(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var foundRecord = db.AMSD_IF_Item.First(x => x.Id == key);

                var formHeader = db.AMSD_IF.FirstOrDefault(x => x.Id == foundRecord.FormId);
                var isAdminEdit = User.IsInRole(Config.Acl.PowerUser);
                if (isAdminEdit)
                {
                    formHeader.AdminEditted = true;
                    formHeader.AdminEdittedBy = User.Identity.Name;
                    formHeader.AdminEdittedDate = DateTime.Now;
                }

                db.AMSD_IF_Item.Remove(foundRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion



    }

}
