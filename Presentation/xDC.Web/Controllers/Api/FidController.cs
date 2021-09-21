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
                    var approvedTsForms = db.ISSD_FormHeader.Where(x => x.FormStatus == Common.FormStatus.Approved).Select(x => x.Id).ToList();

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader, 
                            a => a.FormId, 
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                            )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved && x.ts_form.SettlementDate != null && x.ts_form.Currency != "MYR")
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate, x.ts_form.Currency
                        })
                        .Select(x => new Ts10AmHomeGridVM
                        {
                            Currency = x.Key.Currency,
                            SettlementDate = x.Key.SettlementDate.Value,

                            CountPendingEquity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity 
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingCp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingNotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers
                                                                   && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingRepo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                           && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),

                            CountPendingCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingContribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn
                                                                    && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0))
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
        [Route("TcaTagging/AvailableTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TcaTagging_AvailableTrades(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
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

                        CountPendingEquity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                        CountPendingBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond
                                && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                        CountPendingCp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp
                                                      && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingNotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers
                                                                   && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingRepo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                           && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),

                            //xde outflow for this table
                            CountPendingCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingContribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn
                                                                    && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0))

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
        [Route("TcaTaggingGrid/TradeItem/{tradeType}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TcaTaggingGrid(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
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
        [Route("TcaTaggingGrid/TradeItem")]
        public HttpResponseMessage TcaTaggingGridUpdate(FormDataCollection form)
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
        [Route("TcaTagging/FcaAccount")]
        public HttpResponseMessage TcaTagging_FcaAccount(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = FidService.List_FcaBankAccount(db);
                    
                    result.Add("RENTAS");
                    result.Add("MMA");

                    return Request.CreateResponse(DataSourceLoader.Load(result.Select(x => new { Name = x}).Distinct().OrderBy(x => x.Name), loadOptions));
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
                    var resultRaw = new List<TenAmCutOffItemVM>();
                    
                    var configAccount = db.Config_FcaBankAccount.Where(x => x.Currency != "MYR").ToList();
                    configAccount.Add(new Config_FcaBankAccount
                    {
                        AccountName1 = "RENTAS",
                        AccountName2 = "RENTAS",
                        AccountName3 = "RENTAS",
                        Currency = "MYR"
                    });
                    configAccount.Add(new Config_FcaBankAccount
                    {
                        AccountName1 = "MMA",
                        AccountName2 = "MMA",
                        AccountName3 = "MMA",
                        Currency = "MYR"
                    });
                    

                    foreach (var account in configAccount)
                    {
                        // create row for account and its opening balance. e.g. RENTAS - OB 20,000
                        var item = new TenAmCutOffItemVM
                        {
                            Account = account.AccountName2,
                            Currency = account.Currency
                        };
                        result.Add(item);

                        var approvedTsIds = db.ISSD_FormHeader
                            .Where(x => x.FormStatus == Common.FormStatus.Approved
                                        && x.SettlementDate != null
                                        && x.Currency == item.Currency
                                        && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(reportDateParsed))
                            .Select(x => x.Id)
                            .ToList();

                        if (approvedTsIds.Any())
                        {
                            var tradeItemInflow = db.ISSD_TradeSettlement
                                .Where(x => approvedTsIds.Contains(x.FormId)
                                            && x.InflowTo != null
                                            && x.InflowTo == account.AccountName3
                                            && x.InflowAmount > 0)
                                .Select(l => l.InflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();
                            
                            item.TotalInflow = tradeItemInflow;
                            
                            // get total inflow based on assigned outflow account
                            var tradeItemOutflow = db.ISSD_TradeSettlement
                                .Where(x => approvedTsIds.Contains(x.FormId)
                                            && x.OutflowFrom != null
                                            && x.OutflowFrom == account.AccountName3
                                            && x.OutflowAmount > 0)
                                .Select(l => l.OutflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();
                            
                            item.TotalOutflow = tradeItemOutflow;
                        }
                        
                        resultRaw.Add(item);
                    }
                    
                    // AMSD - Inflow Funds
                    var approvedAmsdForms = db.AMSD_IF
                        .Where(x => DbFunctions.TruncateTime(x.ApprovedDate) == DbFunctions.TruncateTime(reportDateParsed) 
                                    && x.FormStatus == Common.FormStatus.Approved)
                        .Select(x => x.Id);

                    if (approvedAmsdForms.Any())
                    {
                        var inflowFunds = db.AMSD_IF_Item
                            .Where(x => approvedAmsdForms.Contains(x.FormId))
                            .GroupBy(x => new { x.Bank })
                            .Select(x => new
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
                                OpeningBalance = fund.Amount,
                                Net = fund.Amount
                            };
                            resultRaw.Add(inflowFundsFromAmsd);
                        }
                    }

                    result = resultRaw
                        .GroupBy(x => new
                        {
                            x.Account, x.Currency
                        })
                        .Select(x => new TenAmCutOffItemVM
                        {
                            Currency = x.Key.Currency,
                            Account = x.Key.Account,
                            OpeningBalance = 0,
                            TotalInflow = x.Sum(y => y.TotalInflow),
                            TotalOutflow = x.Sum(y => y.TotalOutflow)
                        })
                        .ToList();

                    foreach (var item in result)
                    {
                        // plug in opening balance
                        var ob = FcaTaggingSvc.GetOpeningBalance(db, reportDateParsed.Value, item.Currency, item.Account);

                        item.OpeningBalance = ob;
                    }

                    result = result.GroupBy(x => new
                        {
                            x.Account,
                            x.Currency
                        })
                        .Select(x => new TenAmCutOffItemVM
                        {
                            Currency = x.Key.Currency,
                            Account = x.Key.Account,
                            OpeningBalance = x.Sum(y => y.OpeningBalance),
                            TotalInflow = x.Sum(y => y.TotalInflow),
                            TotalOutflow = x.Sum(y => y.TotalOutflow),
                            Net = x.Sum(y => y.OpeningBalance) + x.Sum(y => y.TotalInflow) - x.Sum(y => y.TotalOutflow)
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

                    var result = db.EDW_Maturity_Deposit
                        .Where(x => DbFunctions.TruncateTime(x.Maturity_Date) == DbFunctions.TruncateTime(tradeDate)
                                    && x.CURRENCY == currency)
                        .Select(
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
        [Route("Treasury/EdwMmi/{tradeDateEpoch}/{Currency}")]
        public HttpResponseMessage Treasury_EdwMmi(long tradeDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);

                    var result = db.EDW_Maturity_MM
                        .Where(x => DbFunctions.TruncateTime(x.Value_Date) == DbFunctions.TruncateTime(tradeDate)
                                    && x.CURRENCY == currency)
                        .Select(x => new TreasuryMmiVM
                        {
                            CashflowType = Common.Cashflow.Inflow,
                            Dealer = x.Operator,
                            Issuer = null,
                            ValueDate = x.Value_Date.Value,
                            MaturityDate = x.Maturity_Date,
                            HoldingDayTenor = (int)x.Tenor,
                            CounterParty = x.Bank,
                            SellPurchaseRateYield = x.Rate.Value,
                            Price = 0,
                            IntDividendReceivable = 0,
                            PurchaseProceeds = 0,
                            Proceeds = 0,
                            CertNoStockCode = x.StockCode,
                            ModifiedBy = null,
                            ModifiedDate = default,
                            Nominal = x.Principle.Value,
                            ProductType = x.Asset_Type,

                        })
                        .ToList();

                    /*foreach (var item in result)
                    {
                        var rate = (double)item.SellPurchaseRateYield / 100;
                        var tenor = (double)item.HoldingDayTenor / 365;

                        item.IntDividendReceivable = item.Principal * tenor * rate;
                        item. = item.Principal + item.IntProfitReceivable;
                    }*/

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
        [Route("Treasury/EdwDataAvailability/{tradeDateEpoch}/{currency}")]
        public HttpResponseMessage Treasury_EdwDataAvailability(long tradeDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);

                    var result = new List<TreasuryEdwDataAvailability>();

                    var deposit = db.EDW_Maturity_Deposit
                        .Count(x => DbFunctions.TruncateTime(x.Maturity_Date) == DbFunctions.TruncateTime(tradeDate)
                                    && x.CURRENCY == currency);

                    var mm = db.EDW_Maturity_MM
                        .Count(x => DbFunctions.TruncateTime(x.Value_Date) == DbFunctions.TruncateTime(tradeDate)
                                    && x.CURRENCY == currency);

                    if (deposit > 0)
                    {
                        result.Add(new TreasuryEdwDataAvailability
                        {
                            Name = "Deposit Maturity " + currency ,
                            Numbers = deposit,
                            CategoryType = 1
                        });
                    }

                    if (mm > 0)
                    {
                        result.Add(new TreasuryEdwDataAvailability
                        {
                            Name = "Money Market " + currency,
                            Numbers = mm,
                            CategoryType = 2
                        });
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

                    form.PreparedBy = User.Identity.Name;
                    form.PreparedDate = DateTime.Now;

                    if (input.Approver != null)
                    {
                        form.ApprovedBy = input.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = Common.FormStatus.PendingApproval;
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
                    else
                    {
                        var existingInflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Inflow);

                        // delete from existing
                        if (existingInflowDeposit.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(existingInflowDeposit);
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
                    else
                    {
                        var existingOutflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Outflow);

                        // delete from existing
                        if (existingOutflowDeposit.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(existingOutflowDeposit);
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
                    else
                    {
                        var existingInflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Inflow);

                        // delete from existing
                        if (existingInflowMmi.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(existingInflowMmi);
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
                    else
                    {
                        var existingInflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Common.Cashflow.Outflow);

                        // delete from existing
                        if (existingInflowMmi.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(existingInflowMmi);
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
