using DevExpress.Xpo.Logger;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.FID.FcaTaggingForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Form_FcaTagging;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Form;
using xDC.Utils;

namespace xDC.Services
{
    public class FcaTaggingFormService : IFcaTaggingFormService
    {
        private readonly IXDcLogger _logger;

        public FcaTaggingFormService(IXDcLogger logger, ITreasuryFormService tFormService)
        {
            _logger = logger;
        }

        public List<SummaryRes> DxGrid_GetSummaryForIssdView(out bool reqStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var approvedTsForms = db.ISSD_FormHeader.Where(x => x.FormStatus == Common.FormStatus.Approved).Select(x => x.Id).ToList();

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader,
                            a => a.FormId,
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                            )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved && x.ts_form.SettlementDate != null)
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate,
                            x.ts_form.Currency
                        })
                        .Select(x => new SummaryRes
                        {
                            Currency = x.Key.Currency,
                            SettlementDate = x.Key.SettlementDate.Value,

                            CountEquity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity),
                            CountBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond),
                            CountCp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp),
                            CountNotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers),
                            CountRepo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo),
                            CountCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon),
                            CountContribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn),
                            CountAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid),
                            CountOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others),
                            CountFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees),
                            CountMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm),
                            CountFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx),

                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),

                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                           && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),

                            CountUnclassifiedBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond
                                                                 && y.ts_item.BondType == null),
                            CountUnclassifiedCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon
                                                                   && y.ts_item.CouponType == null)
                        })
                        .ToList();

                    reqStatus = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

        public List<SummaryRes> DxGrid_GetSummaryForFidView(out bool reqStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var approvedTsForms = db.ISSD_FormHeader.Where(x => x.FormStatus == Common.FormStatus.Approved).Select(x => x.Id).ToList();

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader,
                            a => a.FormId,
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                            )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved && x.ts_form.SettlementDate != null)
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate,
                            x.ts_form.Currency
                        })
                        .Select(x => new SummaryRes
                        {
                            Currency = x.Key.Currency,
                            SettlementDate = x.Key.SettlementDate.Value,

                            CountEquity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity),
                            CountBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond),
                            CountCp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp),
                            CountNotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers),
                            CountRepo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo),
                            CountCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon),
                            CountContribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn),
                            CountAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid),
                            CountOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others),
                            CountFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees),
                            CountMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm),
                            CountFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx),

                            CountPendingEquity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingCp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingNotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers
                                                                   && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingRepo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                           && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),

                            CountPendingCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingContribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn
                                                                    && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0))
                        })
                    .ToList();

                    reqStatus = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

        public List<DetailRes> DxGrid_GetDetailForIssdView(long settlementDateEpoch, string currency, out bool reqStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader,
                            a => a.FormId,
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                        )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved
                                    && x.ts_form.SettlementDate != null
                                    && DbFunctions.TruncateTime(x.ts_form.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                    && x.ts_form.Currency == currency)
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate,
                            x.ts_form.Currency
                        })
                        .Select(x => new DetailRes()
                        {
                            SettlementDate = x.Key.SettlementDate.Value,
                            Currency = x.Key.Currency,

                            Equity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity) > 0,
                            Bond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond) > 0,
                            Cp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp) > 0,
                            NotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers) > 0,
                            Repo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo) > 0,
                            Coupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon) > 0,
                            Fees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees) > 0,
                            Mtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm) > 0,
                            Fx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx) > 0,
                            Contribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn) > 0,
                            Altid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid) > 0,
                            Others = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others) > 0,

                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),
                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0)),

                            CountUnclassifiedBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond && y.ts_item.BondType == null),
                            CountUnclassifiedCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon && y.ts_item.CouponType == null)

                        }).ToList();

                    reqStatus = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

        public List<DetailRes> DxGrid_GetDetailForFidView(long settlementDateEpoch, string currency, out bool reqStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var result = db.ISSD_TradeSettlement
                        .Join(
                            db.ISSD_FormHeader,
                            a => a.FormId,
                            b => b.Id,
                            (a, b) => new { ts_item = a, ts_form = b }
                        )
                        .Where(x => x.ts_form.FormStatus == Common.FormStatus.Approved
                                    && x.ts_form.SettlementDate != null
                                    && DbFunctions.TruncateTime(x.ts_form.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                    && x.ts_form.Currency == currency)
                        .GroupBy(x => new
                        {
                            x.ts_form.SettlementDate,
                            x.ts_form.Currency
                        })
                        .Select(x => new DetailRes()
                        {
                            SettlementDate = x.Key.SettlementDate.Value,
                            Currency = x.Key.Currency,

                            Equity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity) > 0,
                            Bond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond) > 0,
                            Cp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp) > 0,
                            NotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers) > 0,
                            Repo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo) > 0,
                            Coupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon) > 0,
                            Fees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees) > 0,
                            Mtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm) > 0,
                            Fx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx) > 0,
                            Contribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn) > 0,
                            Altid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid) > 0,
                            Others = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others) > 0,

                            CountPendingEquity = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Equity
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingBond = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Bond
                                    && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingCp = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cp
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingNotesPapers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.NotesPapers
                                                                   && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingRepo = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Repo
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingMtm = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Mtm
                                                           && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFx = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fx
                                                          && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingAltid = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Altid
                                                             && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingOthers = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Others
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),

                            //xde outflow for this table
                            CountPendingCoupon = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Coupon
                                                              && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingContribution = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Cn
                                                                    && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0)),
                            CountPendingFees = x.Count(y => y.ts_item.InstrumentType == Common.TsItemCategory.Fees
                                                            && (y.ts_item.InflowTo == null && y.ts_item.InflowAmount > 0 || y.ts_item.OutflowFrom == null && y.ts_item.OutflowAmount > 0))

                        }).ToList();

                    reqStatus = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

        public List<ISSD_TradeSettlement> DxGrid_GetIndividualGridData(string tradeType, long settlementDateEpoch, string currency, out bool reqStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                    var tradeItemType = Common.TsCategoryUrlParamMapping(tradeType);

                    if (string.IsNullOrEmpty(tradeItemType))
                    {
                        throw new Exception("Invalid Trade Item Type");
                    }

                    var approvedIds = db.ISSD_FormHeader
                        .Where(x => x.FormStatus == Common.FormStatus.Approved
                                             && x.SettlementDate != null
                                             && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                             && x.Currency == currency)
                        .Select(x => x.Id)
                        .ToList();

                    if (!approvedIds.Any())
                    {
                        throw new Exception("Form not found");
                    }

                    var tradeItems =
                        db.ISSD_TradeSettlement
                            .Where(x => x.InstrumentType == tradeItemType && approvedIds.Contains(x.FormId))
                            .ToList();

                    reqStatus = true;
                    return tradeItems;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

        public List<FcaBankAccountRes> List_GetFcaBankAccount(out bool reqStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Config_FcaBankAccount.Select(x => x.AccountName3).ToList();

                    result.Add("RENTAS");
                    result.Add("MMA");

                    reqStatus = true;
                    return result.Select(x => new FcaBankAccountRes { Name = x }).Distinct().OrderBy(x => x.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

        public FcaAccountAssignmentFormVM Page_FcaTaggingForm(long settlementDateEpoch, string currency, out bool reqStatus)
        {
            reqStatus = true;

            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var model = new FcaAccountAssignmentFormVM
                    {
                        Currency = currency,
                        OpeningBalance = new List<TsOpeningBalance>()
                    };

                    if (settlementDate != null)
                    {
                        model.SettlementDate = settlementDate.Value;

                        var ob = FcaTaggingSvc.GetOpeningBalance(db, settlementDate.Value, currency);
                        model.OpeningBalance.AddRange(ob);
                        var totalOb = model.OpeningBalance.Sum(x => x.Amount);

                        var totalInflow = FcaTaggingSvc.TotalInflow(db, settlementDate.Value, currency);
                        var totalOutflow = FcaTaggingSvc.TotalOutflow(db, settlementDate.Value, currency);

                        model.ClosingBalance = totalOb + totalInflow - totalOutflow;
                        return model;
                    }
                    else
                    {
                        reqStatus = false;
                        return null;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                reqStatus = false;
                return null;
            }
        }

    }
}
