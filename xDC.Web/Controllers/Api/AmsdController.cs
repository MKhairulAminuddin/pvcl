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
using Apps = xDC.Infrastructure.Application;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles="Administrator, Power User, Amsd")]
    public class AmsdController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetAmsdForms(DataSourceLoadOptions loadOptions)
        {
            var result = new Apps.kashflowDBEntities().FormHeader.Where(x => x.FormType == "Amsd - Inflow Funds");

            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }


        [HttpPost]
        public HttpResponseMessage SubmitInflowFundsForm(FormDataCollection form)
        {
            try
            {
                using (var db = new Apps.kashflowDBEntities())
                {
                    var values = form.Get("values");

                    var newRecord = new Apps.FormHeader();
                    JsonConvert.PopulateObject(values, newRecord);

                    newRecord.CreatedBy = User.Identity.Name;
                    newRecord.CreatedDate = DateTime.Now;
                    
                    Validate(newRecord);

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                    db.FormHeader.Add(newRecord);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, newRecord);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            
        }

    }

}
