using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExpress.Web.Data;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.Mmi;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("api/fid")]
    public class FidController : ApiController
    {
        #region 10 AM Cut Off

        [HttpGet]
        [Route("Ts10AmAccountAssignment")]
        public HttpResponseMessage Ts10AmAccountAssignment(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_TS10_TradeItem.GroupBy(x => new { x.FormId }).Select(x => new Ts10AmHomeGridVM
                    {
                        FormId = x.Key.FormId,
                        CountPendingEquity = x.Count(y => y.InstrumentType == "EQUITY" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingBond = x.Count(y => y.InstrumentType == "BOND" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingCp = x.Count(y => y.InstrumentType == "COMMERCIAL PAPER" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingNotesPapers = x.Count(y => y.InstrumentType == "NOTES AND PAPERS" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingRepo = x.Count(y => y.InstrumentType == "REPO" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingCoupon = x.Count(y => y.InstrumentType == "COUPON" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingFees = x.Count(y => y.InstrumentType == "FEES" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingMtm = x.Count(y => y.InstrumentType == "PAYMENT/RECEIVED (MTM)" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingFx = x.Count(y => y.InstrumentType == "FX SETTLEMENT" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingContribution = x.Count(y => y.InstrumentType == "CONTRIBUTION CREDITED" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingAltid = x.Count(y => y.InstrumentType == "ALTID DISTRIBUTION AND DRAWDOWN" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingOthers = x.Count(y => y.InstrumentType == "OTHERS" && y.OutflowFrom == null && y.InflowTo == null)
                    }).ToList();

                    var availableForms = db.FID_TS10.ToList();
                    foreach (var form in availableForms)
                    {
                        foreach (var item in result.Where(x => x.FormId == form.Id))
                        {
                            item.Currency = form.Currency;

                            if (form.SettlementDate != null)
                            {
                                item.SettlementDate = form.SettlementDate.Value;
                            }
                        }
                    }

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
        [Route("Ts10AmAccountAssignment/AvailableTrades/{formId}")]
        public HttpResponseMessage Ts10AmAcctAssign_AvailableTrades(int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formIdReq = formId;

                    var form = db.FID_TS10.FirstOrDefault(x => x.Id == formIdReq);

                    var result = db.FID_TS10_TradeItem.Where(x => x.FormId == formIdReq).GroupBy(x => new { x.FormId }).Select(x => new Ts10AmAvailableTradeVM()
                    {
                        FormId = x.Key.FormId,
                        SettlementDate = form.SettlementDate.Value,
                        Currency = form.Currency,
                        
                        Equity = x.Count(y => y.InstrumentType == "EQUITY") > 0,
                        Bond = x.Count(y => y.InstrumentType == "BOND") > 0,
                        Cp = x.Count(y => y.InstrumentType == "COMMERCIAL PAPER") > 0,
                        NotesPapers = x.Count(y => y.InstrumentType == "NOTES AND PAPERS") > 0,
                        Repo = x.Count(y => y.InstrumentType == "REPO") > 0,
                        Coupon = x.Count(y => y.InstrumentType == "COUPON") > 0,
                        Fees = x.Count(y => y.InstrumentType == "FEES") > 0,
                        Mtm = x.Count(y => y.InstrumentType == "PAYMENT/RECEIVED (MTM)") > 0,
                        Fx = x.Count(y => y.InstrumentType == "FX SETTLEMENT") > 0,
                        Contribution = x.Count(y => y.InstrumentType == "CONTRIBUTION CREDITED") > 0,
                        Altid = x.Count(y => y.InstrumentType == "ALTID DISTRIBUTION AND DRAWDOWN") > 0,
                        Others = x.Count(y => y.InstrumentType == "OTHERS") > 0,

                        CountPendingEquity = x.Count(y => y.InstrumentType == "EQUITY" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingBond = x.Count(y => y.InstrumentType == "BOND" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingCp = x.Count(y => y.InstrumentType == "COMMERCIAL PAPER" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingNotesPapers = x.Count(y => y.InstrumentType == "NOTES AND PAPERS" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingRepo = x.Count(y => y.InstrumentType == "REPO" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingCoupon = x.Count(y => y.InstrumentType == "COUPON" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingFees = x.Count(y => y.InstrumentType == "FEES" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingMtm = x.Count(y => y.InstrumentType == "PAYMENT/RECEIVED (MTM)" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingFx = x.Count(y => y.InstrumentType == "FX SETTLEMENT" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingContribution = x.Count(y => y.InstrumentType == "CONTRIBUTION CREDITED" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingAltid = x.Count(y => y.InstrumentType == "ALTID DISTRIBUTION AND DRAWDOWN" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingOthers = x.Count(y => y.InstrumentType == "OTHERS" && y.OutflowFrom == null && y.InflowTo == null)
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
        [Route("Ts10AmAccountAssignmentGrid/TradeItem/{tradeType}/{formId}")]
        public HttpResponseMessage Ts10AmAccountAssignmentGrid(string tradeType, int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeItemType = Common.TradeSettlementUrlParamMapping(tradeType);
                    if (string.IsNullOrEmpty(tradeItemType))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trade Item Type");
                    }

                    var tradeItems =
                        db.FID_TS10_TradeItem.Where(x => x.InstrumentType == tradeItemType && x.FormId == formId).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(tradeItems, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPut]
        [Route("Ts10AmAccountAssignmentGrid/TradeItem")]
        public HttpResponseMessage Ts10AmAccountAssignmentGridUpdate(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var id = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");

                var existingRecord = db.FID_TS10_TradeItem.FirstOrDefault(o => o.Id == id);

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
        [Route("Ts10AmAccountAssignmentGrid/opBalance/{formId}")]
        public HttpResponseMessage Ts10AmAccountAssignmentOpBalanceGrid(int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeItems =
                        db.FID_TS10_OpeningBalance.Where(x => x.FormId == formId && x.FormId == formId).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(tradeItems, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPut]
        [Route("Ts10AmAccountAssignmentGrid/opBalance")]
        public HttpResponseMessage Ts10AmAccountAssignmentOpBalanceGridUpdate(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var id = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");

                var existingRecord = db.FID_TS10_OpeningBalance.FirstOrDefault(o => o.Id == id);

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
        [Route("10AmCutOff/{reportDate}")]
        public HttpResponseMessage TenAmCutOff(long reportDate, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var reportDateParsed = Common.ConvertEpochToDateTime(reportDate);
                reportDateParsed = reportDateParsed.Value.Date;

                using (var db = new kashflowDBEntities())
                {
                    var result = new List<TenAmCutOffItemVM>();

                    var forms = db.FID_TS10.Where(x => DbFunctions.TruncateTime(x.SettlementDate) == reportDateParsed)
                        .ToList();

                    foreach (var form in forms)
                    {
                        var opBalances = db.FID_TS10_OpeningBalance.Where(x => x.FormId == form.Id).ToList();

                        foreach (var opBalance in opBalances)
                        {
                            var item = new TenAmCutOffItemVM();
                            item.Account = opBalance.Account;
                            if (opBalance.Amount != null) item.OpeningBalance = opBalance.Amount.Value;
                            item.Currency = form.Currency;

                            var tradeItemInflow = db.FID_TS10_TradeItem.Where(x => x.FormId == form.Id && x.InflowTo == opBalance.Account).ToList();

                            var tradeItemInflowGrouped = tradeItemInflow
                                .GroupBy(x => new { x.FormId, x.InflowTo }).Select(x => new
                                {
                                    FormId = x.Key.FormId,
                                    InflowAccount = x.Key.InflowTo,
                                    TotalInflow = x.Sum(y => y.AmountPlus) + x.Sum(y => y.FirstLeg) +
                                                  x.Sum(y => y.Maturity) + x.Sum(y => y.Sales)
                                }).FirstOrDefault();

                            if (tradeItemInflowGrouped != null)
                            {
                                if (tradeItemInflowGrouped.TotalInflow != null) item.TotalInflow = tradeItemInflowGrouped.TotalInflow.Value;
                            }

                            var tradeItemOutflow = db.FID_TS10_TradeItem.Where(x => x.FormId == form.Id && x.OutflowFrom == opBalance.Account).ToList();

                            var tradeItemOutflowGrouped = tradeItemOutflow
                                .GroupBy(x => new { x.FormId, x.OutflowFrom }).Select(x => new
                                {
                                    FormId = x.Key.FormId,
                                    OutflowAccount = x.Key.OutflowFrom,
                                    TotalOutflow = x.Sum(y => y.Purchase) + x.Sum(y => y.SecondLeg) +
                                                   x.Sum(y => y.AmountMinus)
                                }).FirstOrDefault();

                            if (tradeItemOutflowGrouped != null)
                            {
                                if (tradeItemOutflowGrouped.TotalOutflow != null) item.TotalOutflow = tradeItemOutflowGrouped.TotalOutflow.Value;
                            }

                            item.Net = item.TotalInflow - item.TotalOutflow;

                            result.Add(item);
                        }
                    }

                    // AMSD - Inflow Funds
                    var submittedAmsdForms = db.Form_Header.FirstOrDefault(x =>
                        DbFunctions.TruncateTime(x.ApprovedDate) == reportDateParsed);
                    if (submittedAmsdForms != null)
                    {
                        var inflowFunds = db.AMSD_InflowFund.Where(x => x.FormId == submittedAmsdForms.Id).GroupBy(x => new { x.Bank}).Select(x => new
                        {
                            Bank = x.Key.Bank,
                            Amount = x.Sum(y => y.Amount)
                        });

                        foreach (var fund in inflowFunds)
                        {
                            var inflowFundsFromAmsd = new TenAmCutOffItemVM()
                            {
                                Account = fund.Bank,
                                Currency = "MYR",
                                OpeningBalance = fund.Amount ?? 0
                            };
                            result.Add(inflowFundsFromAmsd);
                        }
                        
                    }


                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion


        #region Money Market

        [HttpGet]
        [Route("Mmi")]
        public HttpResponseMessage Mmi(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_MMI.Select(x => new MmiGridVm
                    {
                        FormId = x.Id,
                        TradeDate = x.TradeDate,
                        Currency = x.Currency,
                        FormStatus = x.FormStatus,
                        Preparer = x.PreparedBy,
                        PreparedDate = x.PreparedDate,
                        Approver = x.ApprovedBy,
                        ApprovedDate = x.ApprovedDate,

                        IsEditAllowed = false,
                        IsDeleteAllowed = false,
                        IsViewAllowed = false,

                        IsPendingMyApproval = false,
                        IsMyFormRejected = false,
                        IsPendingApproval = false
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

        #endregion

    }
}
