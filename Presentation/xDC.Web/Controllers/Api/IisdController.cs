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
    [RoutePrefix("api/iisd")]
    public class IisdController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetIisdForms(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var iisdFormTypes = new List<string>()
                    {
                        Common.FormTypeMapping(2)
                    };

                    var result = db.FormHeader
                        .Where(x => iisdFormTypes.Contains(x.FormType)).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }


        [HttpPost]
        [Route("NewTradeSettlementForm")]
        public HttpResponseMessage NewTradeSettlementForm([FromBody] TradeSettlementModel inputs)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var newFormHeader = new xDC.Infrastructure.Application.FormHeader()
                    {
                        FormType = Common.FormTypeMapping(2),
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = Common.FormStatusMapping(2),
                        Currency = inputs.currency,
                        ApprovedBy = inputs.Approver
                    };

                    Validate(newFormHeader);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.FormHeader.Add(newFormHeader);
                    db.SaveChanges();


                    var newTrades = new List<Iisd_TradeSettlement> ();

                    if (inputs.Equity != null)
                    {
                        foreach (var item in inputs.Equity)
                        {
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                            newTrades.Add(new Iisd_TradeSettlement()
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
                    
                    Validate(newTrades);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.Iisd_TradeSettlement.AddRange(newTrades);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, newFormHeader.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("GetTradeSettlement")]
        public HttpResponseMessage GetInflowFunds(string id, string tradeType,  DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<Iisd_TradeSettlement>();

                    if (!string.IsNullOrEmpty(id))
                    {
                        var formId = Convert.ToInt32(id);
                        var instrumentType = Common.TradeSettlementUrlParamMapping(tradeType);
                        result = db.Iisd_TradeSettlement
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
                var existingRecord = db.Iisd_TradeSettlement.SingleOrDefault(o => o.Id == key);

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

                var newRecord = new Amsd_InflowFunds();
                JsonConvert.PopulateObject(values, newRecord);

                newRecord.CreatedBy = User.Identity.Name;
                newRecord.CreatedDate = DateTime.Now;

                Validate(newRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.Amsd_InflowFunds.Add(newRecord);
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
                var foundRecord = db.Amsd_InflowFunds.First(x => x.Id == key);

                db.Amsd_InflowFunds.Remove(foundRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
