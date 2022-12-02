using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Results;
using xDC.Domain.Web.FID.TreasuryForm;
using xDC.Domain.WebApi.Forms;
using xDC.Domain.WebApi.Forms.Treasury;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.Audit;
using xDC.Services.Form;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels;
using xDC_Web.ViewModels.Fid.Treasury;
using static xDC.Utils.Common;


namespace xDC_Web.Controllers.Api
{
    [RoutePrefix("api/fid")]
    [KflowAuthorize(Common.PermissionKey.FID)]
    public class FidController : ApiController
    {
        #region Fields

        private readonly ITreasuryFormService _tFormService;

        #endregion

        #region Ctor

        public FidController(ITreasuryFormService treasuryFormService)
        {
            _tFormService = treasuryFormService;
        }

        #endregion

        #region FCA Tagging

        [HttpGet]
        [Route("FcaTagging")]
        public HttpResponseMessage FcaTaggingSummaryList(DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetSummaryForFidView(out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpGet]
        [Route("FcaTagging/AvailableTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage FcaTagging_AvailableTrades(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetDetailForFidView(settlementDateEpoch, currency, out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpGet]
        [Route("FcaTaggingGrid/TradeItem/{tradeType}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TcaTaggingGrid(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().DxGrid_GetIndividualGridData(tradeType, settlementDateEpoch, currency, out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }

        [HttpPut]
        [Route("FcaTaggingGrid/TradeItem")]
        public HttpResponseMessage FcaTaggingGridUpdate(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var id = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");

                var existingRecord = db.ISSD_TradeSettlement.FirstOrDefault(o => o.Id == id);

                JsonConvert.PopulateObject(values, existingRecord);

                if (existingRecord != null)
                {
                    existingRecord.AssignedBy = User.Identity.Name;
                    existingRecord.AssignedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        [HttpGet]
        [Route("FcaTagging/FcaAccount")]
        public HttpResponseMessage FcaTagging_FcaAccount(DataSourceLoadOptions loadOptions)
        {
            var response = new FcaTaggingFormService().List_GetFcaBankAccount(out bool statusRequest);

            if (statusRequest)
            {
                return Request.CreateResponse(DataSourceLoader.Load(response, loadOptions));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");
            }
        }
        
        #endregion


        #region Treasury Form

        [HttpGet]
        [Route("Treasury")]
        [KflowAuthorize(PermissionKey.FID_TreasuryForm_View)]
        public HttpResponseMessage TreasuryHomeGrid1(DataSourceLoadOptions loadOptions)
        {
            var TreasuryHomeGrid1Data = _tFormService.GetTsHomeGrid1(User.Identity.Name);
            return Request.CreateResponse(DataSourceLoader.Load(TreasuryHomeGrid1Data, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/EdwMaturity/{tradeDateEpoch}/{Currency}")]
        public HttpResponseMessage Treasury_EdwMaturity(long tradeDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);
            if (tradeDate == null) return null;

            var getData = _tFormService.DepositFromEdw(tradeDate.Value, currency);
            if (getData == null) return null;

            return Request.CreateResponse(DataSourceLoader.Load(getData, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/EdwMmi/{tradeDateEpoch}/{Currency}")]
        public HttpResponseMessage Treasury_EdwMmi(long tradeDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);
            if (tradeDate == null) return null;

            var getData = _tFormService.MmiFromEdw(tradeDate.Value, currency);
            if (getData == null) return null;

            return Request.CreateResponse(DataSourceLoader.Load(getData, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/EdwBankCounterParty")]
        public HttpResponseMessage Treasury_EdwBankCounterParty(DataSourceLoadOptions loadOptions)
        {
            var result = _tFormService.List_CounterParty();
            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/EdwIssuer")]
        public HttpResponseMessage Treasury_EdwIssuer(DataSourceLoadOptions loadOptions)
        {
            var result = _tFormService.List_Issuer();
            return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
        }
        
        [HttpGet]
        [Route("Treasury/EdwDataAvailability/{tradeDateEpoch}/{currency}")]
        public HttpResponseMessage Treasury_EdwDataAvailability(long tradeDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeDate = Common.ConvertEpochToDateTime(tradeDateEpoch);

                    var result = new List<TreasuryEdwDataAvailability>();

                    var deposit = db.EDW_Maturity_Deposit
                        .Count(x => DbFunctions.TruncateTime(x.MaturityDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency);

                    var mm = db.EDW_Maturity_MM
                        .Count(x => DbFunctions.TruncateTime(x.ValueDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency);

                    if (deposit > 0)
                    {
                        result.Add(new TreasuryEdwDataAvailability
                        {
                            Name = "Deposit Maturity " + currency ,
                            Numbers = deposit,
                            CategoryType = 1
                        });
                    }

                    if (mm > 0)
                    {
                        result.Add(new TreasuryEdwDataAvailability
                        {
                            Name = "Money Market " + currency,
                            Numbers = mm,
                            CategoryType = 2
                        });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #region Grid

        [HttpGet]
        [Route("Treasury/inflow/deposit/{formId}")]
        public HttpResponseMessage Treasury_Inflow_Deposit(int formId, DataSourceLoadOptions loadOptions)
        {
            var getDepositData = _tFormService.GetDepositGrid(formId, Cashflow.INFLOW);
            return Request.CreateResponse(DataSourceLoader.Load(getDepositData, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/inflow/mmi/{formId}")]
        public HttpResponseMessage Treasury_Inflow_Mmi(int formId, DataSourceLoadOptions loadOptions)
        {
            var getMmiData = _tFormService.GetMmiGrid(formId, Cashflow.INFLOW);
            return Request.CreateResponse(DataSourceLoader.Load(getMmiData, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/outflow/deposit/{formId}")]
        public HttpResponseMessage Treasury_Outflow_Deposit(int formId, DataSourceLoadOptions loadOptions)
        {
            var getDepositData = _tFormService.GetDepositGrid(formId, Cashflow.OUTFLOW);
            return Request.CreateResponse(DataSourceLoader.Load(getDepositData, loadOptions));
        }

        [HttpGet]
        [Route("Treasury/outflow/mmi/{formId}")]
        public HttpResponseMessage Treasury_Outflow_Mmi(int formId, DataSourceLoadOptions loadOptions)
        {
            var getMmiData = _tFormService.GetMmiGrid(formId, Cashflow.OUTFLOW);
            return Request.CreateResponse(DataSourceLoader.Load(getMmiData, loadOptions));
        }


        #endregion

        #region Form Action

        [HttpPost]
        [Route("Treasury/New")]
        [KflowApiAuthorize(PermissionKey.FID_TreasuryForm_Edit)]
        public HttpResponseMessage Treasury_SubmitNewForm([FromBody] TForm input)
        {
            var createdFormId = _tFormService.CreateForm(input, User.Identity.Name);
            if (createdFormId < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to create form");

            return Request.CreateResponse(HttpStatusCode.Created, createdFormId);
        }

        [HttpPost]
        [Route("Treasury/Edit")]
        [KflowApiAuthorize(PermissionKey.FID_TreasuryForm_Edit)]
        public HttpResponseMessage Treasury_EditForm([FromBody] TForm input)
        {
            var edittedForm = _tFormService.EditForm(input.Id, input, User.Identity.Name);
            if (edittedForm < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "System unable to save changes. Please check with system admin.");
            return Request.CreateResponse(HttpStatusCode.Created, edittedForm);
        }

        [HttpPost]
        [Route("Treasury/Approval")]
        public HttpResponseMessage Treasury_FormApproval([FromBody] TreasuryFormApprovingReq input)
        {
            var form = _tFormService.ApproveForm(input, User.Identity.Name);
            if (form < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Error in approving the form! Contact system admin for more details");

            return Request.CreateResponse(HttpStatusCode.Accepted, form);
        }

        [HttpDelete]
        [Route("Treasury")]
        public HttpResponseMessage Treasury_FormDelete(FormDataCollection req)
        {
            var key = Convert.ToInt32(req.Get("id"));

            var form = _tFormService.DeleteForm(key, User.Identity.Name);
            if (form < 1) return Request.CreateResponse(HttpStatusCode.BadRequest, "Operation failed. Check with system admin for further details");

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [KflowApiAuthorize(PermissionKey.FID_TreasuryForm_Edit)]
        [HttpPost]
        [Route("Treasury/Retract")]
        public HttpResponseMessage Treasury_Retract(RetractFormVM req)
        {
            var retractFormStatus = _tFormService.WithdrawForm(req.FormId, User.Identity.Name, FormType.FID_TREASURY);
            if (!retractFormStatus) return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to rectract submitted form. Please check with system admin.");

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [KflowApiAuthorize(PermissionKey.FID_TreasuryForm_Edit)]
        [HttpPost]
        [Route("Treasury/Reassign")]
        public HttpResponseMessage Treasury_Reassign(ReassignNewApproverReq req)
        {
            var reassignApproverStatus = _tFormService.ReassignApproverForm(req.formId, req.newApprover, User.Identity.Name);
            if (!reassignApproverStatus) return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to reassign to new approver. Please check with system admin.");

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPost]
        [Route("Treasury/GenFile")]
        [KflowApiAuthorize(Common.PermissionKey.FID_TreasuryForm_Download)]
        public HttpResponseMessage Treasury_GenFile([FromBody] TreasuryForm_PrintReq req)
        {
            var generatedDocId = _tFormService.GenExportFormId(req.formId, User.Identity.Name, req.isExportAsExcel);

            if (string.IsNullOrEmpty(generatedDocId)) return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error. Check application logs.");

            return Request.CreateResponse(HttpStatusCode.Created, generatedDocId);
        }

        #endregion



        #endregion

    }
}
