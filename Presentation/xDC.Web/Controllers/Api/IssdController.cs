using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Models;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Iisd;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, ISSD")]
    [RoutePrefix("api/issd")]
    public class IssdController : ApiController
    {
        [HttpGet]
        [Route("TradeSettlement")]
        public HttpResponseMessage TradeSettlement(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formTypes = new List<string>()
                    {
                        Common.FormType.ISSD_TS_A,
                        Common.FormType.ISSD_TS_B,
                        Common.FormType.ISSD_TS_C,
                        Common.FormType.ISSD_TS_D,
                        Common.FormType.ISSD_TS_E,
                        Common.FormType.ISSD_TS_F,
                        Common.FormType.ISSD_TS_G,
                        Common.FormType.ISSD_TS_H,
                    };

                    var todayDate = DateTime.Now.Date;
                    
                    /*var result = db.ISSD_FormHeader
                        .Where(x => formTypes.Contains(x.FormType) && DbFunctions.TruncateTime(x.SettlementDate) >= todayDate);*/
                    var result = db.ISSD_FormHeader
                        .Where(x => formTypes.Contains(x.FormType));

                    var getApprover = db.Config_Approver.Where(x => x.Username == User.Identity.Name);
                    var isMeApprover = getApprover.Any();

                    var resultVM = new List<ISSD_LandingPageGridVM>();

                    foreach (var item in result)
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

                            EnableEdit = TradeSettlementSvc.EnableEdit(item.FormStatus, item.ApprovedBy, User.Identity.Name),
                            EnableDelete = item.FormStatus != Common.FormStatus.PendingApproval && item.ApprovedBy != User.Identity.Name,
                            EnablePrint = TradeSettlementSvc.EnablePrint(item.FormStatus),

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

        [HttpGet]
        [Route("TradeSettlement/Approved")]
        public HttpResponseMessage ApprovedTradeSettlement(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var formTypes = new List<string>()
                    {
                        Common.FormType.ISSD_TS_A,
                        Common.FormType.ISSD_TS_B,
                        Common.FormType.ISSD_TS_C,
                        Common.FormType.ISSD_TS_D,
                        Common.FormType.ISSD_TS_E,
                        Common.FormType.ISSD_TS_F,
                        Common.FormType.ISSD_TS_G,
                        Common.FormType.ISSD_TS_H
                    };

                    var result = db.ISSD_FormHeader
                        .Where(x => formTypes.Contains(x.FormType) && x.FormStatus == Common.FormStatus.Approved)
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
                            ApprovedDate = item.ApprovedDate
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
                    
                    var trades = TradeSettlementSvc.GetTradeSettlement(db, settlementDate.Value.Date, currency);

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
                    if (TradeSettlementSvc.NewFormRules(db, settlementDateConverted.Value.Date, inputs.Currency, Common.FormTypeMapping(inputs.FormType), out var errorMessage))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errorMessage);
                    }

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
                    
                    if (inputs.Approver != null)
                    {
                        TradeSettlementSvc.NotifyApprover(inputs.Approver, newFormHeader.Id, User.Identity.Name, newFormHeader.FormType, inputs.ApprovalNotes);
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

                    if (TradeSettlementSvc.NewFormRules(db, form.SettlementDate.Value, inputs.Currency, Common.FormTypeMapping(inputs.FormType), out var errorMessage))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errorMessage);
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

                        form.ApprovedBy = inputs.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = Common.FormStatus.PendingApproval;

                        TradeSettlementSvc.NotifyApprover(form.ApprovedBy, form.Id, User.Identity.Name,
                            form.FormType, inputs.ApprovalNotes);
                    }
                    
                    var getTradeItems = db.ISSD_TradeSettlement.Where(x =>
                        x.FormId == form.Id);

                    var cc_itemBefore = getTradeItems;
                    var cc_itemBeforeList = cc_itemBefore.ToList();

                    if (inputs.Equity != null)
                    {
                        foreach (var item in inputs.Equity)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.Bond != null)
                    {
                        foreach (var item in inputs.Bond)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.Cp != null)
                    {
                        foreach (var item in inputs.Cp)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.NotesPaper != null)
                    {
                        foreach (var item in inputs.NotesPaper)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.Repo != null)
                    {
                        foreach (var item in inputs.Repo)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.FirstLeg != item.FirstLeg ||
                                    foundItem.SecondLeg != item.SecondLeg ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.FirstLeg = item.FirstLeg;
                                    foundItem.SecondLeg = item.SecondLeg;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Repo, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.Coupon != null)
                    {
                        foreach (var item in inputs.Coupon)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Coupon, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.Mtm != null)
                    {
                        foreach (var item in inputs.Mtm)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                                    
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Mtm, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.FxSettlement != null)
                    {
                        foreach (var item in inputs.FxSettlement)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                                    
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Fx, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.Altid != null)
                    {
                        foreach (var item in inputs.Altid)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Altid, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.ContributionCredited != null)
                    {
                        foreach (var item in inputs.ContributionCredited)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.Remarks = item.Remarks;

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                                    
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Cn, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.Fees != null)
                    {
                        foreach (var item in inputs.Fees)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, inputs.Id, Common.TsItemCategory.Fees, inputs.Currency));
                            }
                        }
                    }

                    if (inputs.Others != null)
                    {
                        foreach (var item in inputs.Others)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode||
                                    foundItem.AmountPlus != item.AmountPlus||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                                
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

                            TradeSettlementSvc.NotifyPreparer(form.Id, form.FormType, form.FormStatus, form.PreparedBy, form.ApprovedBy, input.ApprovalNote);
                            new MailService().TS_IncomingFund(form.Id, form.FormType, form.Currency);

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
                    var result = TradeSettlementSvc.GetOpeningBalance(db, settlementDateOnly, currency);
                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion
        
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
        public HttpResponseMessage TS_EdwAvailability(string part,long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
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

                        var finalResult = new List<ISSD_TradeSettlement>();
                        foreach (var item in result)
                        {
                            var tradeItem = new ISSD_TradeSettlement();
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
        public HttpResponseMessage FcaTaggingLandingPage(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var approvedTsForms = db.ISSD_FormHeader.Where(x => x.FormStatus == Common.FormStatus.Approved).Select(x => x.Id).ToList();

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader,
                            a => a.FormId,
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                            )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved && x.ts_form.SettlementDate != null)
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate,
                            x.ts_form.Currency
                        })
                        .Select(x => new Ts10AmHomeGridVM
                        {
                            Currency = x.Key.Currency,
                            SettlementDate = x.Key.SettlementDate.Value,
                            
                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 )),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 )),

                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                           && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0))
                        })
                        .ToList();

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
        [Route("FcaTagging/AvailableTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage FcaTagging_AvailableTrades(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader,
                            a => a.FormId,
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                        )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved
                                    && x.ts_form.SettlementDate != null
                                    && DbFunctions.TruncateTime(x.ts_form.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                    && x.ts_form.Currency == currency)
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate,
                            x.ts_form.Currency
                        })
                        .Select(x => new Ts10AmAvailableTradeVM()
                        {
                            SettlementDate = x.Key.SettlementDate.Value,
                            Currency = x.Key.Currency,

                            Equity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity) > 0,
                            Bond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond) > 0,
                            Cp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp) > 0,
                            NotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers) > 0,
                            Repo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo) > 0,
                            Coupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon) > 0,
                            Fees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees) > 0,
                            Mtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm) > 0,
                            Fx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx) > 0,
                            Contribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn) > 0,
                            Altid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid) > 0,
                            Others = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others) > 0,

                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0))

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
        [Route("FcaTaggingGrid/TradeItem/{tradeType}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage FcaTaggingGrid(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                    var tradeItemType = Common.TsCategoryUrlParamMapping(tradeType);

                    if (string.IsNullOrEmpty(tradeItemType))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trade Item Type");
                    }

                    var approvedIds = db.ISSD_FormHeader
                        .Where(x => x.FormStatus == Common.FormStatus.Approved
                                             && x.SettlementDate != null
                                             && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                             && x.Currency == currency)
                        .Select(x => x.Id)
                        .ToList();

                    if (!approvedIds.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found");
                    }

                    var tradeItems =
                        db.ISSD_TradeSettlement
                            .Where(x => x.InstrumentType == tradeItemType && approvedIds.Contains(x.FormId))
                            .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(tradeItems, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
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
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = FidService.List_FcaBankAccount(db);

                    result.Add("RENTAS");
                    result.Add("MMA");

                    return Request.CreateResponse(DataSourceLoader.Load(result.Select(x => new { Name = x }).Distinct().OrderBy(x => x.Name), loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        
        #endregion

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
                    AssignedDate = null
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
                    StockCode =  item.StockCode,
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
                    AssignedDate = null
                };
            
        }
    }
}
