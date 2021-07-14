using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using xDC.Infrastructure.Application;
using xDC.Utils;
using xDC_Web.Models;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, IISD")]
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
        public HttpResponseMessage InsertIisdTradeSettlement([FromBody] TradeSettlementModel inputs)
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
                        Currency = inputs.currency
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
    }
}
