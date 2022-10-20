using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using DevExpress.XtraRichEdit.Fields;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.Extension.DocGenerator;
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
        #region TS Landing Page

        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_View)]
        [HttpGet]
        [Route("ts/home/grid1")]
        public HttpResponseMessage TsHomeGrid1(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var TsHomeGrid1Data = TsFormService.GetTsHomeGrid1(User.Identity.Name);

                if (TsHomeGrid1Data != null)
                {
                    return Request.CreateResponse(DataSourceLoader.Load(TsHomeGrid1Data, loadOptions));
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

        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_Edit)]
        [HttpPost]
        [Route("ts/home/retractForm")]
        public HttpResponseMessage TsHomeRetractForm(RetractFormVM req)
        {
            try
            {
                var retractFormStatus = TsFormService.RetractFormSubmission(req.FormId, User.Identity.Name);

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
                var TsHomeGrid2Data = TsFormService.GetTsHomeGrid2(User.Identity.Name);

                if (TsHomeGrid2Data != null)
                {
                    return Request.CreateResponse(DataSourceLoader.Load(TsHomeGrid2Data, loadOptions));
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

        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_View)]
        [HttpGet]
        [Route("ts/approvedTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TsConsolidatedPage(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var consolidatedPageData = TsFormService.GetConsolidatedPage(settlementDateEpoch, currency);

                if (consolidatedPageData != null)
                {
                    return Request.CreateResponse(DataSourceLoader.Load(consolidatedPageData, loadOptions));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error. Please check logs.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_View)]
        [HttpGet]
        [Route("ts/approvedTradeItems/{formId}/{instrumentType}")]
        public HttpResponseMessage TsApprovedItemsGrid(int formId, string instrumentType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeCategory = Common.TsCategoryUrlParamMapping(instrumentType);

                    if (tradeCategory != null)
                    {
                        var result = TsFormService.GetTsItemsGrid(formId, instrumentType);

                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Instrument Type not exist!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        #endregion

        #region TS View Page

        #endregion

        #region TS Generate File for download

        [Route("ts/generateConsolidated")]
        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_Download)]
        [HttpPost]
        public HttpResponseMessage GenerateConsolidated([FromBody] TsGenerateFileRequest req)
        {
            try
            {
                var settlementDateParsed = ConvertEpochToDateTime(req.settlementDate);

                var generatedDocumentFile =
                    new TradeSettlementFormDoc().GenerateFileConsolidated(settlementDateParsed.Value.Date,
                        req.currency.ToUpper(), req.isExportAsExcel);

                if (!string.IsNullOrEmpty(generatedDocumentFile))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, generatedDocumentFile);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }


        }

        [Route("ts/generatePart")]
        [KflowApiAuthorize(PermissionKey.ISSD_TradeSettlementForm_Download)]
        [HttpPost]
        public HttpResponseMessage GeneratePart([FromBody] TsGenerateFileRequest req)
        {
            try
            {
                var generatedDocumentFile = new TradeSettlementFormDoc().GenerateFile(req.formId, req.isExportAsExcel);

                if (!string.IsNullOrEmpty(generatedDocumentFile))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, generatedDocumentFile);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }


        }

        #endregion

        #region Trade Settlement Form

        [HttpPost]
        [Route("ts/New")]
        public HttpResponseMessage TS_NewForm([FromBody] TsCreateNewFormRequest req)
        {
            try
            {
                var createdFormId = TsFormService.CreateNewForm(req, User.Identity.Name);
                if (createdFormId > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, createdFormId);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in saving form data. Please refer system logs.");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [Route("ts/Edit")]
        public HttpResponseMessage TS_EditForm([FromBody] TsCreateNewFormRequest req)
        {
            try
            {
                var editedFormId = TsFormService.EditForm(req, User.Identity.Name);
                if (editedFormId > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, editedFormId);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in saving editted form data. Please refer system logs.");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [Route("TradeSettlement/Approval")]
        public HttpResponseMessage TS_Approval([FromBody] TsFormApprovalRequest req)
        {
            try
            {
                var formApprovalResult = TsFormService.FormApproval(req, User.Identity.Name);

                if (formApprovalResult > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Accepted, formApprovalResult);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed to accept form approval. Please refer system logs.");
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
                var key = Convert.ToInt32(input.Get("id"));
                var deleteFormResult = TsFormService.DeleteForm(key, User.Identity.Name);

                if (deleteFormResult)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed to delete the form. Please check system logs for details.");
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
                    var result = TsFormService.GetOpeningBalance(db, settlementDateOnly, currency);
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
