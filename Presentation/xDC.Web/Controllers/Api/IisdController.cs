using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using xDC.Utils;
using xDC_Web.Models;

namespace xDC_Web.Controllers.Api
{
    [Authorize]
    public class IisdController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetIisdForms(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.FormHeader.Where(x => x.FormType == "IISD.TradeSettlement").ToList();

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

                    var newRecord = new xDC.Infrastructure.Application.FormHeader()
                    {
                        FormType = inputs.FormType
                    };

                    foreach (var item in inputs.Trades)
                    {
                        /*newRecord.AmsdInflowFunds.Add(new xDC.Infrastructure.Application.AmsdInflowFunds()
                        {
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount
                        });*/
                    }

                    newRecord.PreparedBy = User.Identity.Name;
                    newRecord.PreparedDate = DateTime.Now;
                    newRecord.FormStatus = Common.FormStatusMapping(2);


                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.FormHeader.Add(newRecord);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, newRecord.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }
    }
}
