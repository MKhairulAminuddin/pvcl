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
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Models;
using xDC_Web.ViewModels;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.Fid.Treasury;
using static xDC.Utils.Common;
using TreasuryFormVM = xDC_Web.Models.TreasuryFormVM;


namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("api/fid")]
    public class FidController : ApiController
    {
        #region FCA Tagging

        [HttpGet]
        [Route("FcaTagging")]
        public HttpResponseMessage FcaTaggingSummaryList(DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetSummaryForFidView(out bool statusRequest);

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
            var response = new FcaTaggingFormService().DxGrid_GetDetailForFidView(settlementDateEpoch, currency, out bool statusRequest);

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
        public HttpResponseMessage TcaTaggingGrid(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
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


        #region Treasury Form

        [HttpGet]
        [Route("Treasury")]
        public HttpResponseMessage TreasuryHomeGrid1(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var TreasuryHomeGrid1Data = TreasuryFormService.GetTsHomeGrid1(User.Identity.Name);

                if (TreasuryHomeGrid1Data != null)
                {
                    return Request.CreateResponse(DataSourceLoader.Load(TreasuryHomeGrid1Data, loadOptions));
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
                        .Where(x => DbFunctions.TruncateTime(x.MaturityDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency)
                        .Select(
                        x => new TreasuryDepositVM
                        {
                            Dealer = x.Operator,
                            Bank = x.Bank,
                            TradeDate = DateTime.Now,
                            ValueDate = x.ValueDate,
                            MaturityDate = x.MaturityDate,
                            Principal = x.Principle,
                            Tenor = x.Tenor,
                            RatePercent = x.Rate,
                            IntProfitReceivable = 0,
                            PrincipalIntProfitReceivable = 0,
                            ManualCalc_P_Plus_I = false,
                            AssetType = x.AssetType,
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

                        item.Id = xDC.Utils.Common.GetRandomId(30, 99);
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
                        .Where(x => DbFunctions.TruncateTime(x.ValueDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency)
                        .Select(x => new TreasuryMmiVM
                        {
                            CashflowType = Common.Cashflow.Inflow,
                            Dealer = x.Operator,
                            Issuer = x.Issuer,
                            TradeDate = DateTime.Now,
                            ValueDate = x.ValueDate,
                            MaturityDate = x.MaturityDate,
                            HoldingDayTenor = x.Tenor,
                            CounterParty = x.Bank,
                            SellPurchaseRateYield = x.Rate,
                            Price = x.Price,
                            IntDividendReceivable = 0,
                            PurchaseProceeds = 0,
                            Proceeds = 0,
                            CertNoStockCode = x.StockCode,
                            ModifiedBy = null,
                            Nominal = x.Principle,
                            ProductType = x.AssetType
                        })
                        .ToList();

                    foreach (var item in result)
                    {
                        item.Id = xDC.Utils.Common.GetRandomId(30, 99);
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
                    var result = TreasuryFormService.List_CounterParty(db);

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
                    var result = TreasuryFormService.List_Issuer(db);

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
                        .Count(x => DbFunctions.TruncateTime(x.MaturityDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency);

                    var mm = db.EDW_Maturity_MM
                        .Count(x => DbFunctions.TruncateTime(x.ValueDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency);

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

        #region Grid

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


        #endregion

        [HttpPost]
        [Route("Treasury/New")]
        public HttpResponseMessage Treasury_FormNew([FromBody] TreasuryFormVM input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var valueDateConverted = Common.ConvertEpochToDateTime(input.ValueDate);
                    valueDateConverted = valueDateConverted.Value.Date;

                    var form = new FID_Treasury
                    {
                        FormType = Common.FormType.FID_TREASURY,
                        FormStatus = !string.IsNullOrEmpty(input.Approver) ? Common.FormStatus.PendingApproval : Common.FormStatus.Draft,
                        Currency = input.Currency,
                        ValueDate = valueDateConverted,
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
                                TradeDate = item.TradeDate,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                Principal = item.Principal,
                                Tenor = item.Tenor,
                                RatePercent = item.RatePercent,
                                IntProfitReceivable = item.IntProfitReceivable,
                                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I,
                                AssetType = item.AssetType,
                                RepoTag = item.RepoTag,
                                ContactPerson = item.ContactPerson,
                                Notes = item.Notes,
                                FcaAccount = item.FcaAccount,
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
                                TradeDate = item.TradeDate,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                Tenor = item.Tenor,
                                Principal = item.Principal,
                                RatePercent = item.RatePercent,
                                IntProfitReceivable = item.IntProfitReceivable,
                                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I,
                                AssetType = item.AssetType,
                                RepoTag = item.RepoTag,
                                ContactPerson = item.ContactPerson,
                                Notes = item.Notes,
                                FcaAccount = item.FcaAccount,
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
                                TradeDate = item.TradeDate,
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
                                FcaAccount = item.FcaAccount,
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
                                TradeDate = item.TradeDate,
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
                                FcaAccount = item.FcaAccount,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_MMI.AddRange(outflowMoneyMarket);
                    db.SaveChanges();

                    AuditService.Capture_FA(form.Id, form.FormType, FormActionType.Create, User.Identity.Name, $"Created an {form.FormType} form");


                    if (form.FormStatus == Common.FormStatus.PendingApproval)
                    {
                        WorkflowService.SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);
                        new MailService().TreasuryForm_SubmitApproval(form.Id, form.ApprovedBy, input.ApprovalNotes);
                        new NotificationService().NotifyApprovalRequest(form.ApprovedBy, form.Id, form.PreparedBy, form.FormType);
                        AuditService.Capture_FA(form.Id, form.FormType, FormActionType.RequestApproval, User.Identity.Name, $"Request Approval for {form.FormType} form");
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
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.AssetType,
                                                item.AssetType, "Asset Type");
                                    }
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.Dealer,
                                                item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Bank != item.Bank)
                                    {
                                        foundItem.Bank = item.Bank;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.Bank,
                                                item.Bank, "Bank");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                                item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                                item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.Tenor != item.Tenor)
                                    {
                                        foundItem.Tenor = item.Tenor;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.Tenor.ToString(),
                                                item.Tenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Principal != item.Principal)
                                    {
                                        foundItem.Principal = item.Principal;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.Principal.ToString(),
                                                item.Principal.ToString(), "Principal");
                                    }
                                    if (foundItem.RatePercent != item.RatePercent)
                                    {
                                        foundItem.RatePercent = item.RatePercent;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.RatePercent.ToString(),
                                                item.RatePercent.ToString(), "Rate");
                                    }
                                    if (foundItem.IntProfitReceivable != item.IntProfitReceivable)
                                    {
                                        foundItem.IntProfitReceivable = item.IntProfitReceivable;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.IntProfitReceivable.ToString(),
                                                item.IntProfitReceivable.ToString(), "Interest Profit Receivable");
                                    }
                                    if (foundItem.PrincipalIntProfitReceivable != item.PrincipalIntProfitReceivable)
                                    {
                                        foundItem.PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.PrincipalIntProfitReceivable.ToString(),
                                                item.PrincipalIntProfitReceivable.ToString(), "Principal + Interest Profit Receivable");
                                    }
                                    if (foundItem.ManualCalc_P_Plus_I != item.ManualCalc_P_Plus_I)
                                    {
                                        foundItem.ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.ManualCalc_P_Plus_I.ToString(),
                                                item.ManualCalc_P_Plus_I.ToString(), "Manual Calculation P + I");
                                    }
                                    if (foundItem.RepoTag != item.RepoTag)
                                    {
                                        foundItem.RepoTag = item.RepoTag;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.RepoTag,
                                                item.RepoTag, "Repo Tag");
                                    }
                                    if (foundItem.ContactPerson != item.ContactPerson)
                                    {
                                        foundItem.ContactPerson = item.ContactPerson;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.ContactPerson,
                                                item.ContactPerson, "Contact Person");
                                    }
                                    if (foundItem.Notes != item.Notes)
                                    {
                                        foundItem.Notes = item.Notes;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.Notes,
                                                item.Notes, "Notes");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.FcaAccount,
                                                item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_Deposit.Add(FID_Treasury_Deposit_ObjMap(form.Id, Common.Cashflow.Inflow, item));
                                AuditService.FA_AddRow(form.Id, form.FormType,
                                                form.ValueDate , User.Identity.Name,
                                                $"{item.Bank}, {item.CashflowType}, {item.Dealer}, {item.AssetType}...");
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
                            foreach (var item in existingInflowDeposit)
                            {
                                AuditService.FA_RemoveRow(form.Id, form.FormType, form.ValueDate, User.Identity.Name,
                                        $"{item.CashflowType}, {item.Bank}, {item.Dealer}, {item.AssetType}...");
                            }

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
                            foreach (var item in removedItems)
                            {
                                AuditService.FA_RemoveRow(form.Id, form.FormType, form.ValueDate, User.Identity.Name,
                                        $"{item.CashflowType}, {item.Bank}, {item.Dealer}, {item.AssetType}...");
                            }

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
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.AssetType,
                                            item.AssetType, "Asset Type");
                                    }
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Dealer,
                                            item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Bank != item.Bank)
                                    {
                                        foundItem.Bank = item.Bank;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Bank,
                                            item.Bank, "Bank");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                            item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                            item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.Tenor != item.Tenor)
                                    {
                                        foundItem.Tenor = item.Tenor;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Tenor.ToString(),
                                            item.Tenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Principal != item.Principal)
                                    {
                                        foundItem.Principal = item.Principal;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Principal.ToString(),
                                            item.Principal.ToString(), "Principal");
                                    }
                                    if (foundItem.RatePercent != item.RatePercent)
                                    {
                                        foundItem.RatePercent = item.RatePercent;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.RatePercent.ToString(),
                                            item.RatePercent.ToString(), "Rate");
                                    }
                                    if (foundItem.IntProfitReceivable != item.IntProfitReceivable)
                                    {
                                        foundItem.IntProfitReceivable = item.IntProfitReceivable;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.IntProfitReceivable.ToString(),
                                            item.IntProfitReceivable.ToString(), "Interest Profit Receivable");
                                    }
                                    if (foundItem.PrincipalIntProfitReceivable != item.PrincipalIntProfitReceivable)
                                    {
                                        foundItem.PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.PrincipalIntProfitReceivable.ToString(),
                                            item.PrincipalIntProfitReceivable.ToString(), "Principal + Interest Profit Receivable");
                                    }
                                    if (foundItem.ManualCalc_P_Plus_I != item.ManualCalc_P_Plus_I)
                                    {
                                        foundItem.ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, User.Identity.Name, foundItem.ManualCalc_P_Plus_I.ToString(),
                                                item.ManualCalc_P_Plus_I.ToString(), "Manual Calculation P + I");
                                    }
                                    if (foundItem.RepoTag != item.RepoTag)
                                    {
                                        foundItem.RepoTag = item.RepoTag;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.RepoTag,
                                            item.RepoTag, "Repo Tag");
                                    }
                                    if (foundItem.ContactPerson != item.ContactPerson)
                                    {
                                        foundItem.ContactPerson = item.ContactPerson;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.ContactPerson,
                                            item.ContactPerson, "Contact Person");
                                    }
                                    if (foundItem.Notes != item.Notes)
                                    {
                                        foundItem.Notes = item.Notes;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Notes,
                                            item.Notes, "Notes");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.FcaAccount,
                                            item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_Deposit.Add(FID_Treasury_Deposit_ObjMap(form.Id, Common.Cashflow.Outflow, item));
                                AuditService.FA_AddRow(form.Id, form.FormType,
                                    form.ValueDate, User.Identity.Name,
                                    $"{item.Bank}, {item.CashflowType}, {item.Dealer}, {item.AssetType}...");
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
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Dealer,
                                            item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Issuer != item.Issuer)
                                    {
                                        foundItem.Issuer = item.Issuer;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Issuer,
                                            item.Issuer, "Issuer");
                                    }
                                    if (foundItem.ProductType != item.ProductType)
                                    {
                                        foundItem.ProductType = item.ProductType;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.ProductType,
                                            item.ProductType, "Product Type");
                                    }
                                    if (foundItem.CounterParty != item.CounterParty)
                                    {
                                        foundItem.CounterParty = item.CounterParty;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.CounterParty,
                                            item.CounterParty, "Counter Party");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                            item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                            item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.HoldingDayTenor != item.HoldingDayTenor)
                                    {
                                        foundItem.HoldingDayTenor = item.HoldingDayTenor;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.HoldingDayTenor.ToString(),
                                            item.HoldingDayTenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Nominal != item.Nominal)
                                    {
                                        foundItem.Nominal = item.Nominal;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Nominal.ToString(),
                                            item.Nominal.ToString(), "Nominal");
                                    }
                                    if (foundItem.SellPurchaseRateYield != item.SellPurchaseRateYield)
                                    {
                                        foundItem.SellPurchaseRateYield = item.SellPurchaseRateYield;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.SellPurchaseRateYield.ToString(),
                                            item.SellPurchaseRateYield.ToString(), "Sell Purchase Rate Yield");
                                    }
                                    if (foundItem.Price != item.Price)
                                    {
                                        foundItem.Price = item.Price;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Price.ToString(),
                                            item.Price.ToString(), "Price");
                                    }
                                    if (foundItem.IntDividendReceivable != item.IntDividendReceivable)
                                    {
                                        foundItem.IntDividendReceivable = item.IntDividendReceivable;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.IntDividendReceivable.ToString(),
                                            item.IntDividendReceivable.ToString(), "Interest Dividend Receivable");
                                    }
                                    if (foundItem.Proceeds != item.Proceeds)
                                    {
                                        foundItem.Proceeds = item.Proceeds;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Proceeds.ToString(),
                                            item.Proceeds.ToString(), "Proceeds");
                                    }
                                    if (foundItem.PurchaseProceeds != item.PurchaseProceeds)
                                    {
                                        foundItem.PurchaseProceeds = item.PurchaseProceeds;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.PurchaseProceeds.ToString(),
                                            item.PurchaseProceeds.ToString(), "Purchase Proceeds");
                                    }
                                    if (foundItem.CertNoStockCode != item.CertNoStockCode)
                                    {
                                        foundItem.CertNoStockCode = item.CertNoStockCode;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.CertNoStockCode,
                                            item.CertNoStockCode, "Cert No Stock Code");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.FcaAccount,
                                            item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_MMI.Add(FID_Treasury_MMI_ObjMap(form.Id, Common.Cashflow.Inflow, item));
                                AuditService.FA_AddRow(form.Id, form.FormType,
                                    form.ValueDate, User.Identity.Name,
                                    $"{item.Issuer}, {item.CashflowType}, {item.Dealer}, {item.CashflowType}...");
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
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Dealer,
                                            item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Issuer != item.Issuer)
                                    {
                                        foundItem.Issuer = item.Issuer;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Issuer,
                                            item.Issuer, "Issuer");
                                    }
                                    if (foundItem.ProductType != item.ProductType)
                                    {
                                        foundItem.ProductType = item.ProductType;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.ProductType,
                                            item.ProductType, "Product Type");
                                    }
                                    if (foundItem.CounterParty != item.CounterParty)
                                    {
                                        foundItem.CounterParty = item.CounterParty;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.CounterParty,
                                            item.CounterParty, "Counter Party");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                            item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                            item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.HoldingDayTenor != item.HoldingDayTenor)
                                    {
                                        foundItem.HoldingDayTenor = item.HoldingDayTenor;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.HoldingDayTenor.ToString(),
                                            item.HoldingDayTenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Nominal != item.Nominal)
                                    {
                                        foundItem.Nominal = item.Nominal;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Nominal.ToString(),
                                            item.Nominal.ToString(), "Nominal");
                                    }
                                    if (foundItem.SellPurchaseRateYield != item.SellPurchaseRateYield)
                                    {
                                        foundItem.SellPurchaseRateYield = item.SellPurchaseRateYield;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.SellPurchaseRateYield.ToString(),
                                            item.SellPurchaseRateYield.ToString(), "Sell Purchase Rate Yield");
                                    }
                                    if (foundItem.Price != item.Price)
                                    {
                                        foundItem.Price = item.Price;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Price.ToString(),
                                            item.Price.ToString(), "Price");
                                    }
                                    if (foundItem.IntDividendReceivable != item.IntDividendReceivable)
                                    {
                                        foundItem.IntDividendReceivable = item.IntDividendReceivable;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.IntDividendReceivable.ToString(),
                                            item.IntDividendReceivable.ToString(), "Interest Dividend Receivable");
                                    }
                                    if (foundItem.Proceeds != item.Proceeds)
                                    {
                                        foundItem.Proceeds = item.Proceeds;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.Proceeds.ToString(),
                                            item.Proceeds.ToString(), "Proceeds");
                                    }
                                    if (foundItem.PurchaseProceeds != item.PurchaseProceeds)
                                    {
                                        foundItem.PurchaseProceeds = item.PurchaseProceeds;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.PurchaseProceeds.ToString(),
                                            item.PurchaseProceeds.ToString(), "Purchase Proceeds");
                                    }
                                    if (foundItem.CertNoStockCode != item.CertNoStockCode)
                                    {
                                        foundItem.CertNoStockCode = item.CertNoStockCode;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.CertNoStockCode,
                                            item.CertNoStockCode, "Cert No Stock Code");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        AuditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, User.Identity.Name, foundItem.FcaAccount,
                                            item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_MMI.Add(FID_Treasury_MMI_ObjMap(form.Id, Common.Cashflow.Outflow, item));
                                AuditService.FA_AddRow(form.Id, form.FormType,
                                    form.ValueDate, User.Identity.Name,
                                    $"{item.Issuer}, {item.CashflowType}, {item.Dealer}, {item.CashflowType}...");
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
                        WorkflowService.SubmitForApprovalWorkflow(form.Id, form.FormType, input.ApprovalNotes);
                        new MailService().TreasuryForm_SubmitApproval(form.Id, form.ApprovedBy, input.ApprovalNotes);
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
        public HttpResponseMessage Treasury_FormApproval([FromBody] InflowFundFormApprovalRequest input)
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
                            new MailService().TreasuryForm_Approval(form.Id, form.PreparedBy, input.ApprovalNote);
                            WorkflowService.ApprovalResponse(form.Id, form.FormStatus, input.ApprovalNote, form.FormType, form.PreparedBy, form.ApprovedBy);
                            AuditService.FA_Approval(form.Id, form.FormType, form.FormStatus, form.ValueDate, User.Identity.Name);

                            if (form.FormStatus == Common.FormStatus.Approved)
                            {
                                new MailService().ApprovedTreasuryToIssd();
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

                        AuditService.Capture_FA(form.Id, form.FormType, FormActionType.Delete, User.Identity.Name, $"Deleted {form.FormType} form");

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

        [KflowApiAuthorize(PermissionKey.FID_TreasuryForm_Edit)]
        [HttpPost]
        [Route("Treasury/retractForm")]
        public HttpResponseMessage Treasury_RetractForm(RetractFormVM req)
        {
            try
            {
                var retractFormStatus = FormService.RetractFormSubmission(req.FormId, User.Identity.Name, Common.FormType.FID_TREASURY);

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

        #endregion

        #region Private Functions

        private FID_Treasury_Deposit FID_Treasury_Deposit_ObjMap(int formId, string cashFlow, TreasuryDepositVM item)
        {
            return new FID_Treasury_Deposit
            {
                FormId = formId,
                CashflowType = cashFlow,
                Dealer = item.Dealer != null ? item.Dealer : User.Identity.Name,
                Bank = item.Bank,
                TradeDate = item.TradeDate,
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
                FcaAccount = item.FcaAccount,
                ModifiedBy = User.Identity.Name,
                ModifiedDate = DateTime.Now
            };
        }

        private FID_Treasury_MMI FID_Treasury_MMI_ObjMap(int formId, string cashFlow, TreasuryMmiVM item)
        {
            return new FID_Treasury_MMI()
            {
                FormId = formId,
                CashflowType = cashFlow,
                Dealer = item.Dealer != null ? item.Dealer:User.Identity.Name,
                Issuer = item.Issuer,
                ProductType = item.ProductType,
                CounterParty = item.CounterParty,
                TradeDate = item.TradeDate,
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
                FcaAccount = item.FcaAccount,
                ModifiedBy = User.Identity.Name,
                ModifiedDate = DateTime.Now
            };
        }

        #endregion

    }
}
