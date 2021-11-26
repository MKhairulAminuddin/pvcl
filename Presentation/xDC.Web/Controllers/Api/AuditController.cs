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

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User")]
    [RoutePrefix("api/audit")]
    public class AuditController : ApiController
    {
        [HttpGet]
        [Route("auditForm/{fromDateEpoch}/{toDateEpoch}/{formId}/{formType}/{userId}/{actionType}")]
        public HttpResponseMessage AuditForm(long fromDateEpoch, long toDateEpoch, string formId, string formType, string userId, string actionType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                   var fromDate = Common.ConvertEpochToDateTime(fromDateEpoch);
                   var toDate = Common.ConvertEpochToDateTime(toDateEpoch);
                   var formIdInt = 0;
                   Int32.TryParse(formId, out formIdInt);
                    formType = formType == "null" ? null : formType;
                    userId = userId == "null" ? null : userId;
                    actionType = actionType == "null" ? null : actionType;

                    var result = db.Audit_Form
                         .Where(x => 
                            (DbFunctions.TruncateTime(x.ModifiedOn) >= DbFunctions.TruncateTime(fromDate) &&
                             DbFunctions.TruncateTime(x.ModifiedOn) <= DbFunctions.TruncateTime(toDate)) &&

                             (formIdInt == 0 || x.FormId == formIdInt) &&
                             (string.IsNullOrEmpty(formType) || x.FormType == formType) &&
                             (string.IsNullOrEmpty(userId) || x.ModifiedBy == userId) &&
                             (string.IsNullOrEmpty(actionType) || x.ActionType == actionType))
                         .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }
    }
}
