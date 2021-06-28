using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xDC_Web.Models;
using System.Data.Entity;
using xDC.Utils;
using Apps = xDC.Infrastructure.Application;

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
                using (var db = new Apps.kashflowDBEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var result = db.FormHeader.Where(x => x.FormType == "Amsd - Inflow Funds").Include(x => x.AmsdInflowFunds).ToList();

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
