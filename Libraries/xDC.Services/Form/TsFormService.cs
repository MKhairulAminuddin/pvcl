using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Xml.Linq;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web;
using xDC.Domain.Web.Application;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Audit;
using xDC.Services.FileGenerator;
using xDC.Services.Membership;
using xDC.Services.Notification;
using xDC.Services.Workflow;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.Form
{
    /// <summary>
    /// Trade Settlement Form
    /// </summary>
    public class TsFormService : FormService, ITsFormService
    {
        #region Fields

        private readonly IAuditService _auditService;
        private readonly IXDcLogger _logger;
        private readonly IWorkflowService _wfService;
        private readonly IGenFile_TsForm _genFile;
        private readonly IRoleManagementService _roleService;

        #endregion

        #region Ctor

        public TsFormService(IWorkflowService wfService, INotificationService notifyService, IXDcLogger logger, IAuditService auditService, IGenFile_TsForm genFile, IRoleManagementService roleService)
            : base(wfService, notifyService, logger, auditService, roleService)
        {
            _wfService = wfService;
            _auditService = auditService;
            _logger = logger;
            _genFile = genFile;
            _roleService = roleService;
        }

        #endregion

        #region Landing Page

        public TsLandingPage GetLandingPageData(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var enableCreateForm = _roleService.IsUserHaveAccess(currentUser, PermissionKey.ISSD_TradeSettlementForm_Edit);
                    var today = DateTime.Now;

                    var model = new TsLandingPage()
                    {
                        CountTodaySubmission = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(FormType.ISSD_TS)
                                                            && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                            && x.FormStatus != FormStatus.Draft),
                        CountTodayPendingApproval = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(FormType.ISSD_TS)
                                                                && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == FormStatus.PendingApproval),
                        CountTodayApproved = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(FormType.ISSD_TS)
                                                                && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == FormStatus.Approved),
                        CountTodayRejected = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(FormType.ISSD_TS)
                                                                && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == FormStatus.Rejected),
                        EnableCreateForm = enableCreateForm
                    };

                    return model;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<TsConsolidatedPage> GetConsolidatedPage(long settlementDateEpoch, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = ConvertEpochToDateTime(settlementDateEpoch);

                    var trades = GetTradeSettlement(db, settlementDate.Value.Date, currency);

                    var result = trades
                        .GroupBy(i => 1)
                        .Select(x => new TsConsolidatedPage()
                        {
                            SettlementDate = settlementDate.Value,
                            Currency = currency,

                            TotalEquity = x.Count(y => y.InstrumentType == TsItemCategory.Equity),
                            TotalBond = x.Count(y => y.InstrumentType == TsItemCategory.Bond),
                            TotalCp = x.Count(y => y.InstrumentType == TsItemCategory.Cp),
                            TotalNotesPapers = x.Count(y => y.InstrumentType == TsItemCategory.NotesPapers),
                            TotalRepo = x.Count(y => y.InstrumentType == TsItemCategory.Repo),
                            TotalCoupon = x.Count(y => y.InstrumentType == TsItemCategory.Coupon),
                            TotalFees = x.Count(y => y.InstrumentType == TsItemCategory.Fees),
                            TotalMtm = x.Count(y => y.InstrumentType == TsItemCategory.Mtm),
                            TotalFx = x.Count(y => y.InstrumentType == TsItemCategory.Fx),
                            TotalCn = x.Count(y => y.InstrumentType == TsItemCategory.Cn),
                            TotalAltid = x.Count(y => y.InstrumentType == TsItemCategory.Altid),
                            TotalOthers = x.Count(y => y.InstrumentType == TsItemCategory.Others),

                            FormIdEquity = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Equity)?.FormId,
                            FormIdBond = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Bond)?.FormId,
                            FormIdCp = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Cp)?.FormId,
                            FormIdNotesPapers = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.NotesPapers)?.FormId,
                            FormIdRepo = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Repo)?.FormId,
                            FormIdCoupon = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Coupon)?.FormId,
                            FormIdFees = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Fees)?.FormId,
                            FormIdMtm = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Mtm)?.FormId,
                            FormIdFx = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Fx)?.FormId,
                            FormIdCn = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Cn)?.FormId,
                            FormIdAltid = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Altid)?.FormId,
                            FormIdOthers = x.FirstOrDefault(y => y.InstrumentType == TsItemCategory.Others)?.FormId,

                        }).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        #endregion

        #region Grid

        public List<TsHomeGrid1> GetTsHomeGrid1(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todayDate = DateTime.Now.Date;

                    var tsForms = db.ISSD_FormHeader;
                    var resultVM = new List<TsHomeGrid1>();

                    foreach (var item in tsForms)
                    {
                        resultVM.Add(new TsHomeGrid1
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormDate = item.SettlementDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = EnableEdit(item.FormStatus, currentUser, PermissionKey.ISSD_TradeSettlementForm_Edit),
                            EnableDelete = EnableDelete(item.FormStatus, item.ApprovedBy, currentUser, PermissionKey.ISSD_TradeSettlementForm_Edit),
                            EnablePrint = EnablePrint(currentUser, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Download),
                            EnableRetractSubmission = EnableFormWithdrawal(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Edit),

                            IsRejected = currentUser == item.PreparedBy && item.FormStatus == FormStatus.Rejected,
                            IsPendingMyApproval = currentUser == item.ApprovedBy && item.FormStatus == FormStatus.PendingApproval,
                            IsPendingApproval = item.FormStatus == FormStatus.PendingApproval
                        });
                    }

                    return resultVM;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public List<TsHomeGrid2> GetTsHomeGrid2(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.ISSD_FormHeader
                        .Where(x => x.FormStatus == FormStatus.Approved)
                        .GroupBy(x => new { x.SettlementDate, x.Currency })
                        .Select(x => new
                        {
                            x.Key.SettlementDate,
                            x.Key.Currency,
                            ApprovedDate = x.Max(i => i.ApprovedDate)
                        });

                    var resultVM = new List<TsHomeGrid2>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new TsHomeGrid2
                        {
                            FormDate = item.SettlementDate,
                            Currency = item.Currency,
                            ApprovedDate = item.ApprovedDate,
                            EnablePrint = EnablePrint(currentUser, FormStatus.Approved, PermissionKey.ISSD_TradeSettlementForm_Download)
                        });
                    }

                    return resultVM;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public List<ISSD_TradeSettlement> GetTsItemsGrid(int formId, string instrumentType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.ISSD_TradeSettlement.Where(x => x.InstrumentType == instrumentType && x.FormId == formId).ToList();

                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        #endregion

        #region Form Page

        public bool CreateForm(TsCreateNewFormRequest req, string currentUser, out int createdFormId)
        {
            createdFormId = 0;

            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = new ISSD_FormHeader()
                    {
                        FormType = FormTypeMapping(req.FormType),
                        PreparedBy = currentUser,
                        PreparedDate = DateTime.Now,
                        FormStatus = req.IsSaveAsDraft ? FormStatus.Draft : FormStatus.PendingApproval,
                        SettlementDate = ConvertEpochToDateTime(req.SettlementDateEpoch)?.Date,
                        Currency = req.Currency.ToUpper(),
                        ApprovedBy = req.IsSaveAsDraft ? null : req.Approver
                    };

                    db.ISSD_FormHeader.Add(form);
                    var formCreated = db.SaveChanges();

                    if (formCreated > 0)
                    {
                        var newTrades = new List<ISSD_TradeSettlement>();

                        if (req.Equity.Any())
                        {
                            NewTsObjMapping(req.Equity, form.Id, TsItemCategory.Equity, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Bond.Any())
                        {
                            NewTsObjMapping(req.Bond, form.Id, TsItemCategory.Bond, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Cp.Any())
                        {
                            NewTsObjMapping(req.Cp, form.Id, TsItemCategory.Cp, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.NotesPaper.Any())
                        {
                            NewTsObjMapping(req.NotesPaper, form.Id, TsItemCategory.NotesPapers, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Repo.Any())
                        {
                            NewTsObjMapping(req.Repo, form.Id, TsItemCategory.Repo, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Coupon.Any())
                        {
                            NewTsObjMapping(req.Coupon, form.Id, TsItemCategory.Coupon, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Fees.Any())
                        {
                            NewTsObjMapping(req.Fees, form.Id, TsItemCategory.Fees, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Mtm.Any())
                        {
                            NewTsObjMapping(req.Mtm, form.Id, TsItemCategory.Mtm, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.FxSettlement.Any())
                        {
                            NewTsObjMapping(req.FxSettlement, form.Id, TsItemCategory.Fx, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.ContributionCredited.Any())
                        {
                            NewTsObjMapping(req.ContributionCredited, form.Id, TsItemCategory.Cn, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Altid.Any())
                        {
                            NewTsObjMapping(req.Altid, form.Id, TsItemCategory.Altid, req.Currency, currentUser, ref newTrades);
                        }

                        if (req.Others.Any())
                        {
                            NewTsObjMapping(req.Others, form.Id, TsItemCategory.Others, req.Currency, currentUser, ref newTrades);
                        }

                        db.ISSD_TradeSettlement.AddRange(newTrades);
                        var createdFormItems = db.SaveChanges();

                        if (createdFormItems > 0)
                        {
                            createdFormId = form.Id;
                            if (form.FormStatus == FormStatus.PendingApproval)
                            {
                                Create(form.Id, form.FormType, form.SettlementDate, form.PreparedBy, form.ApprovedBy, req.ApprovalNotes);
                            }
                            return true;
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool EditForm(TsCreateNewFormRequest req, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == req.Id);

                    if (req.IsSaveAdminEdit)
                    {
                        form.AdminEditted = true;
                        form.AdminEdittedBy = currentUser;
                        form.AdminEdittedDate = DateTime.Now;
                    }

                    if (req.IsSaveAsDraft)
                    {
                        form.PreparedBy = currentUser;
                        form.PreparedDate = DateTime.Now;
                    }

                    if (req.Approver != null && form.FormStatus == FormStatus.Draft)
                    {
                        _auditService.FA_AssignApprover(form.Id, form.FormType, form.SettlementDate, currentUser,
                            form.ApprovedBy, req.Approver);

                        form.ApprovedBy = req.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = FormStatus.PendingApproval;
                    }

                    var formTradeItems = db.ISSD_TradeSettlement.Where(x => x.FormId == form.Id).ToList();
                    var cc_itemBeforeList = formTradeItems.ToList();

                    if (req.Equity != null)
                    {
                        // delete from existing
                        if (req.Equity.Any())
                        {
                            var itemExistInGrid =
                                req.Equity.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Equity)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Equity - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "Equity - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "Equity - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Equity - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Equity - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Equity - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Equity, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" Equity: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Bond != null)
                    {
                        // delete from existing
                        if (req.Bond.Any())
                        {
                            var itemExistInGrid =
                                req.Bond.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Bond)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "BOND - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "BOND - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "BOND - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "BOND - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "BOND - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.BondType != item.BondType)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.BondType, item.BondType, "BOND - Bond Type");

                                    foundItem.BondType = item.BondType;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "BOND - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Bond, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" BOND: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.BondType}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Cp != null)
                    {
                        // delete from existing
                        if (req.Cp.Any())
                        {
                            var itemExistInGrid =
                                req.Cp.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Cp)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "CP - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "CP - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "CP - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "CP - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "CP - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "CP - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Cp, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" CP: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.NotesPaper != null)
                    {
                        // delete from existing
                        if (req.NotesPaper.Any())
                        {
                            var itemExistInGrid =
                                req.NotesPaper.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.NotesPaper)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Notes & Paper - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "Notes & Paper - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "Notes & Paper - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Notes & Paper - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Notes & Paper - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Notes & Paper - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.NotesPapers, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" Notes & Paper: {item.InstrumentCode}, {item.StockCode}, {item.Maturity}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Repo != null)
                    {
                        // delete from existing
                        if (req.Repo.Any())
                        {
                            var itemExistInGrid =
                                req.Repo.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Repo)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "REPO - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "REPO - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.FirstLeg != item.FirstLeg)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.FirstLeg.ToString(), item.FirstLeg.ToString(), "REPO - First Leg");

                                    foundItem.FirstLeg = item.FirstLeg;
                                }

                                if (foundItem.SecondLeg != item.SecondLeg)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.SecondLeg.ToString(), item.SecondLeg.ToString(), "REPO - Second Leg");

                                    foundItem.SecondLeg = item.SecondLeg;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "REPO - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Repo, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" REPO: {item.InstrumentCode}, {item.StockCode}, {item.FirstLeg}, {item.SecondLeg}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Coupon != null)
                    {
                        // delete from existing
                        if (req.Coupon.Any())
                        {
                            var itemExistInGrid =
                                req.Coupon.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Coupon)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Coupon - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "Coupon - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Coupon - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Coupon - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                if (foundItem.CouponType != item.CouponType)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.CouponType, item.CouponType, "Coupon - Coupon Type");

                                    foundItem.CouponType = item.CouponType;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Coupon, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" Coupon: {item.InstrumentCode}, {item.StockCode}, {item.AmountPlus}, {item.Remarks}, {item.CouponType}");
                            }
                        }
                    }

                    if (req.Mtm != null)
                    {
                        // delete from existing
                        if (req.Mtm.Any())
                        {
                            var itemExistInGrid =
                                req.Mtm.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Mtm)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "MTM - Payment/Receipt");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "MTM - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "MTM - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "MTM - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Mtm, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" MTM: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.FxSettlement != null)
                    {
                        // delete from existing
                        if (req.FxSettlement.Any())
                        {
                            var itemExistInGrid =
                                req.FxSettlement.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.FxSettlement)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "FX - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "FX - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "FX - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "FX - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Fx, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" FX: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Altid != null)
                    {
                        // delete from existing
                        if (req.Altid.Any())
                        {
                            var itemExistInGrid =
                                req.Altid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Altid)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "ALTID - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "ALTID - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "ALTID - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "ALTID - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Altid, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" ALTID: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.ContributionCredited != null)
                    {
                        // delete from existing
                        if (req.ContributionCredited.Any())
                        {
                            var itemExistInGrid =
                                req.ContributionCredited.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.ContributionCredited)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Contribution - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Contribution - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Contribution - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }

                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Cn, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" Contribution: {item.InstrumentCode}, {item.AmountPlus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Fees != null)
                    {
                        // delete from existing
                        if (req.Fees.Any())
                        {
                            var itemExistInGrid =
                                req.Fees.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Fees)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Fees - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Fees - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Fees - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Fees - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Fees, req.Currency, currentUser));

                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.SettlementDate, currentUser,
                                    $" Fees: {item.InstrumentCode}, {item.AmountPlus}, {item.AmountMinus}, {item.Remarks}");
                            }
                        }
                    }

                    if (req.Others != null)
                    {
                        // delete from existing
                        if (req.Others.Any())
                        {
                            var itemExistInGrid =
                                req.Others.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                            var removedItems =
                                formTradeItems.Where(x => !itemExistInGrid.Contains(x.Id)).ToList();
                            if (removedItems.Any())
                            {
                                foreach (var item in removedItems)
                                {
                                    _auditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        item.InstrumentCode);
                                }
                                db.ISSD_TradeSettlement.RemoveRange(removedItems);
                            }
                        }

                        foreach (var item in req.Others)
                        {
                            var foundItem = formTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Others - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Others - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Others - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Others - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                if (foundItem.OthersType != item.OthersType)
                                {
                                    _auditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.OthersType, item.OthersType, "Others - Others Type");

                                    foundItem.OthersType = item.OthersType;
                                }

                                foundItem.InflowAmount = SumInflowAmount(item);
                                foundItem.OutflowAmount = SumOutflowAmount(item);
                                foundItem.ModifiedBy = currentUser;
                                foundItem.ModifiedDate = DateTime.Now;

                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, TsItemCategory.Others, req.Currency, currentUser));
                            }
                        }
                    }

                    var formChangesSaved = db.SaveChanges();

                    if (formChangesSaved > 0)
                    {
                        if (form.FormStatus == FormStatus.PendingApproval)
                        {
                            Create(form.Id, form.FormType, form.SettlementDate, form.PreparedBy, form.ApprovedBy, req.ApprovalNotes);
                        }
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool DeleteForm(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        db.ISSD_FormHeader.Remove(form);

                        var tradeItems = db.ISSD_TradeSettlement.Where(x => x.FormId == formId);
                        if (tradeItems.Any())
                        {
                            db.ISSD_TradeSettlement.RemoveRange(tradeItems);
                        }

                        var deletionAccepted = db.SaveChanges();

                        if (deletionAccepted > 0)
                        {
                            Delete(form.Id, form.FormType, form.SettlementDate, currentUser);
                            return true;
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }

        public bool FormApproval(TsFormApprovalRequest req, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(req.FormId);
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        if (form.ApprovedBy == currentUser)
                        {
                            form.ApprovedDate = DateTime.Now;
                            form.FormStatus = req.ApprovalStatus ? FormStatus.Approved : FormStatus.Rejected;
                            var approvalSaved = db.SaveChanges();

                            if (approvalSaved > 0)
                            {
                                ApprovalResponse(form.Id, form.FormType, form.SettlementDate, form.PreparedBy, form.ApprovedBy, req.ApprovalNote, form.FormStatus);

                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public TsForm ViewConsolidatedForm(string settlementDateEpoch, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(Convert.ToInt64(settlementDateEpoch));
                    var settlementDateOnly = settlementDate.Value.Date;

                    var form = db.ISSD_FormHeader.Where(x =>
                        DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDateOnly) 
                        && x.Currency == currency);

                    if (!form.Any()) return null;

                    var consolidatedForm = new TsForm()
                    {
                        FormStatus = form.First().FormStatus,
                        SettlementDate = form.First().SettlementDate,
                        Currency = form.First().Currency,
                        OpeningBalance = new List<TsOpeningBalance>()
                    };

                    var ob = GetOpeningBalance(settlementDateOnly, currency);
                    consolidatedForm.OpeningBalance.AddRange(ob);

                    var totalOb = consolidatedForm.OpeningBalance.Sum(x => x.Amount);
                    var totalFlow = GetTotalFlow(db, form.Select(x => x.Id).ToList(), settlementDateOnly, currency);

                    consolidatedForm.ClosingBalance = totalOb + totalFlow.Inflow - totalFlow.Outflow;

                    return consolidatedForm;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public TsForm ViewForm(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return null;

                    var wf = _wfService.Info(form.Id, form.FormType)
                        .Where(x => x.WorkflowStatus == FormStatus.PendingApproval)
                        .OrderByDescending(x => x.RecordedDate)
                        .FirstOrDefault();

                    var outputForm = new TsForm()
                    {
                        Id = form.Id,
                        FormStatus = form.FormStatus,
                        FormType = form.FormType,
                        SettlementDate = form.SettlementDate,
                        Currency = form.Currency,

                        PreparedBy = form.PreparedBy,
                        PreparedDate = form.PreparedDate,

                        IsApproved = form.FormStatus == FormStatus.Approved,
                        ApprovedBy = form.ApprovedBy,
                        ApprovedDate = form.ApprovedDate,
                        ApprovalNote = wf?.WorkflowNotes,

                        IsAdminEdited = form.AdminEditted,
                        AdminEditedBy = form.AdminEdittedBy,
                        AdminEditedDate = form.AdminEdittedDate,

                        EnableApproveRejectBtn = EnableFormApproval(currentUser, form.ApprovedBy, form.FormStatus, db),
                        EnableReassign = EnableReassignApprover(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.ISSD_TradeSettlementForm_Edit)
                    };

                    return outputForm;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public TsForm ViewEditForm(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return null;

                    var wf = _wfService.Info(form.Id, form.FormType)
                        //.Where(x => x.WorkflowStatus == FormStatus.PendingApproval)
                        .OrderByDescending(x => x.RecordedDate)
                        .FirstOrDefault();

                    var outputForm = new TsForm()
                    {
                        Id = form.Id,
                        FormStatus = form.FormStatus,
                        SettlementDate = form.SettlementDate,
                        Currency = form.Currency,

                        PreparedBy = form.PreparedBy,
                        PreparedDate = form.PreparedDate,

                        IsApproved = form.FormStatus == Common.FormStatus.Approved,
                        ApprovedBy = form.ApprovedBy,
                        ApprovedDate = form.ApprovedDate,
                        ApprovalNote = wf?.WorkflowNotes,

                        IsAdminEdited = form.AdminEditted,
                        AdminEditedBy = form.AdminEdittedBy,
                        AdminEditedDate = form.AdminEdittedDate,

                        EnableResubmit = EnableResubmission(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.ISSD_TradeSettlementForm_Edit),
                        EnableSubmitForApproval = EnableApprovalSubmission(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.ISSD_TradeSettlementForm_Edit),

                        EnableDraftButton = EnableSaveAsDraft(currentUser, form.FormStatus, form.PreparedBy, form.ApprovedBy),
                        EnableSaveAdminChanges = false, // TODO: Admin Changes
                    };

                    return outputForm;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public TsForm ViewNewForm(string formType, string currentUser)
        {
            var form = new TsForm()
            {
                PreparedBy = currentUser,
                PreparedDate = DateTime.Now,
                FormStatus = Common.FormStatus.Draft,
                EnableDraftButton = true,
                FormType = formType
            };

            return form;
        }

        public bool WithdrawForm(int formId, string performedBy, string formType)
        {
            var withdrawFormStatus = this.RetractFormSubmission(formId, performedBy, formType);
            return withdrawFormStatus;
        }

        public bool ReassignApproverForm(int formId, string newApprover, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return false;

                    var permittedApprover = db.Config_Approver
                        .FirstOrDefault(x =>
                            x.FormType == form.FormType && x.Username != currentUser && x.Username == newApprover);
                    if (form == null) return false;

                    var currentApprover = form.ApprovedBy;
                    form.ApprovedBy = newApprover;
                    form.ApprovedDate = null;

                    var reassignStatus = db.SaveChanges();
                    if (reassignStatus < 1) return false;

                    ReassignApprover(formId, form.FormType, form.SettlementDate, currentUser, currentApprover, form.ApprovedBy);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        #endregion

        public List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var forms = db.ISSD_FormHeader.Where(x =>
                DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate) &&
                x.Currency == currency);

            var trades = new List<ISSD_TradeSettlement>();

            if (forms.Any())
            {
                foreach (var formId in forms.Select(x => x.Id).ToList())
                {
                    var tradesFromId = db.ISSD_TradeSettlement.Where(x => x.FormId == formId).ToList();
                    if (tradesFromId.Any())
                    {
                        trades.AddRange(tradesFromId);
                    }
                }

            }

            return trades;
        }

        public List<TsOpeningBalance> GetOpeningBalance(DateTime settlementDate, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<TsOpeningBalance>();

                    var ob = db.EDW_BankBalance
                        .Where(x => DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate)
                                    && x.Currency == currency)
                        .GroupBy(x => new { x.SettlementDate, x.InstrumentType })
                        .Select(x => new
                        {
                            account = x.Key.InstrumentType,
                            total = x.Sum(y => y.Amount ?? 0)
                        });

                    foreach (var item in ob)
                    {
                        result.Add(new TsOpeningBalance()
                        {
                            Account = item.account,
                            Amount = item.total
                        });
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public TS_TotalFlow GetTotalFlow(kashflowDBEntities db, List<int> formId, DateTime settlementDate, string currency)
        {
            var trades = new List<ISSD_TradeSettlement>();

            foreach (var id in formId)
            {
                var tradesFromId = db.ISSD_TradeSettlement.Where(x => x.FormId == id).ToList();
                if (tradesFromId.Any())
                {
                    trades.AddRange(tradesFromId);
                }
            }

            var totalFlow = trades.GroupBy(x => 1)
                .Select(x => new
                {
                    totalMaturity = x.Sum(y => y.Maturity),
                    totalSales = x.Sum(y => y.Sales),
                    totalFirstLeg = x.Sum(y => y.FirstLeg),
                    totalAmountPlus = x.Sum(y => y.AmountPlus),

                    totalPurchase = x.Sum(y => y.Purchase),
                    totalSecondLeg = x.Sum(y => y.SecondLeg),
                    totalAmountMinus = x.Sum(y => y.AmountMinus)
                })
                .FirstOrDefault();

            double cbInflow = 0;
            double cbOutflow = 0;

            if (totalFlow != null)
            {
                cbInflow = totalFlow.totalMaturity + totalFlow.totalSales + totalFlow.totalFirstLeg +
                           totalFlow.totalAmountPlus;
                cbOutflow = totalFlow.totalPurchase + totalFlow.totalSecondLeg + totalFlow.totalAmountMinus;
            }

            var result = new TS_TotalFlow()
            {
                Currency = currency,
                SettlementDate = settlementDate,
                Inflow = cbInflow,
                Outflow = cbOutflow
            };

            return result;
        }

        public double GetTotalInflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => approvedFormIds.Contains(x.FormId) && x.InstrumentType == category)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountPlus = x.Sum(y => y.AmountPlus),
                    Sales = x.Sum(y => y.Sales),
                    Maturity = x.Sum(y => y.Maturity),
                    FirstLeg = x.Sum(y => y.FirstLeg)
                })
                .Select(x => x.Sales + x.Maturity + x.AmountPlus + x.FirstLeg)
                .FirstOrDefault();

            return result;
        }

        public double GetTotalOutflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => approvedFormIds.Contains(x.FormId) && x.InstrumentType == category)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountMinus = x.Sum(y => y.AmountMinus),
                    Purchase = x.Sum(y => y.Purchase),
                    SecondLeg = x.Sum(y => y.SecondLeg)
                })
                .Select(x => x.AmountMinus + x.Purchase + x.SecondLeg)
                .FirstOrDefault();

            return result;
        }

        public List<TsOpeningBalance> OpeningBalanceSummary(long submissionDateEpoch = 0)
        {
            try
            {
                DateTime selectedDate;
                if (submissionDateEpoch != 0)
                {
                    selectedDate = ConvertEpochToDateTime(submissionDateEpoch).Value;
                }
                else
                {
                    selectedDate = DateTime.Now;
                }

                using (var db = new kashflowDBEntities())
                {
                    var openingBalanceSummary = FcaTaggingSvc.GetOpeningBalance(db, selectedDate);

                    if (openingBalanceSummary.Any())
                    {
                        return openingBalanceSummary.OrderBy(x => x.Currency).ThenBy(x => x.Account).ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<TsFormSummary> TsFormSummaryList(long submissionDateEpoch = 0)
        {
            try
            {
                DateTime selectedDate;
                if (submissionDateEpoch != 0)
                {
                    selectedDate = ConvertEpochToDateTime(submissionDateEpoch).Value;
                }
                else
                {
                    selectedDate = DateTime.Now;
                }

                using (var db = new kashflowDBEntities())
                {
                    var tsForms = db.ISSD_FormHeader.Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate))
                        .Select(x => new TsFormSummary()
                        {
                            FormId = x.Id,
                            SettlementDate = x.SettlementDate,
                            Currency = x.Currency,
                            PreparedBy = x.PreparedBy,
                            SubmittedDate = x.PreparedDate,
                            ApprovedBy = x.ApprovedBy,
                            ApprovedDate = x.ApprovedDate,
                            FormType = x.FormType,
                            FormStatus = x.FormStatus
                        }).ToList();

                    if (tsForms.Any())
                    {
                        return tsForms;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public string GenExportFormId(int formId, string currentUser, bool isExportToExcel)
        {
            return _genFile.GenId_TsForm(formId, currentUser, isExportToExcel);
        }

        public string GenExportConsolidatedFormId(DateTime settlementDate, string currency, string currentUser, bool isExportToExcel)
        {
            return _genFile.GenId_ConsolidatedTsForm(settlementDate, currency, currentUser, isExportToExcel);
        }

        public string GenCnMailId(int formId, string senderEmail)
        {
            return _genFile.Gen_CnEmailFile(formId, senderEmail);
        }

        public ExportedFile GetGeneratedForm(string generatedFileId)
        {
            return _genFile.GenFile(generatedFileId);
        }

        #region Private Functions

        private void NewTsObjMapping(List<TsTradeItem> trades, int formId, string category, string currency, string currentUser, ref List<ISSD_TradeSettlement> tsItemObj)
        {
            foreach (var item in trades)
            {
                tsItemObj.Add(new ISSD_TradeSettlement
                {
                    FormId = formId,
                    InstrumentType = category,
                    InstrumentCode = item.InstrumentCode,
                    StockCode = item.StockCode,
                    Maturity = item.Maturity,
                    Sales = item.Sales,
                    Purchase = item.Purchase,
                    FirstLeg = item.FirstLeg,
                    SecondLeg = item.SecondLeg,
                    AmountPlus = item.AmountPlus,
                    AmountMinus = item.AmountMinus,
                    Remarks = item.Remarks,
                    ModifiedBy = currentUser,
                    ModifiedDate = DateTime.Now,

                    InflowAmount = item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg,
                    OutflowAmount = item.AmountMinus + item.Purchase + item.SecondLeg,

                    InflowTo = item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                    OutflowFrom = item.AmountMinus + item.Purchase + item.SecondLeg > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                    AssignedBy = null,
                    AssignedDate = null,

                    OthersType = item.OthersType,
                    CouponType = item.CouponType,
                    BondType = item.BondType
                });
            }
        }

        private ISSD_TradeSettlement NewTsObjMapping(TsTradeItem item, int formId, string category, string currency, string currentUser)
        {
            return new ISSD_TradeSettlement
            {
                FormId = formId,
                InstrumentType = category,
                InstrumentCode = item.InstrumentCode,
                StockCode = item.StockCode,
                Maturity = item.Maturity,
                Sales = item.Sales,
                Purchase = item.Purchase,
                FirstLeg = item.FirstLeg,
                SecondLeg = item.SecondLeg,
                AmountPlus = item.AmountPlus,
                AmountMinus = item.AmountMinus,
                Remarks = item.Remarks,
                ModifiedBy = currentUser,
                ModifiedDate = DateTime.Now,

                InflowAmount = item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg,
                OutflowAmount = item.AmountMinus + item.Purchase + item.SecondLeg,

                InflowTo = item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                OutflowFrom = item.AmountMinus + item.Purchase + item.SecondLeg > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                AssignedBy = null,
                AssignedDate = null,

                OthersType = item.OthersType,
                CouponType = item.CouponType,
                BondType = item.BondType
            };
        }

        private double SumInflowAmount(TsTradeItem item)
        {
            return item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg;
        }

        private double SumOutflowAmount(TsTradeItem item)
        {
            return item.AmountMinus + item.Purchase + item.SecondLeg;
        }

        #endregion

    }
}
