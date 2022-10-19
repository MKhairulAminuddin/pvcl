﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExpress.XtraRichEdit.Fields;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Models;
using xDC_Web.ViewModels;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Iisd;
using static xDC.Utils.Common;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(PermissionKey.ISSD)]
    [RoutePrefix("api/issd")]
    public class IssdController : ApiController
    {
        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_View)]
        [HttpGet]
        [Route("ts/home/grid1")]
        public HttpResponseMessage TsHomeGrid1(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todayDate = DateTime.Now.Date;
                    
                    var tsForms = db.ISSD_FormHeader;
                    var resultVM = new List<ISSD_LandingPageGridVM>();

                    foreach (var item in tsForms)
                    {
                        resultVM.Add(new ISSD_LandingPageGridVM
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormDate = item.SettlementDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = TradeSettlementFormService.EnableEdit(item.FormStatus, item.ApprovedBy, User.Identity.Name),
                            EnableDelete = item.FormStatus != Common.FormStatus.PendingApproval && item.ApprovedBy != User.Identity.Name,
                            EnablePrint = TradeSettlementFormService.EnablePrint(User.Identity.Name, item.FormStatus),
                            EnableRetractSubmission = TradeSettlementFormService.EnableRetractSubmission(User.Identity.Name, item.PreparedBy, item.FormStatus),

                            IsRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatus.Rejected),
                            IsPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),
                            IsPendingApproval = item.FormStatus == Common.FormStatus.PendingApproval
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

        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [HttpPost]
        [Route("ts/home/retractForm")]
        public HttpResponseMessage TsHomeRetractForm(RetractFormVM req)
        {
            try
            {
                var retractFormStatus = TradeSettlementFormService.RetractFormSubmission(req.FormId, User.Identity.Name);

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

        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_View)]
        [HttpGet]
        [Route("ts/home/grid2")]
        public HttpResponseMessage TsHomeGrid2(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var result = db.ISSD_FormHeader
                        .Where(x => x.FormStatus == Common.FormStatus.Approved)
                        .GroupBy(x => new { x.SettlementDate, x.Currency})
                        .Select(x => new
                        {
                            SettlementDate = x.Key.SettlementDate,
                            Currency = x.Key.Currency,
                            ApprovedDate = x.Max(i=>i.ApprovedDate)
                        });

                    var resultVM = new List<AmsdInflowFundGridModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new AmsdInflowFundGridModel
                        {
                            FormDate = item.SettlementDate,
                            Currency = item.Currency,
                            ApprovedDate = item.ApprovedDate,
                            EnablePrint = TradeSettlementFormService.EnablePrint(User.Identity.Name, FormStatus.Approved)
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
        [Route("ts/approvedTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TS_ApprovedTrades(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                    
                    var trades = TradeSettlementFormService.GetTradeSettlement(db, settlementDate.Value.Date, currency);

                    var result = trades
                        .GroupBy(i => 1)
                        .Select(x => new TS_ApprovedTradeVM()
                        {
                            SettlementDate = settlementDate.Value,
                            Currency = currency,

                            TotalEquity = x.Count(y => y.InstrumentType == Common.TsItemCategory.Equity),
                            TotalBond = x.Count(y => y.InstrumentType == Common.TsItemCategory.Bond),
                            TotalCp = x.Count(y => y.InstrumentType == Common.TsItemCategory.Cp),
                            TotalNotesPapers = x.Count(y => y.InstrumentType == Common.TsItemCategory.NotesPapers),
                            TotalRepo = x.Count(y => y.InstrumentType == Common.TsItemCategory.Repo),
                            TotalCoupon = x.Count(y => y.InstrumentType == Common.TsItemCategory.Coupon),
                            TotalFees = x.Count(y => y.InstrumentType == Common.TsItemCategory.Fees),
                            TotalMtm = x.Count(y => y.InstrumentType == Common.TsItemCategory.Mtm),
                            TotalFx = x.Count(y => y.InstrumentType == Common.TsItemCategory.Fx),
                            TotalCn = x.Count(y => y.InstrumentType == Common.TsItemCategory.Cn),
                            TotalAltid = x.Count(y => y.InstrumentType == Common.TsItemCategory.Altid),
                            TotalOthers = x.Count(y => y.InstrumentType == Common.TsItemCategory.Others),

                            FormIdEquity = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Equity)?.FormId,
                            FormIdBond = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Bond)?.FormId,
                            FormIdCp = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Cp)?.FormId,
                            FormIdNotesPapers = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.NotesPapers)?.FormId,
                            FormIdRepo = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Repo)?.FormId,
                            FormIdCoupon = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Coupon)?.FormId,
                            FormIdFees = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Fees)?.FormId,
                            FormIdMtm = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Mtm)?.FormId,
                            FormIdFx = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Fx)?.FormId,
                            FormIdCn = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Cn)?.FormId,
                            FormIdAltid = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Altid)?.FormId,
                            FormIdOthers = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Others)?.FormId,

                        }).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("ts/approvedTradeItems/{formId}/{instrumentType}")]
        public HttpResponseMessage TS_ApprovedTradeItems(int formId, string instrumentType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeCategory = Common.TsCategoryUrlParamMapping(instrumentType);

                    if (tradeCategory != null)
                    {
                        var result = db.ISSD_TradeSettlement.AsNoTracking()
                            .Where(x => x.InstrumentType == tradeCategory && x.FormId == formId).ToList();

                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("GetIssdForm/{formType}")]
        public HttpResponseMessage GetTradeSettlementForm(int formType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var selectedFormType = Common.FormTypeMapping(formType);

                    var result = db.ISSD_FormHeader
                        .Where(x => x.FormType == selectedFormType);

                    var getApprover = db.Config_Approver.Where(x => x.Username == User.Identity.Name);
                    var isMeApprover = getApprover.Any();

                    var resultVM = new List<ViewTradeSettlementFormHeaderModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new ViewTradeSettlementFormHeaderModel()
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            SettlementDate = item.SettlementDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,
                            AdminEditted = item.AdminEditted,
                            AdminEdittedBy = item.AdminEdittedBy,
                            AdminEdittedDate = item.AdminEdittedDate,

                            IsDraft = (item.FormStatus == Common.FormStatus.Draft),

                            IsMeCanEditDraft = (User.IsInRole(Config.Acl.Issd) && !isMeApprover),


                            IsMyFormRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatus.Rejected),
                            IsFormPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),
                            IsFormOwner = (User.Identity.Name == item.PreparedBy),
                            IsCanAdminEdit = (User.IsInRole(Config.Acl.PowerUser)),
                            IsResubmitEnabled = (item.FormStatus == "Rejected" && User.IsInRole(Config.Acl.Amsd) && User.Identity.Name != item.ApprovedBy)
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
        
        #region Trade Settlement Form

        [HttpPost]
        [Route("TradeSettlement/New")]
        public HttpResponseMessage TS_NewForm([FromBody] TradeSettlementModel inputs)
        {
            try
            {
                var settlementDateConverted = Common.ConvertEpochToDateTime(inputs.SettlementDateEpoch);
                inputs.Currency = inputs.Currency.ToUpper();

                using (var db = new kashflowDBEntities())
                {
                    var newFormHeader = new ISSD_FormHeader()
                    {
                        FormType = Common.FormTypeMapping(inputs.FormType),
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = (inputs.IsSaveAsDraft) ? Common.FormStatus.Draft : Common.FormStatus.PendingApproval,
                        SettlementDate = Common.ConvertEpochToDateTime(inputs.SettlementDateEpoch)?.Date,
                        Currency = inputs.Currency,
                        ApprovedBy = (inputs.IsSaveAsDraft) ? null : inputs.Approver
                    };
                    
                    db.ISSD_FormHeader.Add(newFormHeader);
                    db.SaveChanges();
                    
                    var newTrades = new List<ISSD_TradeSettlement>();

                    if (inputs.Equity.Any())
                    {
                        NewTsObjMapping(inputs.Equity, newFormHeader.Id, Common.TsItemCategory.Equity, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Bond.Any())
                    {
                        NewTsObjMapping(inputs.Bond, newFormHeader.Id, Common.TsItemCategory.Bond, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Cp.Any())
                    {
                        NewTsObjMapping(inputs.Cp, newFormHeader.Id, Common.TsItemCategory.Cp, inputs.Currency, ref newTrades);
                    }

                    if (inputs.NotesPaper.Any())
                    {
                        NewTsObjMapping(inputs.NotesPaper, newFormHeader.Id, Common.TsItemCategory.NotesPapers, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Repo.Any())
                    {
                        NewTsObjMapping(inputs.Repo, newFormHeader.Id, Common.TsItemCategory.Repo, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Coupon.Any())
                    {
                        NewTsObjMapping(inputs.Coupon, newFormHeader.Id, Common.TsItemCategory.Coupon, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Fees.Any())
                    {
                        NewTsObjMapping(inputs.Fees, newFormHeader.Id, Common.TsItemCategory.Fees, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Mtm.Any())
                    {
                        NewTsObjMapping(inputs.Mtm, newFormHeader.Id, Common.TsItemCategory.Mtm, inputs.Currency, ref newTrades);
                    }

                    if (inputs.FxSettlement.Any())
                    {
                        NewTsObjMapping(inputs.FxSettlement, newFormHeader.Id, Common.TsItemCategory.Fx, inputs.Currency, ref newTrades);
                    }

                    if (inputs.ContributionCredited.Any())
                    {
                        NewTsObjMapping(inputs.ContributionCredited, newFormHeader.Id, Common.TsItemCategory.Cn, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Altid.Any())
                    {
                        NewTsObjMapping(inputs.Altid, newFormHeader.Id, Common.TsItemCategory.Altid, inputs.Currency, ref newTrades);
                    }

                    if (inputs.Others.Any())
                    {
                        NewTsObjMapping(inputs.Others, newFormHeader.Id, Common.TsItemCategory.Others, inputs.Currency, ref newTrades);
                    }
                    
                    db.ISSD_TradeSettlement.AddRange(newTrades);
                    db.SaveChanges();

                    new AuditService().Capture_FA(newFormHeader.Id, newFormHeader.FormType, FormActionType.Create, User.Identity.Name, $"Created an {newFormHeader.FormType} form");

                    if (inputs.Approver != null)
                    {
                        CommonService.NotifyApprover(inputs.Approver, newFormHeader.Id, User.Identity.Name, newFormHeader.FormType, inputs.ApprovalNotes);
                        new AuditService().Capture_FA(newFormHeader.Id, newFormHeader.FormType, FormActionType.RequestApproval, User.Identity.Name, $"Request Approval for {newFormHeader.FormType} form");
                    }

                    return Request.CreateResponse(HttpStatusCode.Created, newFormHeader.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpPost]
        [Route("TradeSettlement/Edit")]
        public HttpResponseMessage TS_EditForm([FromBody] TradeSettlementModel inputs)
        {
            try
            {
                var isResubmissionFromApproved = false;

                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == inputs.Id);
                    
                    if (form == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    }
                    
                    if (inputs.IsSaveAdminEdit)
                    {
                        form.AdminEditted = true;
                        form.AdminEdittedBy = User.Identity.Name;
                        form.AdminEdittedDate = DateTime.Now;
                    }

                    if (inputs.IsSaveAsDraft)
                    {
                        form.PreparedBy = User.Identity.Name;
                        form.PreparedDate = DateTime.Now;
                    }

                    if (inputs.Approver != null)
                    {
                        isResubmissionFromApproved = true;

                        new AuditService().AuditForm_ReassignApprover(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                            form.ApprovedBy, inputs.Approver);
                        
                        form.ApprovedBy = inputs.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = Common.FormStatus.PendingApproval;

                        CommonService.NotifyApprover(form.ApprovedBy, form.Id, User.Identity.Name, form.FormType, inputs.ApprovalNotes);
                    }
                    
                    var getTradeItems = db.ISSD_TradeSettlement.Where(x =>
                        x.FormId == form.Id).ToList();

                    var cc_itemBefore = getTradeItems;
                    var cc_itemBeforeList = cc_itemBefore.ToList();

                    if (inputs.Equity != null)
                    {
                        // delete from existing
                        if (inputs.Equity.Any())
                        {
                            var itemExistInGrid =
                                inputs.Equity.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Equity)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Equity - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.StockCode, item.StockCode, "Equity - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "Equity - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Equity - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Equity - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "Equity - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Equity, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" Equity: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Bond != null)
                    {
                        // delete from existing
                        if (inputs.Bond.Any())
                        {
                            var itemExistInGrid =
                                inputs.Bond.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Bond)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "BOND - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.StockCode, item.StockCode, "BOND - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "BOND - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "BOND - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "BOND - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.BondType != item.BondType)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.BondType, item.BondType, "BOND - Bond Type");

                                    foundItem.BondType = item.BondType;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "BOND - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Bond, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" BOND: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.BondType}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Cp != null)
                    {
                        // delete from existing
                        if (inputs.Cp.Any())
                        {
                            var itemExistInGrid =
                                inputs.Cp.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Cp)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "CP - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.StockCode, item.StockCode, "CP - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "CP - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "CP - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "CP - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "CP - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Cp, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" CP: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.NotesPaper != null)
                    {
                        // delete from existing
                        if (inputs.NotesPaper.Any())
                        {
                            var itemExistInGrid =
                                inputs.NotesPaper.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.NotesPaper)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Notes & Paper - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.StockCode, item.StockCode, "Notes & Paper - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "Notes & Paper - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Notes & Paper - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Notes & Paper - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "Notes & Paper - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.NotesPapers, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" Notes & Paper: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Repo != null)
                    {
                        // delete from existing
                        if (inputs.Repo.Any())
                        {
                            var itemExistInGrid =
                                inputs.Repo.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Repo)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "REPO - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.StockCode, item.StockCode, "REPO - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.FirstLeg != item.FirstLeg)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.FirstLeg.ToString(), item.FirstLeg.ToString(), "REPO - First Leg");

                                    foundItem.FirstLeg = item.FirstLeg;
                                }

                                if (foundItem.SecondLeg != item.SecondLeg)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.SecondLeg.ToString(), item.SecondLeg.ToString(), "REPO - Second Leg");

                                    foundItem.SecondLeg = item.SecondLeg;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "REPO - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Repo, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" REPO: {item.InstrumentCode}, {item.StockCode}, {item.FirstLeg}, {item.SecondLeg}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Coupon != null)
                    {
                        // delete from existing
                        if (inputs.Coupon.Any())
                        {
                            var itemExistInGrid =
                                inputs.Coupon.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Coupon)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Coupon - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.StockCode, item.StockCode, "Coupon - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Coupon - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "Coupon - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                if (foundItem.CouponType != item.CouponType)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.CouponType, item.CouponType, "Coupon - Coupon Type");

                                    foundItem.CouponType = item.CouponType;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Coupon, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" Coupon: {item.InstrumentCode}, {item.StockCode}, {item.AmountPlus}, {item.Remarks}, {item.CouponType}");
                            }
                        }
                    }

                    if (inputs.Mtm != null)
                    {
                        // delete from existing
                        if (inputs.Mtm.Any())
                        {
                            var itemExistInGrid =
                                inputs.Mtm.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Mtm)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "MTM - Payment/Receipt");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "MTM - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "MTM - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "MTM - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Mtm, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" MTM: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.FxSettlement != null)
                    {
                        // delete from existing
                        if (inputs.FxSettlement.Any())
                        {
                            var itemExistInGrid =
                                inputs.FxSettlement.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.FxSettlement)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "FX - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "FX - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "FX - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "FX - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Fx, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" FX: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Altid != null)
                    {
                        // delete from existing
                        if (inputs.Altid.Any())
                        {
                            var itemExistInGrid =
                                inputs.Altid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Altid)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "ALTID - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "ALTID - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "ALTID - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "ALTID - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Altid, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" ALTID: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.ContributionCredited != null)
                    {
                        // delete from existing
                        if (inputs.ContributionCredited.Any())
                        {
                            var itemExistInGrid =
                                inputs.ContributionCredited.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.ContributionCredited)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Contribution - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Contribution - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "Contribution - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                                    
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Cn, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" Contribution: {item.InstrumentCode}, {item.AmountPlus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Fees != null)
                    {
                        // delete from existing
                        if (inputs.Fees.Any())
                        {
                            var itemExistInGrid =
                                inputs.Fees.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Fees)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Fees - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Fees - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Fees - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "Fees - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Fees, inputs.Currency));

                                new AuditService().AuditForm_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, User.Identity.Name,
                                    $" Fees: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (inputs.Others != null)
                    {
                        // delete from existing
                        if (inputs.Others.Any())
                        {
                            var itemExistInGrid =
                                inputs.Others.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                getTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    new AuditService().AuditForm_RemoveRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in inputs.Others)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Others - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Others - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Others - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.Remarks, item.Remarks, "Others - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                if (foundItem.OthersType != item.OthersType)
                                {
                                    new AuditService().AuditForm_EditRow(form.Id, form.FormType, form.SettlementDate, User.Identity.Name,
                                        foundItem.OthersType, item.OthersType, "Others - Others Type");

                                    foundItem.OthersType = item.OthersType;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = User.Identity.Name;
                                foundItem.ModifiedDate = DateTime.Now;

                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Others, inputs.Currency));
                            }
                        }
                    }
                    
                    db.SaveChanges();
                    
                    var cc_itemAfterList = db.ISSD_TradeSettlement
                        .Where(x =>
                        x.FormId == form.Id).ToList();

                    if (isResubmissionFromApproved)
                    {
                        new MailService().TS_AmendAfterCutOff(cc_itemBeforeList, cc_itemAfterList, form);
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
        [Route("TradeSettlement/Approval")]
        public HttpResponseMessage TS_Approval([FromBody] FormApprovalModel input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(input.FormId);
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        if (form.ApprovedBy == User.Identity.Name)
                        {
                            form.ApprovedDate = DateTime.Now;
                            form.FormStatus = (input.ApprovalStatus)
                                ? Common.FormStatus.Approved
                                : Common.FormStatus.Rejected;

                            db.SaveChanges();

                            CommonService.NotifyPreparer(form.Id, form.FormType, form.FormStatus, form.PreparedBy, form.ApprovedBy, input.ApprovalNote);
                            new MailService().TS_IncomingFund(form.Id, form.FormType, form.Currency);
                            new AuditService().AuditForm_Approval(form.Id, form.FormType, form.FormStatus, form.SettlementDate, User.Identity.Name);

                            if (form.FormType == Common.FormType.ISSD_TS_E && form.FormStatus == Common.FormStatus.Approved)
                            {
                                new MailService().TS_PartE_NotifyPe(form.Id);
                            }

                            if (form.FormType == Common.FormType.ISSD_TS_H && form.FormStatus == Common.FormStatus.Approved)
                            {
                                // 24/12/2021 - premah requested to remove
                                /*var tsPropertyItemExist = db.ISSD_TradeSettlement.Any(x => x.FormId == form.Id && x.OthersType == Common.TsOthersTypeItem.Property);
                                if (tsPropertyItemExist)
                                {
                                    new MailService().TS_PartH_Notify(form.Id, Common.TsOthersTypeItem.Property);
                                }*/

                                var tsLoanItemExist = db.ISSD_TradeSettlement.Any(x => x.FormId == form.Id && x.OthersType == Common.TsOthersTypeItem.Loan);
                                if (tsLoanItemExist)
                                {
                                    new MailService().TS_PartH_Notify(form.Id, Common.TsOthersTypeItem.Loan);
                                }

                            }

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
        
        [HttpDelete]
        [Authorize(Roles = "Administrator, ISSD")]
        [Route("TradeSettlement")]
        public HttpResponseMessage TS_DeleteForm(FormDataCollection input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(input.Get("id"));
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == key);

                    if (form != null)
                    {
                        db.ISSD_FormHeader.Remove(form);

                        var tradeItems = db.ISSD_TradeSettlement.Where(x => x.FormId == key);
                        if (tradeItems.Any())
                        {
                            db.ISSD_TradeSettlement.RemoveRange(tradeItems);
                        }
                        
                        db.SaveChanges();

                        new AuditService().Capture_FA(form.Id, form.FormType, FormActionType.Delete, User.Identity.Name, $"Deleted {form.FormType} form");

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

        #endregion

        #region Bank Balance

        [HttpGet]
        [Route("GetBalanceConsolidated/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetBalanceConsolidated(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                var settlementDateOnly = settlementDate.Value.Date;

                using (var db = new kashflowDBEntities())
                {
                    var result = TradeSettlementFormService.GetOpeningBalance(db, settlementDateOnly, currency);
                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion

        #region Fetch EDW Data

        [HttpGet]
        [Route("TradeItemDW/{type}/{settlementDateEpoch}")]
        public HttpResponseMessage TradeItemDW(string type, long settlementDateEpoch, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        type = type.ToUpper();
                        settlementDate = settlementDate.Value.Date;

                        var result = db.EDW_TradeItem.AsNoTracking().Where(x => x.InstrumentType == type && x.SettlementDate == settlementDate).ToList();
                        Random rnd = new Random();
                        var resultVM = result.Select(x => new EDW_TradeItemVM()
                        {
                            ISIN = x.ISIN,
                            Currency = x.Currency,
                            InstrumentName = x.InstrumentName,
                            InstrumentType = x.InstrumentType,
                            Amount = x.Amount,
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            SettlementDate = x.SettlementDate,
                            StockCode = x.StockCode,
                            TradeDate = x.TradeDate,
                            Type = x.Type,
                            UpdatedBy = x.UpdatedBy,
                            UpdatedDate = x.UpdatedDate
                        }).ToList();
                        resultVM.ForEach(x => x.Id = rnd.Next(1, 99999));

                        return Request.CreateResponse(DataSourceLoader.Load(resultVM, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("GetOpeningBalanceEdw/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetOpeningBalance(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        settlementDate = settlementDate.Value.Date;

                        var result =
                            db.EDW_BankBalance.AsNoTracking()
                                .Where(x =>
                                    x.SettlementDate == settlementDate && x.Currency == currency)
                                .GroupBy(x => new { x.Currency, x.InstrumentType })
                                .Select(x => new
                                {
                                    instrumentType = x.Key.InstrumentType,
                                    currency = x.Key.Currency,
                                    amount = x.Sum(y => y.Amount)
                                }).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("ts/EdwAvailability/{part}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TS_EdwAvailability(string part, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var instrumentTypes = new List<string>();
                    if (part == "a")
                    {
                        instrumentTypes.Add(Common.TsItemCategory.Equity);
                    }
                    if (part == "b")
                    {
                        instrumentTypes.Add(Common.TsItemCategory.Bond);
                        instrumentTypes.Add(Common.TsItemCategory.Cp);
                        instrumentTypes.Add(Common.TsItemCategory.NotesPapers);
                        instrumentTypes.Add(Common.TsItemCategory.Coupon);
                    }
                    if (part == "c")
                    {
                        instrumentTypes.Add(Common.TsItemCategory.Repo);
                    }


                    var trades = db.EDW_TradeItem
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                    && x.Currency == currency
                                    && instrumentTypes.Contains(x.InstrumentType))
                        .GroupBy(x => x.InstrumentType)
                        .Select(x => new
                        {
                            name = x.Key,
                            count = x.Count()
                        })
                        .ToList();

                    var result = new List<TsEdwAvailability>();
                    if (trades.Count > 0)
                    {
                        foreach (var trade in trades)
                        {
                            result.Add(new TsEdwAvailability
                            {
                                Name = trade.name + " " + currency,
                                Numbers = trade.count,
                                CategoryType = Common.TsReverseInstrumentTypeMapping(trade.name)
                            });
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion
        
        #region Trade Settlement Grid

        [HttpGet]
        [Route("GetTradeSettlementFromEdw/{type}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetTradeSettlementFromEdw(string type, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var excludeRepoCoupon = new List<string>()
                        { Common.TsItemCategory.Repo, Common.TsItemCategory.Coupon };
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        type = type.ToUpper();
                        settlementDate = settlementDate.Value.Date;

                        var result = db.EDW_TradeItem.AsNoTracking().Where(x =>
                            x.InstrumentType == type && x.SettlementDate == settlementDate &&
                            x.Currency == currency);
                        Random rnd = new Random();
                        var resultVM = result.Select(x => new EDW_TradeItemVM()
                        {
                            ISIN = x.ISIN,
                            Currency = x.Currency,
                            InstrumentName = x.InstrumentName,
                            InstrumentType = x.InstrumentType,
                            Amount = x.Amount,
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            SettlementDate = x.SettlementDate,
                            StockCode = x.StockCode,
                            TradeDate = x.TradeDate,
                            Type = x.Type,
                            UpdatedBy = x.UpdatedBy,
                            UpdatedDate = x.UpdatedDate
                        }).ToList();
                        resultVM.ForEach(x => x.Id = rnd.Next(1, 99999));

                        var finalResult = new List<ISSD_TradeSettlement>();
                        foreach (var item in resultVM)
                        {
                            var tradeItem = new ISSD_TradeSettlement();
                            tradeItem.Id = item.Id;
                            tradeItem.InstrumentType = item.InstrumentType;
                            tradeItem.InstrumentCode = item.InstrumentName;
                            tradeItem.StockCode = string.IsNullOrEmpty(item.ISIN) ? item.StockCode : string.Concat(item.StockCode, " / " + item.ISIN);

                            tradeItem.Maturity = item.Type == "M" && !excludeRepoCoupon.Contains(item.InstrumentType) ? item.Amount : 0;
                            tradeItem.Sales = item.Type == "S" && !excludeRepoCoupon.Contains(item.InstrumentType) ? item.Amount : 0;
                            tradeItem.Purchase = item.Type == "P" && !excludeRepoCoupon.Contains(item.InstrumentType) ? item.Amount : 0;
                            tradeItem.SecondLeg = item.InstrumentType == Common.TsItemCategory.Repo ? item.Amount : 0;
                            tradeItem.AmountPlus = item.InstrumentType == Common.TsItemCategory.Coupon ? item.Amount : 0;

                            finalResult.Add(tradeItem);
                        }

                        return Request.CreateResponse(DataSourceLoader.Load(finalResult, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("TradeSettlement/TradeItem/{formId}/{tradeType}")]
        public HttpResponseMessage GetTradeItem(string formId, string tradeType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<ISSD_TradeSettlement>();

                    if (!string.IsNullOrEmpty(formId))
                    {
                        var formIdParsed = Convert.ToInt32(formId);
                        var instrumentType = Common.TsCategoryUrlParamMapping(tradeType);
                        result = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == formIdParsed && x.InstrumentType == instrumentType).ToList();
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

        [HttpGet]
        [Route("TradeSettlement/TradeItemConsolidated/{tradeType}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetTradeItemConsolidated(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                var settlementDateOnly = settlementDate.Value.Date;
                var formType = Common.TsUrlParamInstrumentTypeMapFormType(tradeType);

                using (var db = new kashflowDBEntities())
                {
                    var result = new List<ISSD_TradeSettlement>();

                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x =>
                        DbFunctions.TruncateTime(x.SettlementDate) == settlementDateOnly && x.Currency == currency &&
                        x.FormStatus == "Approved" && x.FormType == formType);

                    if (getForm != null)
                    {
                        var instrumentType = Common.TsCategoryUrlParamMapping(tradeType);
                        result = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == getForm.Id && x.InstrumentType == instrumentType).ToList();
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
        [Route("TradeSettlement/TradeItem/")]
        public HttpResponseMessage UpdateTradeItem(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");
                var existingRecord = db.ISSD_TradeSettlement.SingleOrDefault(o => o.Id == key);

                JsonConvert.PopulateObject(values, existingRecord);

                if (existingRecord != null)
                {
                    existingRecord.ModifiedBy = User.Identity.Name;
                    existingRecord.ModifiedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        #endregion
        
        #region FCA Tagging

        [HttpGet]
        [Route("FcaTagging")]
        public HttpResponseMessage FcaTaggingSummaryList(DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetSummaryForIssdView(out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpGet]
        [Route("FcaTagging/AvailableTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage FcaTagging_AvailableTrades(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetDetailForIssdView(settlementDateEpoch, currency, out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpGet]
        [Route("FcaTaggingGrid/TradeItem/{tradeType}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage FcaTaggingGrid(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetIndividualGridData(tradeType, settlementDateEpoch, currency, out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpPut]
        [Route("FcaTaggingGrid/TradeItem")]
        public HttpResponseMessage FcaTaggingGridUpdate(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var id = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");

                var existingRecord = db.ISSD_TradeSettlement.FirstOrDefault(o => o.Id == id);

                JsonConvert.PopulateObject(values, existingRecord);

                if (existingRecord != null)
                {
                    existingRecord.AssignedBy = User.Identity.Name;
                    existingRecord.AssignedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        [HttpGet]
        [Route("FcaTagging/FcaAccount")]
        public HttpResponseMessage FcaTagging_FcaAccount(DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().List_GetFcaBankAccount(out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        #endregion

        #region Private Methods

        private void NewTsObjMapping(List<Trades> trades, int formId, string category, string currency, ref List<ISSD_TradeSettlement> tsItemObj)
        {
            foreach (var item in trades)
            {
                tsItemObj.Add(new ISSD_TradeSettlement
                {
                    FormId = formId,
                    InstrumentType = category,
                    InstrumentCode = item.InstrumentCode,
                    StockCode = item.StockCode,
                    Maturity = item.Maturity,
                    Sales = item.Sales,
                    Purchase = item.Purchase,
                    FirstLeg = item.FirstLeg,
                    SecondLeg = item.SecondLeg,
                    AmountPlus = item.AmountPlus,
                    AmountMinus = item.AmountMinus,
                    Remarks = item.Remarks,
                    ModifiedBy = User.Identity.Name,
                    ModifiedDate = DateTime.Now,

                    InflowAmount = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg),
                    OutflowAmount = (item.AmountMinus + item.Purchase + item.SecondLeg),

                    InflowTo = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                    OutflowFrom = (item.AmountMinus + item.Purchase + item.SecondLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                    AssignedBy = null,
                    AssignedDate = null,

                    OthersType = item.OthersType,
                    CouponType = item.CouponType,
                    BondType = item.BondType
                });
            }
        }

        private ISSD_TradeSettlement NewTsObjMapping(Trades item, int formId, string category, string currency)
        {
            return new ISSD_TradeSettlement
            {
                FormId = formId,
                InstrumentType = category,
                InstrumentCode = item.InstrumentCode,
                StockCode = item.StockCode,
                Maturity = item.Maturity,
                Sales = item.Sales,
                Purchase = item.Purchase,
                FirstLeg = item.FirstLeg,
                SecondLeg = item.SecondLeg,
                AmountPlus = item.AmountPlus,
                AmountMinus = item.AmountMinus,
                Remarks = item.Remarks,
                ModifiedBy = User.Identity.Name,
                ModifiedDate = DateTime.Now,

                InflowAmount = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg),
                OutflowAmount = (item.AmountMinus + item.Purchase + item.SecondLeg),

                InflowTo = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                OutflowFrom = (item.AmountMinus + item.Purchase + item.SecondLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                AssignedBy = null,
                AssignedDate = null,

                OthersType = item.OthersType,
                CouponType = item.CouponType,
                BondType = item.BondType
            };

        }

        private double SumInflowAmount(Trades item)
        {
            return (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg);
        }

        private double SumOutflowAmount(Trades item)
        {
            return (item.AmountMinus + item.Purchase + item.SecondLeg);
        }

        #endregion

    }
}
