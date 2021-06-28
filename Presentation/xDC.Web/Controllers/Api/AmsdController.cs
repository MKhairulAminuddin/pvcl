using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC_Web.Models;
using Apps = xDC.Infrastructure.Application;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles="Administrator, Power User, Amsd")]
    [RoutePrefix("api/amsd")]
    public class AmsdController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetAmsdForms(DataSourceLoadOptions loadOptions)
        {
            var result = new Apps.kashflowDBEntities().FormHeader.Where(x => x.FormType == "Amsd - Inflow Funds");

            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }


        [HttpPost]
        [Route("NewInflowFundsForm")]
        public HttpResponseMessage NewInflowFundsForm([FromBody] InflowFundsModel inputs)
        {
            try
            {
                using (var db = new Apps.kashflowDBEntities())
                {

                    var newRecord = new Apps.FormHeader()
                    {
                        FormType = inputs.FormType
                    };

                    foreach (var item in inputs.AmsdInflowFunds)
                    {
                        newRecord.AmsdInflowFunds.Add(new Apps.AmsdInflowFunds()
                        {
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount
                        });
                    }

                    newRecord.CreatedBy = User.Identity.Name;
                    newRecord.CreatedDate = DateTime.Now;
                    
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
