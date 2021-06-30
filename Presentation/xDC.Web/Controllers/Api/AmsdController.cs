using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xDC_Web.Models;
using System.Data.Entity;
using xDC.Utils;
using xDC.Infrastructure.Application;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles="Administrator, Power User, Amsd")]
    [RoutePrefix("api/amsd")]
    public class AmsdController : ApiController
    {
        [HttpGet]
        [Route("GetAmsdForms")]
        public HttpResponseMessage GetAmsdForms(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    /*db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;*/
                    var result = db.FormHeader
                        .Where(x => x.FormType == "Inflow Funds").ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            
        }


        [HttpPost]
        [Route("NewInflowFundsForm")]
        public HttpResponseMessage NewInflowFundsForm([FromBody] InflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {

                    var newRecord = new FormHeader()
                    {
                        FormType = inputs.FormType,
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = Common.FormStatusMapping(2),
                    };

                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.FormHeader.Add(newRecord);
                    db.SaveChanges();

                    var newRecordInflowFunds = new List<Amsd_InflowFunds>();

                    foreach (var item in inputs.AmsdInflowFunds)
                    {
                        newRecordInflowFunds.Add(new Amsd_InflowFunds()
                        {
                            FormId = newRecord.Id,
                            FundType = item.FundType,
                            Bank = item.Bank,
                            Amount = item.Amount,
                            CreatedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now
                        });
                    }
                    
                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                    
                    db.Amsd_InflowFunds.AddRange(newRecordInflowFunds);
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
