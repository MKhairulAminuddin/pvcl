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
using xDC_Web.ViewModels.Amsd;
using xDC_Web.ViewModels;
using static xDC.Utils.Common;
using xDC_Web.Extension.CustomAttribute;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Services.Form;
using xDC.Domain.Form;
using xDC.Services.Audit;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(PermissionKey.AMSD)]
    [RoutePrefix("api/amsd")]
    public class AmsdController : ApiController
    {
        #region Fields

        private readonly IIfFormService _ifFormService;

        #endregion

        #region Ctor

        public AmsdController(IIfFormService ifFormService)
        {
            _ifFormService = ifFormService;
        }

        #endregion


        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_View)]
        [HttpGet]
        [Route("inflowfund")]
        public HttpResponseMessage InflowFund_Form(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var IfHomeGrid1Data = _ifFormService.GetHomeGrid(User.Identity.Name);

                if (IfHomeGrid1Data != null)
                {
                    return Request.CreateResponse(DataSourceLoader.Load(IfHomeGrid1Data, loadOptions));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error. Please check logs.");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("inflowfund/retractForm")]
        public HttpResponseMessage InflowFund_RetractFormSubmission(RetractFormVM req)
        {
            try
            {
                var retractFormStatus = _ifFormService.WithdrawForm(req.FormId, User.Identity.Name, FormType.AMSD_IF);

                if (retractFormStatus)
                {
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid form ID");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        #region Inflow Fund Form

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("InflowFund/New")]
        public HttpResponseMessage InflowFund_NewForm([FromBody] InflowFundsModel input)
        {
            try
            {
                var ifForm = new AMSD_IF()
                {
                    FormType = FormType.AMSD_IF,
                    Currency = "MYR",
                    PreparedBy = User.Identity.Name,
                    PreparedDate = DateTime.Now,
                    FormDate = DateTime.Now,
                    ApprovedBy = input.Approver,
                };

                var ifFormItems = new List<AMSD_IF_Item>();
                foreach (var item in input.AmsdInflowFunds)
                {
                    ifFormItems.Add(new AMSD_IF_Item()
                    {
                        FundType = item.FundType,
                        Bank = item.Bank,
                        Amount = item.Amount,
                        ModifiedBy = User.Identity.Name,
                        ModifiedDate = DateTime.Now
                    });
                }

                var formCreated = _ifFormService.CreateForm(ifForm, ifFormItems, input.ApprovalNotes, out int createdFormId);

                if (formCreated)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, createdFormId);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to create form");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("InflowFund/Edit/{formId}")]
        public HttpResponseMessage InflowFund_EditForm(int formId, [FromBody] InflowFundsModel input)
        {
            var form = new AMSD_IF()
            {
                Id = formId,

                //check for power admin edit
                AdminEditted = input.IsSaveAdminEdit,
                AdminEdittedBy = input.IsSaveAdminEdit ? User.Identity.Name: null,
                AdminEdittedDate = input.IsSaveAdminEdit ? DateTime.Now:null,

                // check whether draft or reassigning
                FormStatus = input.IsSaveAsDraft ? FormStatus.Draft : input.Approver!= null? FormStatus.PendingApproval:null,

            };

            if (!form.AdminEditted)
            {
                form.PreparedBy = User.Identity.Name;
                form.PreparedDate = DateTime.Now;
            }



            try
            {
                        return Request.CreateResponse(HttpStatusCode.Accepted, form.Id);
                    
                    
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpDelete]
        [Route("InflowFund")]
        public HttpResponseMessage InflowFund_DeleteForm(FormDataCollection input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(input.Get("id"));

                    var form = db.AMSD_IF.FirstOrDefault(x =>
                        x.Id == key && x.FormStatus != Common.FormStatus.PendingApproval);
                    
                    if (form != null)
                    {
                        AuditService.Capture_FA(form.Id, form.FormType, FormActionType.Delete, User.Identity.Name, $"Deleted a {form.FormType} form. (Form status at the moment of deletion is {form.FormStatus}).");

                        var inflowFunds = db.AMSD_IF_Item.Where(x => x.FormId == form.Id);
                        if (inflowFunds.Any())
                        {
                            db.AMSD_IF_Item.RemoveRange(inflowFunds);
                        }
                        
                        db.AMSD_IF.Remove(form);
                        db.SaveChanges();

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

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("InflowFund/Approval")]
        public HttpResponseMessage InflowFund_ApproveForm([FromBody] InflowFundFormApprovalRequest input)
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
                            WorkflowService.ApprovalResponse(form.Id, form.FormStatus, input.ApprovalNote, form.FormType, form.PreparedBy, form.ApprovedBy);
                            AuditService.FA_Approval(form.Id, form.FormType, form.FormStatus, form.FormDate, User.Identity.Name);

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

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_View)]
        [HttpGet]
        [Route("GetInflowFunds/{formId}")]
        public HttpResponseMessage GetInflowFundFormItems(int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = _ifFormService.GetFormItems(formId).OrderBy(x => x.Id).ToList();
                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPut]
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

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
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
                        AuditService.FA_RemoveRow(form.Id, form.FormType, form.FormDate,
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
                            AuditService.FA_EditRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, matchingItem.FundType, item.FundType, "Fund Type");
                        }

                        if (matchingItem.Bank != item.Bank)
                        {
                            matchingItem.Bank = item.Bank;
                            AuditService.FA_EditRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, matchingItem.Bank, item.Bank, "Bank");
                        }

                        if (matchingItem.Amount != item.Amount)
                        {
                            matchingItem.Amount = item.Amount;
                            AuditService.FA_EditRow(form.Id, form.FormType, form.FormDate,
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
                        AuditService.FA_AddRow(form.Id, form.FormType, form.FormDate,
                                User.Identity.Name, 
                                Common.FlattenStrings(newItem.FundType, newItem.Bank, newItem.Amount.ToString()));
                    }
                }
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created);
            }
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpDelete]
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
