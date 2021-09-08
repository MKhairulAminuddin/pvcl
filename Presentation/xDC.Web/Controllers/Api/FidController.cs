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
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Models;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.Treasury;
using TreasuryFormVM = xDC_Web.Models.TreasuryFormVM;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("api/fid")]
    public class FidController : ApiController
    {
        #region 10 AM Cut Off

        [HttpGet]
        [Route("FcaTagging")]
        public HttpResponseMessage TcaTaggingLandingPage(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_TS10_TradeItem.GroupBy(x => new { x.FormId }).Select(x => new Ts10AmHomeGridVM
                    {
                        FormId = x.Key.FormId,

                        CountPendingEquity = x.Count(y => y.InstrumentType == "EQUITY" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingBond = x.Count(y => y.InstrumentType == "BOND" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingCp = x.Count(y => y.InstrumentType == "COMMERCIAL PAPER" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingNotesPapers = x.Count(y => y.InstrumentType == "NOTES AND PAPERS" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingRepo = x.Count(y => y.InstrumentType == "REPO" && ((y.InflowTo == null && y.FirstLeg > 0) || (y.OutflowFrom == null && y.SecondLeg > 0))),
                        CountPendingMtm = x.Count(y => y.InstrumentType == "PAYMENT/RECEIVED (MTM)" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),
                        CountPendingFx = x.Count(y => y.InstrumentType == "FX SETTLEMENT" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),
                        CountPendingAltid = x.Count(y => y.InstrumentType == "ALTID DISTRIBUTION AND DRAWDOWN" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),
                        CountPendingOthers = x.Count(y => y.InstrumentType == "OTHERS" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),

                        //xde outflow for this table
                        CountPendingCoupon = x.Count(y => y.InstrumentType == "COUPON" && y.InflowTo == null && y.AmountPlus > 0),
                        CountPendingContribution = x.Count(y => y.InstrumentType == "CONTRIBUTION CREDITED" && y.InflowTo == null && y.AmountPlus > 0),
                        CountPendingFees = x.Count(y => y.InstrumentType == "FEES" && y.InflowTo == null && y.AmountPlus > 0)
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
        [Route("TcaTagging/AvailableTrades/{formId}")]
        public HttpResponseMessage TcaTagging_AvailableTrades(int formId, DataSourceLoadOptions loadOptions)
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

                        CountPendingEquity = x.Count(y => y.InstrumentType == "EQUITY" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingBond = x.Count(y => y.InstrumentType == "BOND" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingCp = x.Count(y => y.InstrumentType == "COMMERCIAL PAPER" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingNotesPapers = x.Count(y => y.InstrumentType == "NOTES AND PAPERS" && ((y.InflowTo == null && (y.Maturity + y.Sales) > 0) || (y.OutflowFrom == null && y.Purchase > 0))),
                        CountPendingRepo = x.Count(y => y.InstrumentType == "REPO" && ((y.InflowTo == null && y.FirstLeg > 0) || (y.OutflowFrom == null && y.SecondLeg > 0))),
                        CountPendingMtm = x.Count(y => y.InstrumentType == "PAYMENT/RECEIVED (MTM)" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),
                        CountPendingFx = x.Count(y => y.InstrumentType == "FX SETTLEMENT" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),
                        CountPendingAltid = x.Count(y => y.InstrumentType == "ALTID DISTRIBUTION AND DRAWDOWN" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),
                        CountPendingOthers = x.Count(y => y.InstrumentType == "OTHERS" && ((y.InflowTo == null && y.AmountPlus > 0) || (y.OutflowFrom == null && y.AmountMinus > 0))),

                        //xde outflow for this table
                        CountPendingCoupon = x.Count(y => y.InstrumentType == "COUPON" && y.InflowTo == null && y.AmountPlus > 0),
                        CountPendingContribution = x.Count(y => y.InstrumentType == "CONTRIBUTION CREDITED" && y.InflowTo == null && y.AmountPlus > 0),
                        CountPendingFees = x.Count(y => y.InstrumentType == "FEES" && y.InflowTo == null && y.AmountPlus > 0)

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
        [Route("TcaTaggingGrid/TradeItem/{tradeType}/{formId}")]
        public HttpResponseMessage TcaTaggingGrid(string tradeType, int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeItemType = Common.TsCategoryUrlParamMapping(tradeType);
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
        [Route("TcaTaggingGrid/TradeItem")]
        public HttpResponseMessage TcaTaggingGridUpdate(FormDataCollection form)
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
        [Route("TcaTagging/EdwAccount")]
        public HttpResponseMessage TcaTagging_EdwAccount(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = FidService.List_CounterParty(db);

                    var rentas = new EDW_FID_List()
                    {
                        Name = "RENTAS",
                        Reference = "RENTAS"
                    };

                    var MMA = new EDW_FID_List()
                    {
                        Name = "MMA",
                        Reference = "MMA"
                    };

                    result.Add(rentas);
                    result.Add(MMA);

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
        [Route("10AmCutOff/{reportDate}")]
        public HttpResponseMessage SummaryReport(long reportDate, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var reportDateParsed = Common.ConvertEpochToDateTime(reportDate);
                reportDateParsed = reportDateParsed.Value.Date;

                using (var db = new kashflowDBEntities())
                {
                    var result = new List<TenAmCutOffItemVM>();
                    
                    var opBalances = FcaTaggingSvc.GetOpeningBalance(db, reportDateParsed.Value);
                    foreach (var opBalance in opBalances)
                    {
                        // create row for account and its opening balance. e.g. RENTAS - OB 20,000
                        var item = new TenAmCutOffItemVM
                        {
                            Account = opBalance.Account,
                            OpeningBalance = opBalance.Amount,
                            Currency = opBalance.Currency
                        };
                        result.Add(item);
                    }
                    
                    foreach (var item in result)
                    {
                        // get total inflow based on assigned inflow account
                        var form = db.FID_TS10.FirstOrDefault(x => DbFunctions.TruncateTime(x.SettlementDate) == reportDateParsed && x.Currency == item.Currency);
                        if (form != null)
                        {
                            var tradeItemInflow = db.FID_TS10_TradeItem.Where(x => x.FormId == form.Id && x.InflowTo == item.Account).ToList();

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

                            // get total inflow based on assigned outflow account
                            var tradeItemOutflow = db.FID_TS10_TradeItem.Where(x => x.FormId == form.Id && x.OutflowFrom == item.Account).ToList();

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
                        }

                        item.Net = item.OpeningBalance + item.TotalInflow - item.TotalOutflow;
                    }

                    // AMSD - Inflow Funds
                    var submittedAmsdForms = db.AMSD_IF.FirstOrDefault(x =>
                        DbFunctions.TruncateTime(x.ApprovedDate) == reportDateParsed);
                    if (submittedAmsdForms != null)
                    {
                        var inflowFunds = db.AMSD_IF_Item.Where(x => x.FormId == submittedAmsdForms.Id).GroupBy(x => new { x.Bank}).Select(x => new
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
                                OpeningBalance = fund.Amount ?? 0,
                                Net = fund.Amount ?? 0
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


        #region Treasury

        [HttpGet]
        [Route("Treasury")]
        public HttpResponseMessage TreasuryIndex(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury.Select(x => new TreasuryGridVm
                    {
                        Id = x.Id,
                        TradeDate = x.TradeDate,
                        Currency = x.Currency,
                        FormStatus = x.FormStatus,
                        PreparedBy = x.PreparedBy,
                        PreparedDate = x.PreparedDate,
                        ApprovedBy = x.ApprovedBy,
                        ApprovedDate = x.ApprovedDate,

                        IsEditAllowed = x.FormStatus != Common.FormStatus.PendingApproval && x.ApprovedBy != User.Identity.Name,
                        IsDeleteAllowed = x.FormStatus != Common.FormStatus.PendingApproval && x.ApprovedBy != User.Identity.Name,
                        IsViewAllowed = false,

                        IsPendingMyApproval = x.FormStatus == Common.FormStatus.PendingApproval && x.ApprovedBy == User.Identity.Name,
                        IsMyFormRejected = x.FormStatus == Common.FormStatus.Rejected && x.PreparedBy == User.Identity.Name,
                        IsPendingApproval = x.FormStatus == Common.FormStatus.PendingApproval
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
        [Route("Treasury/EdwMaturity/{tradeDateEpoch}/{Currency}")]
        public HttpResponseMessage Treasury_EdwMaturity(long tradeDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);
                    tradeDate = tradeDate.Value.Date;
                    var result = db.EDW_Maturity.Where(x => DbFunctions.TruncateTime(x.Value_Date) == tradeDate && x.currency == currency).Select(
                        x => new TreasuryDepositGridVm
                        {
                            Dealer = x.Operator,
                            Bank = x.Bank,
                            ValueDate = x.Value_Date.Value,
                            MaturityDate = x.Maturity_Date,
                            Principal = x.Principle.Value,
                            Tenor = x.Tenor.Value,
                            RatePercent = x.Rate.Value,
                            IntProfitReceivable = 0,
                            PrincipalIntProfitReceivable = 0,
                            AssetType = x.Asset_Type,
                            RepoTag = null,
                            ContactPerson = null,
                            Notes = null
                        }).ToList();

                    foreach (var item in result)
                    {
                        var rate = (double) item.RatePercent / 100;
                        var tenor = (double) item.Tenor / 365;

                        item.IntProfitReceivable = item.Principal * tenor * rate;
                        item.PrincipalIntProfitReceivable = item.Principal + item.IntProfitReceivable;
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
        [Route("Treasury/EdwBankCounterParty")]
        public HttpResponseMessage Treasury_EdwBankCounterParty(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = FidService.List_CounterParty(db);

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
        [Route("Treasury/EdwIssuer")]
        public HttpResponseMessage Treasury_EdwIssuer(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = FidService.List_Issuer(db);

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
        [Route("Treasury/EdwMaturity/AvailableMaturity")]
        public HttpResponseMessage Treasury_EdwAvailableMaturity(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.EDW_Maturity.Select(x => new
                    {
                        day = x.Value_Date.Value.Day,
                        month = x.Value_Date.Value.Month,
                        date = x.Value_Date.Value
                    }).Distinct().ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("Treasury/EdwMaturity/AvailableMaturity/{tradeDateEpoch}")]
        public HttpResponseMessage Treasury_EdwAvailableMaturityCurrency(long tradeDateEpoch, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);
                    tradeDate = tradeDate.Value.Date;

                    var result = db.EDW_Maturity.Where(x => DbFunctions.TruncateTime(x.Value_Date.Value) == DbFunctions.TruncateTime(tradeDate)).Select(x => x.currency).Distinct().ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpGet]
        [Route("Treasury/inflow/deposit/{formId}")]
        public HttpResponseMessage Treasury_Inflow_Deposit(long formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury_Deposit
                        .Where(x => x.CashflowType == Common.Cashflow.Inflow && x.FormId == formId)
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
        [Route("Treasury/inflow/mmi/{formId}")]
        public HttpResponseMessage Treasury_Inflow_Mmi(long formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury_MMI
                        .Where(x => x.CashflowType == Common.Cashflow.Inflow && x.FormId == formId)
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
        [Route("Treasury/outflow/deposit/{formId}")]
        public HttpResponseMessage Treasury_Outflow_Deposit(long formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury_Deposit
                        .Where(x => x.CashflowType == Common.Cashflow.Outflow && x.FormId == formId)
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
        [Route("Treasury/outflow/mmi/{formId}")]
        public HttpResponseMessage Treasury_Outflow_Mmi(long formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury_MMI
                        .Where(x => x.CashflowType == Common.Cashflow.Outflow && x.FormId == formId)
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


        // form area
        [HttpPost]
        [Route("Treasury/New")]
        public HttpResponseMessage Treasury_FormNew([FromBody] TreasuryFormVM input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDateConverted = Common.ConvertEpochToDateTime(input.TradeDate);
                    tradeDateConverted = tradeDateConverted.Value.Date;

                    var form = new FID_Treasury
                    {
                        FormType = Common.FormType.FID_TREASURY,
                        FormStatus = !string.IsNullOrEmpty(input.Approver) ? Common.FormStatus.PendingApproval : Common.FormStatus.Draft,
                        Currency = input.Currency,
                        TradeDate = tradeDateConverted,
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,

                        ApprovedBy = !string.IsNullOrEmpty(input.Approver) ? input.Approver : null
                    };
                    db.FID_Treasury.Add(form);
                    db.SaveChanges();

                    var inflowDeposit = new List<FID_Treasury_Deposit>();
                    if (input.InflowDeposit.Any())
                    {
                        foreach (var item in input.InflowDeposit)
                        {
                            inflowDeposit.Add(new FID_Treasury_Deposit
                            {
                                FormId = form.Id,
                                CashflowType = Common.Cashflow.Inflow,
                                Dealer = item.Dealer,
                                Bank = item.Bank,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                Principal = item.Principal,
                                Tenor = item.Tenor,
                                RatePercent = item.RatePercent,
                                IntProfitReceivable = item.IntProfitReceivable,
                                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                AssetType = item.AssetType,
                                RepoTag = item.RepoTag,
                                ContactPerson = item.ContactPerson,
                                Notes = item.Notes,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_Deposit.AddRange(inflowDeposit);
                    db.SaveChanges();

                    var outflowDeposit = new List<FID_Treasury_Deposit>();
                    if (input.OutflowDeposit.Any())
                    {
                        foreach (var item in input.OutflowDeposit)
                        {
                            outflowDeposit.Add(new FID_Treasury_Deposit
                            {
                                FormId = form.Id,
                                CashflowType = Common.Cashflow.Outflow,
                                Dealer = item.Dealer,
                                Bank = item.Bank,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                Tenor = item.Tenor,
                                Principal = item.Principal,
                                RatePercent = item.RatePercent,
                                IntProfitReceivable = item.IntProfitReceivable,
                                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                AssetType = item.AssetType,
                                RepoTag = item.RepoTag,
                                ContactPerson = item.ContactPerson,
                                Notes = item.Notes,

                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_Deposit.AddRange(outflowDeposit);
                    db.SaveChanges();

                    var inflowMoneyMarket = new List<FID_Treasury_MMI>();
                    if (input.InflowMoneyMarket.Any())
                    {
                        foreach (var item in input.InflowMoneyMarket)
                        {
                            inflowMoneyMarket.Add(new FID_Treasury_MMI
                            {
                                FormId = form.Id,
                                CashflowType = Common.Cashflow.Inflow,
                                Dealer = item.Dealer,
                                Issuer = item.Issuer,
                                ProductType = item.ProductType,
                                CounterParty = item.CounterParty,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                HoldingDayTenor = item.HoldingDayTenor,
                                Nominal = item.Nominal,
                                SellPurchaseRateYield = item.SellPurchaseRateYield,
                                Price = item.Price,
                                IntDividendReceivable = item.IntDividendReceivable,
                                Proceeds = item.Proceeds,
                                PurchaseProceeds = item.PurchaseProceeds,
                                CertNoStockCode = item.CertNoStockCode,

                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_MMI.AddRange(inflowMoneyMarket);
                    db.SaveChanges();

                    var outflowMoneyMarket = new List<FID_Treasury_MMI>();
                    if (input.OutflowMoneyMarket.Any())
                    {
                        foreach (var item in input.OutflowMoneyMarket)
                        {
                            outflowMoneyMarket.Add(new FID_Treasury_MMI
                            {
                                FormId = form.Id,
                                CashflowType = Common.Cashflow.Outflow,
                                Dealer = item.Dealer,
                                Issuer = item.Issuer,
                                ProductType = item.ProductType,
                                CounterParty = item.CounterParty,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                HoldingDayTenor = item.HoldingDayTenor,
                                Nominal = item.Nominal,
                                SellPurchaseRateYield = item.SellPurchaseRateYield,
                                Price = item.Price,
                                IntDividendReceivable = item.IntDividendReceivable,
                                Proceeds = item.Proceeds,
                                PurchaseProceeds = item.PurchaseProceeds,
                                CertNoStockCode = item.CertNoStockCode,

                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_MMI.AddRange(outflowMoneyMarket);
                    db.SaveChanges();

                    if (form.FormStatus == Common.FormStatus.PendingApproval)
                    {
                        new WorkflowService().SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);
                        new MailService().SubmitForApproval(form.Id, form.FormType, form.ApprovedBy, input.ApprovalNotes);
                        new NotificationService().NotifyApprovalRequest(form.ApprovedBy, form.Id, form.PreparedBy, form.FormType);
                    }
                    
                    return Request.CreateResponse(HttpStatusCode.Created, form.Id);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [Route("Treasury/Edit")]
        public HttpResponseMessage Treasury_FormEdit([FromBody] TreasuryFormVM input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == input.Id);

                    if (form == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    }

                    if (string.IsNullOrEmpty(input.Approver))
                    {
                        form.PreparedBy = User.Identity.Name;
                        form.PreparedDate = DateTime.Now;
                    }

                    if (input.Approver != null)
                    {
                        form.ApprovedBy = input.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = Common.FormStatus.PendingApproval;

                        new WorkflowService().SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);
                        new MailService().SubmitForApproval(form.Id, form.FormType, form.ApprovedBy, input.ApprovalNotes);
                        new NotificationService().NotifyApprovalRequest(form.ApprovedBy, form.Id, form.PreparedBy, form.FormType);
                    }
                    
                    if (input.InflowDeposit.Any())
                    {
                        var inflowDepositInGrid = input.InflowDeposit;
                        var existingInflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Inflow);

                        // delete from existing
                        var itemExistInGrid =
                            inflowDepositInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingInflowDeposit.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(removedItems);
                        }

                        foreach (var item in inflowDepositInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingInflowDeposit.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.AssetType != item.AssetType)
                                    {
                                        foundItem.AssetType = item.AssetType;
                                    }
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                    }
                                    if (foundItem.Bank != item.Bank)
                                    {
                                        foundItem.Bank = item.Bank;
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                    }
                                    if (foundItem.Tenor != item.Tenor)
                                    {
                                        foundItem.Tenor = item.Tenor;
                                    }
                                    if (foundItem.Principal != item.Principal)
                                    {
                                        foundItem.Principal = item.Principal;
                                    }
                                    if (foundItem.RatePercent != item.RatePercent)
                                    {
                                        foundItem.RatePercent = item.RatePercent;
                                    }
                                    if (foundItem.IntProfitReceivable != item.IntProfitReceivable)
                                    {
                                        foundItem.IntProfitReceivable = item.IntProfitReceivable;
                                    }
                                    if (foundItem.PrincipalIntProfitReceivable != item.PrincipalIntProfitReceivable)
                                    {
                                        foundItem.PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable;
                                    }
                                    if (foundItem.AssetType != item.AssetType)
                                    {
                                        foundItem.AssetType = item.AssetType;
                                    }
                                    if (foundItem.RepoTag != item.RepoTag)
                                    {
                                        foundItem.RepoTag = item.RepoTag;
                                    }
                                    if (foundItem.ContactPerson != item.ContactPerson)
                                    {
                                        foundItem.ContactPerson = item.ContactPerson;
                                    }
                                    if (foundItem.Notes != item.Notes)
                                    {
                                        foundItem.Notes = item.Notes;
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_Deposit.Add(new FID_Treasury_Deposit
                                {
                                    FormId = form.Id,
                                    CashflowType = Common.Cashflow.Inflow,
                                    Dealer = item.Dealer,
                                    Bank = item.Bank,
                                    ValueDate = item.ValueDate,
                                    MaturityDate = item.MaturityDate,
                                    Tenor = item.Tenor,
                                    Principal = item.Principal,
                                    RatePercent = item.RatePercent,
                                    IntProfitReceivable = item.IntProfitReceivable,
                                    PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                    AssetType = item.AssetType,
                                    RepoTag = item.RepoTag,
                                    ContactPerson = item.ContactPerson,
                                    Notes = item.Notes,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }
                    
                    if (input.OutflowDeposit.Any())
                    {
                        var outflowDepositInGrid = input.OutflowDeposit;
                        var existingOutflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Outflow);

                        // delete from existing
                        var itemExistInGrid =
                            outflowDepositInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingOutflowDeposit.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(removedItems);
                        }

                        foreach (var item in outflowDepositInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingOutflowDeposit.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.AssetType != item.AssetType)
                                    {
                                        foundItem.AssetType = item.AssetType;
                                    }
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                    }
                                    if (foundItem.Bank != item.Bank)
                                    {
                                        foundItem.Bank = item.Bank;
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                    }
                                    if (foundItem.Tenor != item.Tenor)
                                    {
                                        foundItem.Tenor = item.Tenor;
                                    }
                                    if (foundItem.Principal != item.Principal)
                                    {
                                        foundItem.Principal = item.Principal;
                                    }
                                    if (foundItem.RatePercent != item.RatePercent)
                                    {
                                        foundItem.RatePercent = item.RatePercent;
                                    }
                                    if (foundItem.IntProfitReceivable != item.IntProfitReceivable)
                                    {
                                        foundItem.IntProfitReceivable = item.IntProfitReceivable;
                                    }
                                    if (foundItem.PrincipalIntProfitReceivable != item.PrincipalIntProfitReceivable)
                                    {
                                        foundItem.PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable;
                                    }
                                    if (foundItem.AssetType != item.AssetType)
                                    {
                                        foundItem.AssetType = item.AssetType;
                                    }
                                    if (foundItem.RepoTag != item.RepoTag)
                                    {
                                        foundItem.RepoTag = item.RepoTag;
                                    }
                                    if (foundItem.ContactPerson != item.ContactPerson)
                                    {
                                        foundItem.ContactPerson = item.ContactPerson;
                                    }
                                    if (foundItem.Notes != item.Notes)
                                    {
                                        foundItem.Notes = item.Notes;
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_Deposit.Add(new FID_Treasury_Deposit
                                {
                                    FormId = form.Id,
                                    CashflowType = Common.Cashflow.Outflow,
                                    Dealer = item.Dealer,
                                    Bank = item.Bank,
                                    ValueDate = item.ValueDate,
                                    MaturityDate = item.MaturityDate,
                                    Tenor = item.Tenor,
                                    Principal = item.Principal,
                                    RatePercent = item.RatePercent,
                                    IntProfitReceivable = item.IntProfitReceivable,
                                    PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                    AssetType = item.AssetType,
                                    RepoTag = item.RepoTag,
                                    ContactPerson = item.ContactPerson,
                                    Notes = item.Notes,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }
                    
                    if (input.InflowMoneyMarket.Any())
                    {
                        var inflowMmiInGrid = input.InflowMoneyMarket;
                        var existingInflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Inflow);

                        // delete from existing
                        var itemExistInGrid =
                            inflowMmiInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingInflowMmi.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(removedItems);
                        }

                        foreach (var item in inflowMmiInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingInflowMmi.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                    }
                                    if (foundItem.Issuer != item.Issuer)
                                    {
                                        foundItem.Issuer = item.Issuer;
                                    }
                                    if (foundItem.ProductType != item.ProductType)
                                    {
                                        foundItem.ProductType = item.ProductType;
                                    }
                                    if (foundItem.CounterParty != item.CounterParty)
                                    {
                                        foundItem.CounterParty = item.CounterParty;
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                    }
                                    if (foundItem.HoldingDayTenor != item.HoldingDayTenor)
                                    {
                                        foundItem.HoldingDayTenor = item.HoldingDayTenor;
                                    }
                                    if (foundItem.Nominal != item.Nominal)
                                    {
                                        foundItem.Nominal = item.Nominal;
                                    }
                                    if (foundItem.SellPurchaseRateYield != item.SellPurchaseRateYield)
                                    {
                                        foundItem.SellPurchaseRateYield = item.SellPurchaseRateYield;
                                    }
                                    if (foundItem.Price != item.Price)
                                    {
                                        foundItem.Price = item.Price;
                                    }
                                    if (foundItem.IntDividendReceivable != item.IntDividendReceivable)
                                    {
                                        foundItem.IntDividendReceivable = item.IntDividendReceivable;
                                    }
                                    if (foundItem.Proceeds != item.Proceeds)
                                    {
                                        foundItem.Proceeds = item.Proceeds;
                                    }
                                    if (foundItem.PurchaseProceeds != item.PurchaseProceeds)
                                    {
                                        foundItem.PurchaseProceeds = item.PurchaseProceeds;
                                    }
                                    if (foundItem.CertNoStockCode != item.CertNoStockCode)
                                    {
                                        foundItem.CertNoStockCode = item.CertNoStockCode;
                                    }
                                    
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_MMI.Add(new FID_Treasury_MMI
                                {
                                    FormId = form.Id,
                                    CashflowType = Common.Cashflow.Inflow,
                                    Dealer = item.Dealer,
                                    Issuer = item.Issuer,
                                    ProductType = item.ProductType,
                                    CounterParty = item.CounterParty,
                                    ValueDate = item.ValueDate,
                                    MaturityDate = item.MaturityDate,
                                    HoldingDayTenor = item.HoldingDayTenor,
                                    Nominal = item.Nominal,
                                    SellPurchaseRateYield = item.SellPurchaseRateYield,
                                    Price = item.Price,
                                    IntDividendReceivable = item.IntDividendReceivable,
                                    Proceeds = item.Proceeds,
                                    PurchaseProceeds = item.PurchaseProceeds,
                                    CertNoStockCode = item.CertNoStockCode,

                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }
                    
                    if (input.OutflowMoneyMarket.Any())
                    {
                        var outflowMmiInGrid = input.OutflowMoneyMarket;
                        var existingOutflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Outflow);

                        // delete from existing
                        var itemExistInGrid =
                            outflowMmiInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingOutflowMmi.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(removedItems);
                        }

                        foreach (var item in outflowMmiInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingOutflowMmi.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                    }
                                    if (foundItem.Issuer != item.Issuer)
                                    {
                                        foundItem.Issuer = item.Issuer;
                                    }
                                    if (foundItem.ProductType != item.ProductType)
                                    {
                                        foundItem.ProductType = item.ProductType;
                                    }
                                    if (foundItem.CounterParty != item.CounterParty)
                                    {
                                        foundItem.CounterParty = item.CounterParty;
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                    }
                                    if (foundItem.HoldingDayTenor != item.HoldingDayTenor)
                                    {
                                        foundItem.HoldingDayTenor = item.HoldingDayTenor;
                                    }
                                    if (foundItem.Nominal != item.Nominal)
                                    {
                                        foundItem.Nominal = item.Nominal;
                                    }
                                    if (foundItem.SellPurchaseRateYield != item.SellPurchaseRateYield)
                                    {
                                        foundItem.SellPurchaseRateYield = item.SellPurchaseRateYield;
                                    }
                                    if (foundItem.Price != item.Price)
                                    {
                                        foundItem.Price = item.Price;
                                    }
                                    if (foundItem.IntDividendReceivable != item.IntDividendReceivable)
                                    {
                                        foundItem.IntDividendReceivable = item.IntDividendReceivable;
                                    }
                                    if (foundItem.Proceeds != item.Proceeds)
                                    {
                                        foundItem.Proceeds = item.Proceeds;
                                    }
                                    if (foundItem.PurchaseProceeds != item.PurchaseProceeds)
                                    {
                                        foundItem.PurchaseProceeds = item.PurchaseProceeds;
                                    }
                                    if (foundItem.CertNoStockCode != item.CertNoStockCode)
                                    {
                                        foundItem.CertNoStockCode = item.CertNoStockCode;
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_MMI.Add(new FID_Treasury_MMI
                                {
                                    FormId = form.Id,
                                    CashflowType = Common.Cashflow.Outflow,
                                    Dealer = item.Dealer,
                                    Issuer = item.Issuer,
                                    ProductType = item.ProductType,
                                    CounterParty = item.CounterParty,
                                    ValueDate = item.ValueDate,
                                    MaturityDate = item.MaturityDate,
                                    HoldingDayTenor = item.HoldingDayTenor,
                                    Nominal = item.Nominal,
                                    SellPurchaseRateYield = item.SellPurchaseRateYield,
                                    Price = item.Price,
                                    IntDividendReceivable = item.IntDividendReceivable,
                                    Proceeds = item.Proceeds,
                                    PurchaseProceeds = item.PurchaseProceeds,
                                    CertNoStockCode = item.CertNoStockCode,

                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }
                    
                    db.SaveChanges();

                    if (form.FormStatus == Common.FormStatus.PendingApproval)
                    {
                        new WorkflowService().SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);
                        new MailService().SubmitForApproval(form.Id, form.FormType, form.ApprovedBy, input.ApprovalNotes);
                        new NotificationService().NotifyApprovalRequest(form.ApprovedBy, form.Id, form.PreparedBy, form.FormType);
                    }

                    return Request.CreateResponse(HttpStatusCode.Created, form.Id);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [Route("Treasury/Approval")]
        public HttpResponseMessage Treasury_FormApproval([FromBody] FormApprovalModel input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == input.FormId);

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


        [HttpDelete]
        [Route("Treasury")]
        public HttpResponseMessage Treasury_FormDelete(FormDataCollection request)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(request.Get("id"));

                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == key);

                    if (form != null)
                    {
                        db.FID_Treasury.Remove(form);

                        var depositInflowOutflow = db.FID_Treasury_Deposit.Where(x => x.FormId == key);
                        if (depositInflowOutflow.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(depositInflowOutflow);
                        }

                        var mmiInflowOutflow = db.FID_Treasury_MMI.Where(x => x.FormId == key);
                        if (mmiInflowOutflow.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(mmiInflowOutflow);
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

    }
}
