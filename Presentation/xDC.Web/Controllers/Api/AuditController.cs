using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Office.Crypto;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Domain.WebApi.Audit;
using xDC.Infrastructure.Application;
using xDC.Services;
using xDC.Utils;
using xDC_Web.Models;

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

        #region User Access Log

        [HttpGet]
        [Route("UserAccess")]
        public HttpResponseMessage GetUserAccessLog(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Log_UserAccess.OrderByDescending(x => x.RecordedDate).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("UserAccess")]
        public HttpResponseMessage GetUserAccessAudit([FromBody] AuditReq req, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var fromDate = xDC.Utils.Common.ConvertEpochToDateTime(req.FromDateUnix);
                    var toDate = xDC.Utils.Common.ConvertEpochToDateTime(req.ToDateUnix);

                    var result = db.Log_UserAccess
                        .Where(x => 
                            DbFunctions.TruncateTime(x.RecordedDate) >= DbFunctions.TruncateTime(fromDate) && 
                            DbFunctions.TruncateTime(x.RecordedDate) <= DbFunctions.TruncateTime(toDate))
                        .OrderByDescending(x => x.RecordedDate)
                        .ToList();

                    if (req.UserId != null)
                    {
                        result.Where(x => x.UserName == req.UserId);
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #endregion

        #region User Management

        [HttpGet]
        [Route("UserManagement")]
        public HttpResponseMessage GetUserManagementAudit(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = AuditService.Get_UMA(out bool requestStatus);
                    if (requestStatus)
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("UserManagement")]
        public HttpResponseMessage GetUserManagementAuditFiltered([FromBody] AuditReq req,DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = AuditService.Get_UMA(out bool requestStatus, req);
                    if (requestStatus)
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #endregion

        #region Role Management


        [HttpGet]
        [Route("RoleManagement")]
        public HttpResponseMessage GetRoleManagementAudit(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = AuditService.Get_RMA(out bool requestStatus);
                    if (requestStatus)
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("RoleManagement")]
        public HttpResponseMessage GetRoleManagementAuditFiltered([FromBody] AuditReq req, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = AuditService.Get_RMA(out bool requestStatus, req);
                    if (requestStatus)
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }


        #endregion
    }
}
