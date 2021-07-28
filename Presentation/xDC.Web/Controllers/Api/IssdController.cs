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
using xDC.Utils;
using xDC_Web.Models;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, IISD")]
    [RoutePrefix("api/issd")]
    public class IssdController : ApiController
    {
        [HttpGet]
        [Route("GetIssdForm")]
        public HttpResponseMessage GetIssdForm(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var iisdFormTypes = new List<string>()
                    {
                        Common.FormTypeMapping(2)
                    };

                    var result = db.Form_Header
                        .Where(x => iisdFormTypes.Contains(x.FormType)).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
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
        public HttpResponseMessage NewTradeSettlement([FromBody] TradeSettlementModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newFormHeader = new Form_Header()
                    {
                        FormType = Common.FormTypeMapping(2),
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = Common.FormStatusMapping(2),
                        FormDate = Common.ConvertEpochToDateTime(inputs.SettlementDateEpoch),
                        Currency = inputs.Currency,
                        ApprovedBy = inputs.Approver
                    };
                    
                    db.Form_Header.Add(newFormHeader);
                    db.SaveChanges();


                    var newTrades = new List<ISSD_TradeSettlement>();

                    if (inputs.Equity != null)
                    {
                        foreach (var item in inputs.Equity)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(1),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Bond != null)
                    {
                        foreach (var item in inputs.Bond)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(2),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Cp != null)
                    {
                        foreach (var item in inputs.Cp)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(3),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.NotesPaper != null)
                    {
                        foreach (var item in inputs.NotesPaper)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(4),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Repo != null)
                    {
                        foreach (var item in inputs.Repo)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(5),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                FirstLeg = item.FirstLeg,
                                SecondLeg = item.SecondLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Coupon != null)
                    {
                        foreach (var item in inputs.Coupon)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(6),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                AmountPlus = item.AmountPlus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Fees != null)
                    {
                        foreach (var item in inputs.Fees)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(7),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Mtm != null)
                    {
                        foreach (var item in inputs.Mtm)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(8),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.FxSettlement != null)
                    {
                        foreach (var item in inputs.FxSettlement)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(9),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.ContributionCredited != null)
                    {
                        foreach (var item in inputs.ContributionCredited)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(10),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Altid != null)
                    {
                        foreach (var item in inputs.Altid)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(11),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Others != null)
                    {
                        foreach (var item in inputs.Others)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TradeSettlementMapping(12),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    /*Validate(newTrades);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);*/

                    db.ISSD_TradeSettlement.AddRange(newTrades);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, newFormHeader.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }



        #endregion

        #region Bank Balance

        [HttpGet]
        [Route("GetOpeningBalance/{type}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetOpeningBalance(string type, long settlementDateEpoch, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var amount = 0.0;

                    if (settlementDate != null)
                    {
                        type = type.ToUpper();
                        settlementDate = settlementDate.Value.Date;

                        var result = db.EDW_BankBalance.AsNoTracking().Where(x =>
                                x.InstrumentType == type && x.SettlementDate == settlementDate &&
                                x.Currency == currency)
                            .GroupBy(x => x.Currency)
                            .Select(x => new
                            {
                                amount = x.Sum(y => y.Amount)
                            }).FirstOrDefault();

                        if (result!=null)
                        {
                            amount = (double) result.amount;
                        }
                        

                        return Request.CreateResponse(HttpStatusCode.OK, amount);
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
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        type = type.ToUpper();
                        settlementDate = settlementDate.Value.Date;

                        var result = db.EDW_TradeItem.AsNoTracking().Where(x =>
                                x.InstrumentType == type && x.SettlementDate == settlementDate &&
                                x.Currency == currency)
                            .ToList();

                        var finalResult = new List<ISSD_TradeSettlement>();
                        foreach (var item in result)
                        {
                            var tradeItem = new ISSD_TradeSettlement
                            {
                                InstrumentType = item.InstrumentType,
                                InstrumentCode = item.InstrumentName,
                                StockCode = item.StockCode,
                                Maturity = (decimal?)((item.Type == "M" && item.InstrumentType != Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                Sales = (decimal?)((item.Type == "S" && item.InstrumentType != Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                Purchase = (decimal?)((item.Type == "P" && item.InstrumentType != Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                SecondLeg = (decimal?)((item.InstrumentType == Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                AmountPlus = (decimal?)((item.InstrumentType == Common.TradeSettlementMapping(6)) ? item.Amount : 0),
                            };

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
        [Route("GetTradeSettlement")]
        public HttpResponseMessage GetTradeSettlement(string id, string tradeType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<ISSD_TradeSettlement>();

                    if (!string.IsNullOrEmpty(id))
                    {
                        var formId = Convert.ToInt32(id);
                        var instrumentType = Common.TradeSettlementUrlParamMapping(tradeType);
                        result = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == formId && x.InstrumentType == instrumentType).ToList();
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
        public HttpResponseMessage UpdateTradeSettlement(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");
                var existingRecord = db.ISSD_TradeSettlement.SingleOrDefault(o => o.Id == key);

                JsonConvert.PopulateObject(values, existingRecord);

                if (existingRecord != null)
                {
                    existingRecord.UpdatedBy = User.Identity.Name;
                    existingRecord.UpdatedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        [HttpPost]
        public HttpResponseMessage InsertTradeSettlement(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var values = form.Get("values");

                var newRecord = new AMSD_InflowFund();
                JsonConvert.PopulateObject(values, newRecord);

                newRecord.CreatedBy = User.Identity.Name;
                newRecord.CreatedDate = DateTime.Now;

                Validate(newRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.AMSD_InflowFund.Add(newRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, newRecord);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteTradeSettlement(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var foundRecord = db.AMSD_InflowFund.First(x => x.Id == key);

                db.AMSD_InflowFund.Remove(foundRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
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
    }
}
