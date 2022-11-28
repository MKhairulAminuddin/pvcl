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
using System.Net.Http.Formatting;
using DevExpress.Utils;
using Newtonsoft.Json;
using xDC.Utils;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC_Web.ViewModels.Amsd;
using xDC_Web.ViewModels;
using static xDC.Utils.Common;
using xDC_Web.Extension.CustomAttribute;
using xDC.Services.Form;
using xDC.Domain.Form;
using xDC.Services.Audit;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Domain.WebApi.Forms.InflowFund;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(PermissionKey.AMSD)]
    [RoutePrefix("api/amsd")]
    public class AmsdController : ApiController
    {
        #region Fields

        private readonly IIfFormService _ifFormService;

        #endregion

        #region Ctor

        public AmsdController(IIfFormService ifFormService)
        {
            _ifFormService = ifFormService;
        }

        #endregion

        #region Inflow Fund Form

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_View)]
        [HttpGet]
        [Route("inflowfund/home")]
        public HttpResponseMessage InflowFund_LandingPageGrid(DataSourceLoadOptions loadOptions)
        {
            var IfHomeGrid1Data = _ifFormService.GetHomeGrid(User.Identity.Name);
            if (IfHomeGrid1Data == null) return Request.CreateResponse(HttpStatusCode.BadRequest, "Error. Please check logs.");

            return Request.CreateResponse(DataSourceLoader.Load(IfHomeGrid1Data, loadOptions));
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_View)]
        [HttpGet]
        [Route("InflowFund/Items/{formId}")]
        public HttpResponseMessage InflowFundFormGrid_Items(int formId, DataSourceLoadOptions loadOptions)
        {
            var result = _ifFormService.GetFormItems(formId).OrderBy(x => x.Id).ToList();
            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("inflowfund/Retract")]
        public HttpResponseMessage InflowFund_RetractFormSubmission(RetractFormVM req)
        {
            var retractFormStatus = _ifFormService.WithdrawForm(req.FormId, User.Identity.Name, FormType.AMSD_IF);
            if (!retractFormStatus) return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to rectract submitted form. Please check with system admin.");

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("InflowFund/New")]
        public HttpResponseMessage InflowFund_CreateNew([FromBody] IfFormPage input)
        {
            var createdFormId = _ifFormService.CreateForm(input, User.Identity.Name);
            if (createdFormId < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to create form");

            return Request.CreateResponse(HttpStatusCode.Created, createdFormId);
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("InflowFund/Edit/{formId}")]
        public HttpResponseMessage InflowFund_EditForm(int formId, [FromBody] IfFormPage input)
        {
            var edittedForm = _ifFormService.EditForm(formId, input, User.Identity.Name);
            if (edittedForm < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

            return Request.CreateResponse(HttpStatusCode.Accepted, edittedForm);
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpDelete]
        [Route("InflowFund/Delete")]
        public HttpResponseMessage InflowFund_DeleteForm(FormDataCollection input)
        {           
            var key = Convert.ToInt32(input.Get("id"));

            var form = _ifFormService.DeleteForm(key, User.Identity.Name);
            if (form < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [KflowApiAuthorize(PermissionKey.AMSD_InflowFundForm_Edit)]
        [HttpPost]
        [Route("InflowFund/Approval")]
        public HttpResponseMessage InflowFund_ApproveForm([FromBody] IfFormApprovingReq input)
        {
            var form = _ifFormService.ApproveForm(input, User.Identity.Name);
            if (form < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in approving the form! Contact system admin for more details");

            return Request.CreateResponse(HttpStatusCode.Accepted, form);
        }

        #endregion

    }

}
