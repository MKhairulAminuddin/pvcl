using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using xDC_Web.ViewModels.Fid;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, FID")]
    [RoutePrefix("api/fid")]
    public class FidController : ApiController
    {
        [HttpGet]
        [Route("Ts10AmAccountAssignment")]
        public HttpResponseMessage Ts10AmAccountAssignment(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_TS10_TradeItem.GroupBy(x => new { x.FormId}).Select(x => new Ts10AmHomeGridVM
                    {
                        FormId = x.Key.FormId,
                        CountPendingEquity = x.Count(y => y.InstrumentType == "EQUITY" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingBond = x.Count(y => y.InstrumentType == "BOND" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingCp = x.Count(y => y.InstrumentType == "COMMERCIAL PAPER" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingNotesPapers = x.Count(y => y.InstrumentType == "NOTES AND PAPERS" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingRepo = x.Count(y => y.InstrumentType == "REPO" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingCoupon = x.Count(y => y.InstrumentType == "COUPON" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingFees = x.Count(y => y.InstrumentType == "FEES" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingMtm = x.Count(y => y.InstrumentType == "PAYMENT/RECEIVED (MTM)" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingFx = x.Count(y => y.InstrumentType == "FX SETTLEMENT" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingContribution = x.Count(y => y.InstrumentType == "CONTRIBUTION CREDITED" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingAltid = x.Count(y => y.InstrumentType == "ALTID DISTRIBUTION AND DRAWDOWN" && y.OutflowFrom == null && y.InflowTo == null),
                        CountPendingOthers = x.Count(y => y.InstrumentType == "OTHERS" && y.OutflowFrom == null && y.InflowTo == null)
                    }).ToList();

                    var availableForms = db.FID_TS10.ToList();
                    foreach (var form in availableForms)
                    {
                        foreach (var item in result.Where( x => x.FormId == form.Id))
                        {
                            item.Currency = form.Currency;

                            if (form.SettlementDate != null)
                            {
                                item.SettlementDate = form.SettlementDate.Value;
                            }
                        }
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("Ts10AmAccountAssignmentGrid/TradeItem/{tradeType}/{formId}")]
        public HttpResponseMessage Ts10AmAccountAssignmentGrid(string tradeType, int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeItemType = Common.TradeSettlementUrlParamMapping(tradeType);
                    if (string.IsNullOrEmpty(tradeItemType))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trade Item Type");
                    }

                    var tradeItems =
                        db.FID_TS10_TradeItem.Where(x => x.InstrumentType == tradeItemType && x.FormId == formId).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(tradeItems, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
