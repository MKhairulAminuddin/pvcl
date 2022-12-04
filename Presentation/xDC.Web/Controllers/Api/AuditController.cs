
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using xDC.Domain.WebApi.Audit;
using xDC.Services.Audit;
using xDC.Services.Form;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using static xDC.Utils.Common;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(PermissionKey.AuditTrail)]
    [RoutePrefix("api/audit")]
    public class AuditController : ApiController
    {
        #region Fields
        private readonly IAuditService _auditService = Startup.Container.GetInstance<IAuditService>();

        #endregion

        [HttpGet]
        [Route("auditForm/{fromDateEpoch}/{toDateEpoch}/{formId}/{formType}/{userId}/{actionType}")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_FormAudit)]
        public HttpResponseMessage AuditForm(long fromDateEpoch, long toDateEpoch, string formId, string formType, string userId, string actionType, DataSourceLoadOptions loadOptions)
        {
            var fromDate = Common.ConvertEpochToDateTime(fromDateEpoch);
            var toDate = Common.ConvertEpochToDateTime(toDateEpoch);
            var formIdInt = 0;
            Int32.TryParse(formId, out formIdInt);
            formType = formType == "null" ? null : formType;
            userId = userId == "null" ? null : userId;
            actionType = actionType == "null" ? null : actionType;

            var result = _auditService.FA(fromDate.Value, toDate.Value)
                            .Where(x => 
                                (formIdInt == 0 || x.FormId == formIdInt) &&
                                (string.IsNullOrEmpty(formType) || x.FormType == formType) &&
                                (string.IsNullOrEmpty(userId) || x.ModifiedBy == userId) &&
                                (string.IsNullOrEmpty(actionType) || x.ActionType == actionType))
                            .ToList();

            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }

        #region User Access Audit

        [HttpGet]
        [Route("UserAccess")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_UserAccessAudit)]
        public HttpResponseMessage GetUserAccessLog(DataSourceLoadOptions loadOptions)
        {
            var past30Days = DateTime.Now.AddDays(-30);
            var result = _auditService.Get_UAA()
                .Where(x => DbFunctions.TruncateTime(x.RecordedDate) > past30Days)
                .OrderByDescending(x => x.RecordedDate).ToList();
            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }

        [HttpPost]
        [Route("UserAccess")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_UserAccessAudit)]
        public HttpResponseMessage GetUserAccessAudit([FromBody] AuditReq req, DataSourceLoadOptions loadOptions)
        {
            var fromDate = xDC.Utils.Common.ConvertEpochToDateTime(req.FromDateUnix);
            var toDate = xDC.Utils.Common.ConvertEpochToDateTime(req.ToDateUnix);

            var result = _auditService.Get_UAA();

            if (fromDate != null && toDate != null)
            {
                result.Where(x => DbFunctions.TruncateTime(x.RecordedDate) >= fromDate &&
                                    DbFunctions.TruncateTime(x.RecordedDate) <= toDate);
            }

            if (req.UserId != null)
            {
                result.Where(x => x.UserName == req.UserId);
            }

            result.OrderByDescending(x => x.RecordedDate);

            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }

        #endregion

        #region User Management

        [HttpGet]
        [Route("UserManagement")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_UserManagementAudit)]
        public HttpResponseMessage GetUserManagementAudit(DataSourceLoadOptions loadOptions)
        {
            var past30Days = DateTime.Now.AddDays(-30);
            var result = _auditService.Get_UMA().Where(x => DbFunctions.TruncateTime(x.RecordedDate) > past30Days);
            return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
        }

        [HttpPost]
        [Route("UserManagement")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_UserManagementAudit)]
        public HttpResponseMessage GetUserManagementAuditFiltered([FromBody] AuditReq req,DataSourceLoadOptions loadOptions)
        {
            var fromDate = Common.ConvertEpochToDateTime(req.FromDateUnix);
            var toDate = Common.ConvertEpochToDateTime(req.ToDateUnix);

            var result = _auditService.Get_UMA();

            if (fromDate != null && toDate != null)
            {
                result.Where(x => DbFunctions.TruncateTime(x.RecordedDate) >= fromDate &&
                                    DbFunctions.TruncateTime(x.RecordedDate) <= toDate);
            }

            if (!string.IsNullOrEmpty(req.UserId))
            {
                result.Where(x => x.UserAccount == req.UserId);
            }

            return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
        }

        #endregion

        #region Role Management


        [HttpGet]
        [Route("RoleManagement")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_RoleManagementAudit)]
        public HttpResponseMessage GetRoleManagementAudit(DataSourceLoadOptions loadOptions)
        {
            var past30Days = DateTime.Now.AddDays(-30);
            var result = _auditService.Get_RMA().Where(x => DbFunctions.TruncateTime(x.RecordedDate) > past30Days);

            return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
        }

        [HttpPost]
        [Route("RoleManagement")]
        [KflowApiAuthorize(PermissionKey.AuditTrail_RoleManagementAudit)]
        public HttpResponseMessage GetRoleManagementAuditFiltered([FromBody] AuditReq req, DataSourceLoadOptions loadOptions)
        {
            var fromDate = Common.ConvertEpochToDateTime(req.FromDateUnix);
            var toDate = Common.ConvertEpochToDateTime(req.ToDateUnix);

            var result = _auditService.Get_RMA();

            if (fromDate != null && toDate != null)
            {
                result.Where(x => DbFunctions.TruncateTime(x.RecordedDate) >= fromDate &&
                                    DbFunctions.TruncateTime(x.RecordedDate) <= toDate);
            }

            if (!string.IsNullOrEmpty(req.RoleName))
            {
                result.Where(x => x.Role == req.RoleName);
            }

            return Request.CreateResponse(DataSourceLoader.Load(result.ToList(), loadOptions));
        }


        #endregion
    }
}
