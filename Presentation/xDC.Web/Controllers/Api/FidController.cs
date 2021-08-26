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
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Models;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.Treasury;

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
        [Route("TcaTaggingGrid/opBalance/{formId}")]
        public HttpResponseMessage TcaTaggingOpBalanceGrid(int formId, DataSourceLoadOptions loadOptions)
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
        [Route("TcaTaggingGrid/opBalance")]
        public HttpResponseMessage TcaTaggingOpBalanceGridUpdate(FormDataCollection form)
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
        public HttpResponseMessage SummaryReport(long reportDate, DataSourceLoadOptions loadOptions)
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
                            // create row for account and its opening balance. e.g. RENTAS - OB 20,000
                            var item = new TenAmCutOffItemVM();
                            item.Account = opBalance.Account;
                            if (opBalance.Amount != null) item.OpeningBalance = opBalance.Amount.Value;
                            item.Currency = form.Currency;

                            // get total inflow based on assigned inflow account
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

                            // get total inflow based on assigned outflow account
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

                            item.Net = item.OpeningBalance + item.TotalInflow - item.TotalOutflow;

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
                    var result = db.EDW_Maturity.Where(x => DbFunctions.TruncateTime(x.Value_Date) == tradeDate).Select(
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


        [HttpPost]
        [Route("Treasury/New")]
        public HttpResponseMessage Treasury_FormNew([FromBody] TreasuryFormModel input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDateConverted = Common.ConvertEpochToDateTime(input.TradeDate);
                    tradeDateConverted = tradeDateConverted.Value.Date;

                    var form = new FID_Treasury
                    {
                        FormType = "Treasury",
                        FormStatus = "Pending Approval",
                        Currency = input.Currency,
                        TradeDate = tradeDateConverted,
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now
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
                                Id = 0,
                                // form id mana
                                CashflowType = "INFLOW",
                                Dealer = item.Dealer,
                                Bank = item.Bank,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
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
                            inflowDeposit.Add(new FID_Treasury_Deposit
                            {
                                Id = 0,
                                // form id mana
                                CashflowType = "OUTFLOW",
                                Dealer = item.Dealer,
                                Bank = item.Bank,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
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
                    db.FID_Treasury_Deposit.AddRange(outflowDeposit);
                    db.SaveChanges();

                    var inflowMoneyMarket = new List<FID_Treasury_Item>();
                    if (input.InflowMoneyMarket.Any())
                    {
                        foreach (var item in input.InflowMoneyMarket)
                        {
                            inflowMoneyMarket.Add(new FID_Treasury_Item
                            {
                                Id = 0,
                                // form id mana
                                CashflowType = "INFLOW",
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
                                CertNoStockCode = item.CertNoStockCode,

                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_Item.AddRange(inflowMoneyMarket);
                    db.SaveChanges();

                    var outflowMoneyMarket = new List<FID_Treasury_Item>();
                    if (input.OutflowMoneyMarket.Any())
                    {
                        foreach (var item in input.OutflowMoneyMarket)
                        {
                            outflowMoneyMarket.Add(new FID_Treasury_Item
                            {
                                Id = 0,
                                // form id mana
                                CashflowType = "INFLOW",
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
                                CertNoStockCode = item.CertNoStockCode,

                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_Item.AddRange(outflowMoneyMarket);
                    db.SaveChanges();


                    return Request.CreateResponse(HttpStatusCode.Created, form.Id);
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
