using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.App
{
    public static class TsFormService
    {
        #region Landing Page

        public static TsLandingPage GetLandingPageData(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var enableCreateForm = new AuthService().IsUserHaveAccess(currentUser, Common.PermissionKey.ISSD_TradeSettlementForm_Edit);
                    var today = DateTime.Now;

                    var model = new TsLandingPage()
                    {
                        CountTodaySubmission = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(Common.FormType.ISSD_TS)
                                                            && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                            && x.FormStatus != Common.FormStatus.Draft),
                        CountTodayPendingApproval = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(Common.FormType.ISSD_TS)
                                                                && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == Common.FormStatus.PendingApproval),
                        CountTodayApproved = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(Common.FormType.ISSD_TS)
                                                                && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == Common.FormStatus.Approved),
                        CountTodayRejected = db.ISSD_FormHeader
                                                        .Count(x => x.FormType.Contains(Common.FormType.ISSD_TS)
                                                                && DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == Common.FormStatus.Rejected),
                        EnableCreateForm = enableCreateForm
                    };

                    return model;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public static List<TsConsolidatedPage> GetConsolidatedPage(long settlementDateEpoch, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    var trades = TsFormService.GetTradeSettlement(db, settlementDate.Value.Date, currency);

                    var result = trades
                        .GroupBy(i => 1)
                        .Select(x => new TsConsolidatedPage()
                        {
                            SettlementDate = settlementDate.Value,
                            Currency = currency,

                            TotalEquity = x.Count(y => y.InstrumentType == Common.TsItemCategory.Equity),
                            TotalBond = x.Count(y => y.InstrumentType == Common.TsItemCategory.Bond),
                            TotalCp = x.Count(y => y.InstrumentType == Common.TsItemCategory.Cp),
                            TotalNotesPapers = x.Count(y => y.InstrumentType == Common.TsItemCategory.NotesPapers),
                            TotalRepo = x.Count(y => y.InstrumentType == Common.TsItemCategory.Repo),
                            TotalCoupon = x.Count(y => y.InstrumentType == Common.TsItemCategory.Coupon),
                            TotalFees = x.Count(y => y.InstrumentType == Common.TsItemCategory.Fees),
                            TotalMtm = x.Count(y => y.InstrumentType == Common.TsItemCategory.Mtm),
                            TotalFx = x.Count(y => y.InstrumentType == Common.TsItemCategory.Fx),
                            TotalCn = x.Count(y => y.InstrumentType == Common.TsItemCategory.Cn),
                            TotalAltid = x.Count(y => y.InstrumentType == Common.TsItemCategory.Altid),
                            TotalOthers = x.Count(y => y.InstrumentType == Common.TsItemCategory.Others),

                            FormIdEquity = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Equity)?.FormId,
                            FormIdBond = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Bond)?.FormId,
                            FormIdCp = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Cp)?.FormId,
                            FormIdNotesPapers = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.NotesPapers)?.FormId,
                            FormIdRepo = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Repo)?.FormId,
                            FormIdCoupon = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Coupon)?.FormId,
                            FormIdFees = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Fees)?.FormId,
                            FormIdMtm = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Mtm)?.FormId,
                            FormIdFx = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Fx)?.FormId,
                            FormIdCn = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Cn)?.FormId,
                            FormIdAltid = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Altid)?.FormId,
                            FormIdOthers = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Others)?.FormId,

                        }).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        #endregion

        #region Grid

        public static List<TsHomeGrid1> GetTsHomeGrid1(string currentUser)
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

                            EnableEdit = FormService.EnableEdit(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnableDelete = FormService.EnableDelete(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnablePrint = FormService.EnablePrint(currentUser, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Download),
                            EnableRetractSubmission = FormService.EnableRetractSubmission(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Edit),

                            IsRejected = (currentUser == item.PreparedBy && item.FormStatus == Common.FormStatus.Rejected),
                            IsPendingMyApproval = (currentUser == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),
                            IsPendingApproval = item.FormStatus == FormStatus.PendingApproval
                        });
                    }

                    return resultVM;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public static List<TsHomeGrid2> GetTsHomeGrid2(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.ISSD_FormHeader
                        .Where(x => x.FormStatus == Common.FormStatus.Approved)
                        .GroupBy(x => new { x.SettlementDate, x.Currency })
                        .Select(x => new
                        {
                            SettlementDate = x.Key.SettlementDate,
                            Currency = x.Key.Currency,
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
                            EnablePrint = FormService.EnablePrint(currentUser, FormStatus.Approved, PermissionKey.ISSD_TradeSettlementForm_Download)
                        });
                    }

                    return resultVM;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public static List<ISSD_TradeSettlement> GetTsItemsGrid(int formId, string instrumentType)
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
                Logger.LogError(ex.Message);
                return null;
            }
        }

        #endregion

        #region Form Page

        public static int CreateNewForm(TsCreateNewFormRequest req, string currentUser)
        {
            try
            {
                var settlementDateConverted = Common.ConvertEpochToDateTime(req.SettlementDateEpoch);
                req.Currency = req.Currency.ToUpper();

                using (var db = new kashflowDBEntities())
                {
                    var newFormHeader = new ISSD_FormHeader()
                    {
                        FormType = Common.FormTypeMapping(req.FormType),
                        PreparedBy = currentUser,
                        PreparedDate = DateTime.Now,
                        FormStatus = (req.IsSaveAsDraft) ? Common.FormStatus.Draft : Common.FormStatus.PendingApproval,
                        SettlementDate = Common.ConvertEpochToDateTime(req.SettlementDateEpoch)?.Date,
                        Currency = req.Currency,
                        ApprovedBy = (req.IsSaveAsDraft) ? null : req.Approver
                    };

                    db.ISSD_FormHeader.Add(newFormHeader);
                    db.SaveChanges();

                    var newTrades = new List<ISSD_TradeSettlement>();

                    if (req.Equity.Any())
                    {
                        NewTsObjMapping(req.Equity, newFormHeader.Id, Common.TsItemCategory.Equity, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Bond.Any())
                    {
                        NewTsObjMapping(req.Bond, newFormHeader.Id, Common.TsItemCategory.Bond, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Cp.Any())
                    {
                        NewTsObjMapping(req.Cp, newFormHeader.Id, Common.TsItemCategory.Cp, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.NotesPaper.Any())
                    {
                        NewTsObjMapping(req.NotesPaper, newFormHeader.Id, Common.TsItemCategory.NotesPapers, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Repo.Any())
                    {
                        NewTsObjMapping(req.Repo, newFormHeader.Id, Common.TsItemCategory.Repo, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Coupon.Any())
                    {
                        NewTsObjMapping(req.Coupon, newFormHeader.Id, Common.TsItemCategory.Coupon, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Fees.Any())
                    {
                        NewTsObjMapping(req.Fees, newFormHeader.Id, Common.TsItemCategory.Fees, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Mtm.Any())
                    {
                        NewTsObjMapping(req.Mtm, newFormHeader.Id, Common.TsItemCategory.Mtm, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.FxSettlement.Any())
                    {
                        NewTsObjMapping(req.FxSettlement, newFormHeader.Id, Common.TsItemCategory.Fx, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.ContributionCredited.Any())
                    {
                        NewTsObjMapping(req.ContributionCredited, newFormHeader.Id, Common.TsItemCategory.Cn, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Altid.Any())
                    {
                        NewTsObjMapping(req.Altid, newFormHeader.Id, Common.TsItemCategory.Altid, req.Currency, currentUser, ref newTrades);
                    }

                    if (req.Others.Any())
                    {
                        NewTsObjMapping(req.Others, newFormHeader.Id, Common.TsItemCategory.Others, req.Currency, currentUser, ref newTrades);
                    }

                    db.ISSD_TradeSettlement.AddRange(newTrades);
                    db.SaveChanges();

                    AuditService.Capture_FA(newFormHeader.Id, newFormHeader.FormType, FormActionType.Create, currentUser, $"Created an {newFormHeader.FormType} form");

                    if (req.Approver != null)
                    {
                        FormService.NotifyApprover(req.Approver, newFormHeader.Id, currentUser, newFormHeader.FormType, req.ApprovalNotes);
                        AuditService.Capture_FA(newFormHeader.Id, newFormHeader.FormType, FormActionType.RequestApproval, currentUser, $"Request Approval for {newFormHeader.FormType} form");
                    }

                    return newFormHeader.Id;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return 0;
            }
        }

        public static int EditForm(TsCreateNewFormRequest req, string currentUser)
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

                    if (req.Approver != null)
                    {
                        AuditService.FA_ReassignApprover(form.Id, form.FormType, form.SettlementDate, currentUser,
                            form.ApprovedBy, req.Approver);

                        form.ApprovedBy = req.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = Common.FormStatus.PendingApproval;
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Equity - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "Equity - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "Equity - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Equity - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Equity - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Equity, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "BOND - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "BOND - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "BOND - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "BOND - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "BOND - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.BondType != item.BondType)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.BondType, item.BondType, "BOND - Bond Type");

                                    foundItem.BondType = item.BondType;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Bond, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "CP - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "CP - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "CP - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "CP - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "CP - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Cp, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Notes & Paper - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "Notes & Paper - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.Maturity != item.Maturity)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Maturity.ToString(), item.Maturity.ToString(), "Notes & Paper - Maturity (+)");

                                    foundItem.Maturity = item.Maturity;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Notes & Paper - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Notes & Paper - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.NotesPapers, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "REPO - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "REPO - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.FirstLeg != item.FirstLeg)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.FirstLeg.ToString(), item.FirstLeg.ToString(), "REPO - First Leg");

                                    foundItem.FirstLeg = item.FirstLeg;
                                }

                                if (foundItem.SecondLeg != item.SecondLeg)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.SecondLeg.ToString(), item.SecondLeg.ToString(), "REPO - Second Leg");

                                    foundItem.SecondLeg = item.SecondLeg;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Repo, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Coupon - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.StockCode != item.StockCode)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.StockCode, item.StockCode, "Coupon - Stock Code");

                                    foundItem.StockCode = item.StockCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Coupon - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Coupon - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                if (foundItem.CouponType != item.CouponType)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Coupon, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "MTM - Payment/Receipt");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "MTM - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "MTM - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Mtm, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "FX - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "FX - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "FX - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Fx, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "ALTID - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "ALTID - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "ALTID - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Altid, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Contribution - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Contribution - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Cn, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Fees - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Fees - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Fees - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Fees, req.Currency, currentUser));

                                AuditService.FA_AddRow(form.Id, form.FormType,
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
                                    AuditService.FA_RemoveRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.InstrumentCode, item.InstrumentCode, "Others - Instrument Code");

                                    foundItem.InstrumentCode = item.InstrumentCode;
                                }

                                if (foundItem.AmountPlus != item.AmountPlus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountPlus.ToString(), item.AmountPlus.ToString(), "Others - Amount (+)");

                                    foundItem.AmountPlus = item.AmountPlus;
                                }

                                if (foundItem.AmountMinus != item.AmountMinus)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.AmountMinus.ToString(), item.AmountMinus.ToString(), "Others - Amount (-)");

                                    foundItem.AmountMinus = item.AmountMinus;
                                }

                                if (foundItem.Remarks != item.Remarks)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
                                        foundItem.Remarks, item.Remarks, "Others - Remarks");

                                    foundItem.Remarks = item.Remarks;
                                }

                                if (foundItem.OthersType != item.OthersType)
                                {
                                    AuditService.FA_EditRow(form.Id, form.FormType, form.SettlementDate, currentUser,
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
                                db.ISSD_TradeSettlement.Add(NewTsObjMapping(item, req.Id, Common.TsItemCategory.Others, req.Currency, currentUser));
                            }
                        }
                    }

                    db.SaveChanges();

                    if (req.Approver != null && !req.IsSaveAsDraft && !req.IsSaveAdminEdit)
                    {
                        FormService.NotifyApprover(form.ApprovedBy, form.Id, currentUser, form.FormType, req.ApprovalNotes);
                    }
                    
                    return form.Id;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return 0;
            }
        }

        public static int FormApproval(TsFormApprovalRequest req, string currentUser)
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
                            form.FormStatus = (req.ApprovalStatus) ? Common.FormStatus.Approved : Common.FormStatus.Rejected;
                            db.SaveChanges();

                            FormService.NotifyPreparer(form.Id, form.FormType, form.FormStatus, form.PreparedBy, form.ApprovedBy, req.ApprovalNote);
                            new MailService().TS_IncomingFund(form.Id, form.FormType, form.Currency);
                            AuditService.FA_Approval(form.Id, form.FormType, form.FormStatus, form.SettlementDate, currentUser);

                            /*if (WorkflowService.FormResubmissionFromApprovedRejected(form.Id, form.FormType))
                            {

                            }*/

                            if (form.FormType == Common.FormType.ISSD_TS_E && form.FormStatus == Common.FormStatus.Approved)
                            {
                                new MailService().TS_PartE_NotifyPe(form.Id);
                            }

                            if (form.FormType == Common.FormType.ISSD_TS_H && form.FormStatus == Common.FormStatus.Approved)
                            {
                                var tsLoanItemExist = db.ISSD_TradeSettlement.Any(x => x.FormId == form.Id && x.OthersType == Common.TsOthersTypeItem.Loan);
                                if (tsLoanItemExist)
                                {
                                    new MailService().TS_PartH_Notify(form.Id, Common.TsOthersTypeItem.Loan);
                                }

                            }

                            return formId;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return 0;
            }
        }

        public static bool DeleteForm(int formId, string currentUser)
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

                        db.SaveChanges();

                        AuditService.Capture_FA(form.Id, form.FormType, FormActionType.Delete, currentUser, $"Deleted {form.FormType} form");

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        #endregion


        public static List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, int formId)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => x.FormId == formId)
                .ToList();

            return result;
        }

        public static List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, DateTime settlementDate, string currency)
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

        public static List<TsOpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var result = new List<TsOpeningBalance>();

            var ob = db.EDW_BankBalance
                .AsNoTracking()
                .Where(x =>
                    DbFunctions.TruncateTime(x.SettlementDate) ==
                    DbFunctions.TruncateTime(settlementDate) && x.Currency == currency)
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

        public static TS_TotalFlow GetTotalFlow(kashflowDBEntities db, List<int> formId, DateTime settlementDate, string currency)
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

        public static bool EditFormRules(string formStatus, string approvedBy, string currentUser, out string errorMessage)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isYouAreTheApprover = approvedBy == currentUser;

            if (isPendingApproval)
            {
                errorMessage = "Form is still in Pending Approval status";
                return true;
            }
            else if (isYouAreTheApprover)
            {
                errorMessage = "You are this form Approver. Then you cannot edit it";
                return true;
            }
            else
            {
                errorMessage = null;
                return false;
            }
        }

        public static double GetTotalInflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category)
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
                .Select(x => (x.Sales + x.Maturity + x.AmountPlus + x.FirstLeg))
                .FirstOrDefault();

            return result;
        }
        
        public static double GetTotalOutflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category)
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
                .Select(x => (x.AmountMinus + x.Purchase + x.SecondLeg))
                .FirstOrDefault();

            return result;
        }

        public static bool RetractFormSubmission(int formId, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        form.FormStatus = Common.FormStatus.Draft;
                        db.SaveChanges();

                        AuditService.Capture_FA(form.Id, form.FormType, Common.FormActionType.RetractSubmission, performedBy, $"Retract form submission for {form.FormType} form.");

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return false;
            }
        }

        #region Private Functions

        private static void NewTsObjMapping(List<TsTradeItem> trades, int formId, string category, string currency, string currentUser, ref List<ISSD_TradeSettlement> tsItemObj)
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

                    InflowAmount = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg),
                    OutflowAmount = (item.AmountMinus + item.Purchase + item.SecondLeg),

                    InflowTo = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                    OutflowFrom = (item.AmountMinus + item.Purchase + item.SecondLeg) > 0 && currency == "MYR"
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

        private static ISSD_TradeSettlement NewTsObjMapping(TsTradeItem item, int formId, string category, string currency, string currentUser)
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

                InflowAmount = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg),
                OutflowAmount = (item.AmountMinus + item.Purchase + item.SecondLeg),

                InflowTo = (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                OutflowFrom = (item.AmountMinus + item.Purchase + item.SecondLeg) > 0 && currency == "MYR"
                        ? "RENTAS"
                        : null,
                AssignedBy = null,
                AssignedDate = null,

                OthersType = item.OthersType,
                CouponType = item.CouponType,
                BondType = item.BondType
            };
        }

        private static double SumInflowAmount(TsTradeItem item)
        {
            return (item.AmountPlus + item.Sales + item.Maturity + item.FirstLeg);
        }

        private static double SumOutflowAmount(TsTradeItem item)
        {
            return (item.AmountMinus + item.Purchase + item.SecondLeg);
        }

        #endregion

    }
}
