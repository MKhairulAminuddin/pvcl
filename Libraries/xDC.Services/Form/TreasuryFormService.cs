using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using xDC.Domain.Web.Application;
using xDC.Domain.Web.FID.TreasuryForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms;
using xDC.Domain.WebApi.Forms.InflowFund;
using xDC.Domain.WebApi.Forms.Treasury;
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
    public class TreasuryFormService : FormService, ITreasuryFormService
    {
        #region Fields

        private readonly IXDcLogger _logger;
        private readonly IGenFile_TreasuryForm _genFile;
        private readonly IWorkflowService _wfService;
        private readonly IAuditService _auditService;

        #endregion

        #region Ctor

        public TreasuryFormService(IWorkflowService wfService, INotificationService notifyService, IXDcLogger logger, IAuditService auditService, IGenFile_TreasuryForm genFile, IRoleManagementService roleManagement)
            : base(wfService, notifyService, logger, auditService, roleManagement)
        {
            _logger = logger;
            _genFile = genFile;
            _wfService = wfService;
            _auditService = auditService;
        }

        #endregion

        #region Read Form Data

        public List<TreasuryHomeGrid1> GetTsHomeGrid1(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todayDate = DateTime.Now.Date;

                    var tsForms = db.FID_Treasury;
                    var resultVM = new List<TreasuryHomeGrid1>();

                    foreach (var item in tsForms)
                    {
                        resultVM.Add(new TreasuryHomeGrid1
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            ValueDate = item.ValueDate,
                            Currency = item.Currency,
                            FormStatus = item.FormStatus,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = this.EnableEdit(item.FormStatus, currentUser, PermissionKey.FID_TreasuryForm_Edit),
                            EnableDelete = this.EnableDelete(item.FormStatus, item.ApprovedBy, currentUser, PermissionKey.FID_TreasuryForm_Edit),
                            EnablePrint = this.EnablePrint(currentUser, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Download),
                            EnableRetractSubmission = this.EnableFormWithdrawal(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.FID_TreasuryForm_Edit),

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

        public List<TreasuryFormSummary> TreasuryFormSummaryList(long submissionDateEpoch = 0)
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
                    var tsForms = db.FID_Treasury.Where(x => DbFunctions.TruncateTime(x.PreparedDate) == DbFunctions.TruncateTime(selectedDate))
                        .Select(x => new TreasuryFormSummary()
                        {
                            FormId = x.Id,
                            Currency = x.Currency,
                            PreparedBy = x.PreparedBy,
                            SubmittedDate = x.PreparedDate,
                            ApprovedBy = x.ApprovedBy,
                            ApprovedDate = x.ApprovedDate,
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

        public TreasuryFormPage GetViewPageData(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var formModel = new TreasuryFormPage()
                        {
                            Id = form.Id,
                            FormStatus = form.FormStatus,
                            ValueDate = form.ValueDate,
                            Currency = form.Currency,
                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,
                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,
                            ApprovalNotes = _wfService.LatestApprovalNotes(form.Id, form.FormType),

                            EnableApproveRejectBtn = this.EnableFormApproval(currentUser, form.ApprovedBy, form.FormStatus, db),
                            EnableReassign = this.EnableReassignApprover(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.FID_TreasuryForm_Edit),
                            EnableEditDraftBtn = this.EnableEdit(form.FormStatus, currentUser, PermissionKey.FID_TreasuryForm_Edit)
                        };

                        return formModel;
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

        public TreasuryFormPage GetEditPageData(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var model = new TreasuryFormPage
                        {
                            Currency = form.Currency,
                            ValueDate = form.ValueDate,
                            FormStatus = form.FormStatus,
                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,
                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,

                            EnableSubmitForApproval = this.EnableApprovalSubmission(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.FID_TreasuryForm_Edit),
                            EnableResubmitBtn = this.EnableResubmission(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.FID_TreasuryForm_Edit),
                            EnableSaveAsDraftBtn = this.EnableSaveAsDraft(currentUser, form.FormStatus, form.PreparedBy, form.ApprovedBy),
                            EnableReassign = this.EnableReassignApprover(form.FormStatus, form.ApprovedBy, currentUser, PermissionKey.FID_TreasuryForm_Edit)
                        };
                        return model;
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

        public List<FID_Treasury_Deposit> GetDepositGrid(int formId, Cashflow cashflow)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury_Deposit
                        .Where(x => x.CashflowType == cashflow.ToString() && x.FormId == formId)
                    .ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<FID_Treasury_MMI> GetMmiGrid(int formId, Cashflow cashflow)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.FID_Treasury_MMI
                        .Where(x => x.CashflowType == cashflow.ToString() && x.FormId == formId)
                    .ToList();

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

        #region Form Action

        public int CreateForm(TForm input, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var valueDateConverted = Common.ConvertEpochToDateTime(input.ValueDate);
                    valueDateConverted = valueDateConverted.Value.Date;

                    var form = new FID_Treasury
                    {
                        FormType = Common.FormType.FID_TREASURY,
                        FormStatus = !string.IsNullOrEmpty(input.Approver) ? Common.FormStatus.PendingApproval : Common.FormStatus.Draft,
                        Currency = input.Currency,
                        ValueDate = valueDateConverted,
                        PreparedBy = currentUser,
                        PreparedDate = DateTime.Now,

                        ApprovedBy = !string.IsNullOrEmpty(input.Approver) ? input.Approver : null
                    };
                    db.FID_Treasury.Add(form);
                    var createFormStatus = db.SaveChanges();
                    if (createFormStatus < 1) return 0;



                    var inflowDeposit = new List<FID_Treasury_Deposit>();
                    if (input.InflowDeposit.Any())
                    {
                        foreach (var item in input.InflowDeposit)
                        {
                            inflowDeposit.Add(new FID_Treasury_Deposit
                            {
                                FormId = form.Id,
                                CashflowType = Cashflow.INFLOW.ToString(),
                                Dealer = item.Dealer,
                                Bank = item.Bank,
                                TradeDate = item.TradeDate,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                Principal = item.Principal,
                                Tenor = item.Tenor,
                                RatePercent = item.RatePercent,
                                IntProfitReceivable = item.IntProfitReceivable,
                                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I,
                                AssetType = item.AssetType,
                                RepoTag = item.RepoTag,
                                ContactPerson = item.ContactPerson,
                                Notes = item.Notes,
                                FcaAccount = item.FcaAccount,
                                ModifiedBy = currentUser,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_Deposit.AddRange(inflowDeposit);

                    var outflowDeposit = new List<FID_Treasury_Deposit>();
                    if (input.OutflowDeposit.Any())
                    {
                        foreach (var item in input.OutflowDeposit)
                        {
                            outflowDeposit.Add(new FID_Treasury_Deposit
                            {
                                FormId = form.Id,
                                CashflowType = Cashflow.OUTFLOW.ToString(),
                                Dealer = item.Dealer,
                                Bank = item.Bank,
                                TradeDate = item.TradeDate,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                Tenor = item.Tenor,
                                Principal = item.Principal,
                                RatePercent = item.RatePercent,
                                IntProfitReceivable = item.IntProfitReceivable,
                                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                                ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I,
                                AssetType = item.AssetType,
                                RepoTag = item.RepoTag,
                                ContactPerson = item.ContactPerson,
                                Notes = item.Notes,
                                FcaAccount = item.FcaAccount,
                                ModifiedBy = currentUser,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_Deposit.AddRange(outflowDeposit);

                    var inflowMoneyMarket = new List<FID_Treasury_MMI>();
                    if (input.InflowMoneyMarket.Any())
                    {
                        foreach (var item in input.InflowMoneyMarket)
                        {
                            inflowMoneyMarket.Add(new FID_Treasury_MMI
                            {
                                FormId = form.Id,
                                CashflowType = Cashflow.INFLOW.ToString(),
                                Dealer = item.Dealer,
                                Issuer = item.Issuer,
                                ProductType = item.ProductType,
                                CounterParty = item.CounterParty,
                                TradeDate = item.TradeDate,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                HoldingDayTenor = item.HoldingDayTenor,
                                Nominal = item.Nominal,
                                SellPurchaseRateYield = item.SellPurchaseRateYield,
                                Price = item.Price,
                                IntDividendReceivable = item.IntDividendReceivable,
                                Proceeds = item.Proceeds,
                                PurchaseProceeds = item.PurchaseProceeds,
                                CertNoStockCode = item.CertNoStockCode,
                                FcaAccount = item.FcaAccount,
                                ModifiedBy = currentUser,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_MMI.AddRange(inflowMoneyMarket);

                    var outflowMoneyMarket = new List<FID_Treasury_MMI>();
                    if (input.OutflowMoneyMarket.Any())
                    {
                        foreach (var item in input.OutflowMoneyMarket)
                        {
                            outflowMoneyMarket.Add(new FID_Treasury_MMI
                            {
                                FormId = form.Id,
                                CashflowType = Cashflow.OUTFLOW.ToString(),
                                Dealer = item.Dealer,
                                Issuer = item.Issuer,
                                ProductType = item.ProductType,
                                CounterParty = item.CounterParty,
                                TradeDate = item.TradeDate,
                                ValueDate = item.ValueDate,
                                MaturityDate = item.MaturityDate,
                                HoldingDayTenor = item.HoldingDayTenor,
                                Nominal = item.Nominal,
                                SellPurchaseRateYield = item.SellPurchaseRateYield,
                                Price = item.Price,
                                IntDividendReceivable = item.IntDividendReceivable,
                                Proceeds = item.Proceeds,
                                PurchaseProceeds = item.PurchaseProceeds,
                                CertNoStockCode = item.CertNoStockCode,
                                FcaAccount = item.FcaAccount,
                                ModifiedBy = currentUser,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    db.FID_Treasury_MMI.AddRange(outflowMoneyMarket);

                    var createFormItemsStatus = db.SaveChanges();
                    if (createFormItemsStatus < 1) return 0;


                    _auditService.FA_Add(form.Id, form.FormType, form.ValueDate, FormActionType.Create, currentUser, $"Created an {form.FormType} form");

                    if (form.FormStatus == FormStatus.PendingApproval)
                    {
                        Create(form.Id, form.FormType, form.ValueDate, form.PreparedBy, form.ApprovedBy, input.ApprovalNotes);
                    }

                    return form.Id;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return 0;
            }
        }

        public int EditForm(int formId, TForm input, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return 0;

                    form.PreparedBy = currentUser;
                    form.PreparedDate = DateTime.Now;

                    if (input.Approver != null)
                    {
                        form.ApprovedBy = input.Approver;
                        form.ApprovedDate = null; // empty the date as this is new submission
                        form.FormStatus = FormStatus.PendingApproval;
                    }

                    if (input.InflowDeposit.Any())
                    {
                        var inflowDepositInGrid = input.InflowDeposit;
                        var existingInflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.INFLOW.ToString());

                        // delete from existing
                        var itemExistInGrid =
                            inflowDepositInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingInflowDeposit.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(removedItems);
                        }

                        foreach (var item in inflowDepositInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingInflowDeposit.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.AssetType != item.AssetType)
                                    {
                                        foundItem.AssetType = item.AssetType;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.AssetType,
                                                item.AssetType, "Asset Type");
                                    }
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.Dealer,
                                                item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Bank != item.Bank)
                                    {
                                        foundItem.Bank = item.Bank;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.Bank,
                                                item.Bank, "Bank");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                                item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                                item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.Tenor != item.Tenor)
                                    {
                                        foundItem.Tenor = item.Tenor;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.Tenor.ToString(),
                                                item.Tenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Principal != item.Principal)
                                    {
                                        foundItem.Principal = item.Principal;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.Principal.ToString(),
                                                item.Principal.ToString(), "Principal");
                                    }
                                    if (foundItem.RatePercent != item.RatePercent)
                                    {
                                        foundItem.RatePercent = item.RatePercent;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.RatePercent.ToString(),
                                                item.RatePercent.ToString(), "Rate");
                                    }
                                    if (foundItem.IntProfitReceivable != item.IntProfitReceivable)
                                    {
                                        foundItem.IntProfitReceivable = item.IntProfitReceivable;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.IntProfitReceivable.ToString(),
                                                item.IntProfitReceivable.ToString(), "Interest Profit Receivable");
                                    }
                                    if (foundItem.PrincipalIntProfitReceivable != item.PrincipalIntProfitReceivable)
                                    {
                                        foundItem.PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.PrincipalIntProfitReceivable.ToString(),
                                                item.PrincipalIntProfitReceivable.ToString(), "Principal + Interest Profit Receivable");
                                    }
                                    if (foundItem.ManualCalc_P_Plus_I != item.ManualCalc_P_Plus_I)
                                    {
                                        foundItem.ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.ManualCalc_P_Plus_I.ToString(),
                                                item.ManualCalc_P_Plus_I.ToString(), "Manual Calculation P + I");
                                    }
                                    if (foundItem.RepoTag != item.RepoTag)
                                    {
                                        foundItem.RepoTag = item.RepoTag;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.RepoTag,
                                                item.RepoTag, "Repo Tag");
                                    }
                                    if (foundItem.ContactPerson != item.ContactPerson)
                                    {
                                        foundItem.ContactPerson = item.ContactPerson;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.ContactPerson,
                                                item.ContactPerson, "Contact Person");
                                    }
                                    if (foundItem.Notes != item.Notes)
                                    {
                                        foundItem.Notes = item.Notes;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.Notes,
                                                item.Notes, "Notes");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.FcaAccount,
                                                item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = currentUser;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_Deposit.Add(FID_Treasury_Deposit_ObjMap(form.Id, Cashflow.INFLOW.ToString(), item, currentUser));
                                _auditService.FA_AddRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser,
                                                $"{item.Bank}, {item.CashflowType}, {item.Dealer}, {item.AssetType}...");
                            }
                        }
                    }
                    else
                    {
                        var existingInflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.INFLOW.ToString());

                        // delete from existing
                        if (existingInflowDeposit.Any())
                        {
                            foreach (var item in existingInflowDeposit)
                            {
                                _auditService.FA_RemoveRow(form.Id, form.FormType, form.ValueDate, currentUser,
                                        $"{item.CashflowType}, {item.Bank}, {item.Dealer}, {item.AssetType}...");
                            }

                            db.FID_Treasury_Deposit.RemoveRange(existingInflowDeposit);
                        }
                    }

                    if (input.OutflowDeposit.Any())
                    {
                        var outflowDepositInGrid = input.OutflowDeposit;
                        var existingOutflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.OUTFLOW.ToString());

                        // delete from existing
                        var itemExistInGrid =
                            outflowDepositInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingOutflowDeposit.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            foreach (var item in removedItems)
                            {
                                _auditService.FA_RemoveRow(form.Id, form.FormType, form.ValueDate, currentUser,
                                        $"{item.CashflowType}, {item.Bank}, {item.Dealer}, {item.AssetType}...");
                            }

                            db.FID_Treasury_Deposit.RemoveRange(removedItems);
                        }

                        foreach (var item in outflowDepositInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingOutflowDeposit.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.AssetType != item.AssetType)
                                    {
                                        foundItem.AssetType = item.AssetType;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.AssetType,
                                            item.AssetType, "Asset Type");
                                    }
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Dealer,
                                            item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Bank != item.Bank)
                                    {
                                        foundItem.Bank = item.Bank;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Bank,
                                            item.Bank, "Bank");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                            item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                            item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.Tenor != item.Tenor)
                                    {
                                        foundItem.Tenor = item.Tenor;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Tenor.ToString(),
                                            item.Tenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Principal != item.Principal)
                                    {
                                        foundItem.Principal = item.Principal;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Principal.ToString(),
                                            item.Principal.ToString(), "Principal");
                                    }
                                    if (foundItem.RatePercent != item.RatePercent)
                                    {
                                        foundItem.RatePercent = item.RatePercent;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.RatePercent.ToString(),
                                            item.RatePercent.ToString(), "Rate");
                                    }
                                    if (foundItem.IntProfitReceivable != item.IntProfitReceivable)
                                    {
                                        foundItem.IntProfitReceivable = item.IntProfitReceivable;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.IntProfitReceivable.ToString(),
                                            item.IntProfitReceivable.ToString(), "Interest Profit Receivable");
                                    }
                                    if (foundItem.PrincipalIntProfitReceivable != item.PrincipalIntProfitReceivable)
                                    {
                                        foundItem.PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.PrincipalIntProfitReceivable.ToString(),
                                            item.PrincipalIntProfitReceivable.ToString(), "Principal + Interest Profit Receivable");
                                    }
                                    if (foundItem.ManualCalc_P_Plus_I != item.ManualCalc_P_Plus_I)
                                    {
                                        foundItem.ManualCalc_P_Plus_I = item.ManualCalc_P_Plus_I;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                                form.ValueDate, currentUser, foundItem.ManualCalc_P_Plus_I.ToString(),
                                                item.ManualCalc_P_Plus_I.ToString(), "Manual Calculation P + I");
                                    }
                                    if (foundItem.RepoTag != item.RepoTag)
                                    {
                                        foundItem.RepoTag = item.RepoTag;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.RepoTag,
                                            item.RepoTag, "Repo Tag");
                                    }
                                    if (foundItem.ContactPerson != item.ContactPerson)
                                    {
                                        foundItem.ContactPerson = item.ContactPerson;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.ContactPerson,
                                            item.ContactPerson, "Contact Person");
                                    }
                                    if (foundItem.Notes != item.Notes)
                                    {
                                        foundItem.Notes = item.Notes;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Notes,
                                            item.Notes, "Notes");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.FcaAccount,
                                            item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = currentUser;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_Deposit.Add(FID_Treasury_Deposit_ObjMap(form.Id, Cashflow.OUTFLOW.ToString(), item, currentUser));
                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.ValueDate, currentUser,
                                    $"{item.Bank}, {item.CashflowType}, {item.Dealer}, {item.AssetType}...");
                            }
                        }
                    }
                    else
                    {
                        var existingOutflowDeposit = db.FID_Treasury_Deposit.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.OUTFLOW.ToString());

                        // delete from existing
                        if (existingOutflowDeposit.Any())
                        {
                            db.FID_Treasury_Deposit.RemoveRange(existingOutflowDeposit);
                        }
                    }

                    if (input.InflowMoneyMarket.Any())
                    {
                        var inflowMmiInGrid = input.InflowMoneyMarket;
                        var existingInflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.INFLOW.ToString());

                        // delete from existing
                        var itemExistInGrid =
                            inflowMmiInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingInflowMmi.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(removedItems);
                        }

                        foreach (var item in inflowMmiInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingInflowMmi.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Dealer,
                                            item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Issuer != item.Issuer)
                                    {
                                        foundItem.Issuer = item.Issuer;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Issuer,
                                            item.Issuer, "Issuer");
                                    }
                                    if (foundItem.ProductType != item.ProductType)
                                    {
                                        foundItem.ProductType = item.ProductType;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.ProductType,
                                            item.ProductType, "Product Type");
                                    }
                                    if (foundItem.CounterParty != item.CounterParty)
                                    {
                                        foundItem.CounterParty = item.CounterParty;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.CounterParty,
                                            item.CounterParty, "Counter Party");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                            item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                            item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.HoldingDayTenor != item.HoldingDayTenor)
                                    {
                                        foundItem.HoldingDayTenor = item.HoldingDayTenor;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.HoldingDayTenor.ToString(),
                                            item.HoldingDayTenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Nominal != item.Nominal)
                                    {
                                        foundItem.Nominal = item.Nominal;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Nominal.ToString(),
                                            item.Nominal.ToString(), "Nominal");
                                    }
                                    if (foundItem.SellPurchaseRateYield != item.SellPurchaseRateYield)
                                    {
                                        foundItem.SellPurchaseRateYield = item.SellPurchaseRateYield;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.SellPurchaseRateYield.ToString(),
                                            item.SellPurchaseRateYield.ToString(), "Sell Purchase Rate Yield");
                                    }
                                    if (foundItem.Price != item.Price)
                                    {
                                        foundItem.Price = item.Price;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Price.ToString(),
                                            item.Price.ToString(), "Price");
                                    }
                                    if (foundItem.IntDividendReceivable != item.IntDividendReceivable)
                                    {
                                        foundItem.IntDividendReceivable = item.IntDividendReceivable;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.IntDividendReceivable.ToString(),
                                            item.IntDividendReceivable.ToString(), "Interest Dividend Receivable");
                                    }
                                    if (foundItem.Proceeds != item.Proceeds)
                                    {
                                        foundItem.Proceeds = item.Proceeds;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Proceeds.ToString(),
                                            item.Proceeds.ToString(), "Proceeds");
                                    }
                                    if (foundItem.PurchaseProceeds != item.PurchaseProceeds)
                                    {
                                        foundItem.PurchaseProceeds = item.PurchaseProceeds;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.PurchaseProceeds.ToString(),
                                            item.PurchaseProceeds.ToString(), "Purchase Proceeds");
                                    }
                                    if (foundItem.CertNoStockCode != item.CertNoStockCode)
                                    {
                                        foundItem.CertNoStockCode = item.CertNoStockCode;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.CertNoStockCode,
                                            item.CertNoStockCode, "Cert No Stock Code");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.FcaAccount,
                                            item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = currentUser;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_MMI.Add(FID_Treasury_MMI_ObjMap(form.Id, Cashflow.INFLOW.ToString(), item, currentUser));
                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.ValueDate, currentUser,
                                    $"{item.Issuer}, {item.CashflowType}, {item.Dealer}, {item.CashflowType}...");
                            }
                        }
                    }
                    else
                    {
                        var existingInflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.INFLOW.ToString());

                        // delete from existing
                        if (existingInflowMmi.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(existingInflowMmi);
                        }
                    }

                    if (input.OutflowMoneyMarket.Any())
                    {
                        var outflowMmiInGrid = input.OutflowMoneyMarket;
                        var existingOutflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.OUTFLOW.ToString());

                        // delete from existing
                        var itemExistInGrid =
                            outflowMmiInGrid.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                        var removedItems =
                            existingOutflowMmi.Where(x => !itemExistInGrid.Contains(x.Id));
                        if (removedItems.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(removedItems);
                        }

                        foreach (var item in outflowMmiInGrid)
                        {
                            if (item.Id != 0)
                            {
                                // edit existing
                                var foundItem = existingOutflowMmi.FirstOrDefault(x => x.Id == item.Id);
                                if (foundItem != null)
                                {
                                    if (foundItem.Dealer != item.Dealer)
                                    {
                                        foundItem.Dealer = item.Dealer;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Dealer,
                                            item.Dealer, "Dealer");
                                    }
                                    if (foundItem.Issuer != item.Issuer)
                                    {
                                        foundItem.Issuer = item.Issuer;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Issuer,
                                            item.Issuer, "Issuer");
                                    }
                                    if (foundItem.ProductType != item.ProductType)
                                    {
                                        foundItem.ProductType = item.ProductType;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.ProductType,
                                            item.ProductType, "Product Type");
                                    }
                                    if (foundItem.CounterParty != item.CounterParty)
                                    {
                                        foundItem.CounterParty = item.CounterParty;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.CounterParty,
                                            item.CounterParty, "Counter Party");
                                    }
                                    if (foundItem.TradeDate != item.TradeDate)
                                    {
                                        foundItem.TradeDate = item.TradeDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.TradeDate?.ToString("dd/MM/yyyy"),
                                            item.TradeDate.ToString("dd/MM/yyyy"), "Trade Date");
                                    }
                                    if (foundItem.ValueDate != item.ValueDate)
                                    {
                                        foundItem.ValueDate = item.ValueDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.ValueDate?.ToString("dd/MM/yyyy"),
                                            item.ValueDate.ToString("dd/MM/yyyy"), "Value Date");
                                    }
                                    if (foundItem.MaturityDate != item.MaturityDate)
                                    {
                                        foundItem.MaturityDate = item.MaturityDate;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.MaturityDate?.ToString("dd/MM/yyyy"),
                                            item.MaturityDate.ToString("dd/MM/yyyy"), "Maturity Date");
                                    }
                                    if (foundItem.HoldingDayTenor != item.HoldingDayTenor)
                                    {
                                        foundItem.HoldingDayTenor = item.HoldingDayTenor;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.HoldingDayTenor.ToString(),
                                            item.HoldingDayTenor.ToString(), "Tenor");
                                    }
                                    if (foundItem.Nominal != item.Nominal)
                                    {
                                        foundItem.Nominal = item.Nominal;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Nominal.ToString(),
                                            item.Nominal.ToString(), "Nominal");
                                    }
                                    if (foundItem.SellPurchaseRateYield != item.SellPurchaseRateYield)
                                    {
                                        foundItem.SellPurchaseRateYield = item.SellPurchaseRateYield;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.SellPurchaseRateYield.ToString(),
                                            item.SellPurchaseRateYield.ToString(), "Sell Purchase Rate Yield");
                                    }
                                    if (foundItem.Price != item.Price)
                                    {
                                        foundItem.Price = item.Price;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Price.ToString(),
                                            item.Price.ToString(), "Price");
                                    }
                                    if (foundItem.IntDividendReceivable != item.IntDividendReceivable)
                                    {
                                        foundItem.IntDividendReceivable = item.IntDividendReceivable;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.IntDividendReceivable.ToString(),
                                            item.IntDividendReceivable.ToString(), "Interest Dividend Receivable");
                                    }
                                    if (foundItem.Proceeds != item.Proceeds)
                                    {
                                        foundItem.Proceeds = item.Proceeds;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.Proceeds.ToString(),
                                            item.Proceeds.ToString(), "Proceeds");
                                    }
                                    if (foundItem.PurchaseProceeds != item.PurchaseProceeds)
                                    {
                                        foundItem.PurchaseProceeds = item.PurchaseProceeds;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.PurchaseProceeds.ToString(),
                                            item.PurchaseProceeds.ToString(), "Purchase Proceeds");
                                    }
                                    if (foundItem.CertNoStockCode != item.CertNoStockCode)
                                    {
                                        foundItem.CertNoStockCode = item.CertNoStockCode;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.CertNoStockCode,
                                            item.CertNoStockCode, "Cert No Stock Code");
                                    }
                                    if (foundItem.FcaAccount != item.FcaAccount)
                                    {
                                        foundItem.FcaAccount = item.FcaAccount;
                                        _auditService.FA_EditRow(form.Id, form.FormType,
                                            form.ValueDate, currentUser, foundItem.FcaAccount,
                                            item.FcaAccount, "FCA Account");
                                    }

                                    foundItem.ModifiedBy = currentUser;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                // add new
                                db.FID_Treasury_MMI.Add(FID_Treasury_MMI_ObjMap(form.Id, Cashflow.OUTFLOW.ToString(), item, currentUser));
                                _auditService.FA_AddRow(form.Id, form.FormType,
                                    form.ValueDate, currentUser,
                                    $"{item.Issuer}, {item.CashflowType}, {item.Dealer}, {item.CashflowType}...");
                            }
                        }
                    }
                    else
                    {
                        var existingInflowMmi = db.FID_Treasury_MMI.Where(x =>
                            x.FormId == form.Id && x.CashflowType == Cashflow.OUTFLOW.ToString());

                        // delete from existing
                        if (existingInflowMmi.Any())
                        {
                            db.FID_Treasury_MMI.RemoveRange(existingInflowMmi);
                        }
                    }

                    var saveChangesStatus = db.SaveChanges();
                    if (saveChangesStatus < 1) return 0;
                    

                    if ((form.FormStatus == FormStatus.PendingApproval) && input.Approver != null)
                    {
                        Create(form.Id, form.FormType, form.ValueDate, form.PreparedBy, form.ApprovedBy, input.ApprovalNotes);
                    }

                    return form.Id;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return 0;
            }
        }

        public int DeleteForm(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId && x.FormStatus != Common.FormStatus.PendingApproval);
                    if (form == null) return 0;

                    db.FID_Treasury.Remove(form);

                    var depositInflowOutflow = db.FID_Treasury_Deposit.Where(x => x.FormId == form.Id);
                    if (depositInflowOutflow.Any())
                    {
                        db.FID_Treasury_Deposit.RemoveRange(depositInflowOutflow);
                    }

                    var mmiInflowOutflow = db.FID_Treasury_MMI.Where(x => x.FormId == form.Id);
                    if (mmiInflowOutflow.Any())
                    {
                        db.FID_Treasury_MMI.RemoveRange(mmiInflowOutflow);
                    }

                    var deleteFormStatus = db.SaveChanges();
                    if (deleteFormStatus < 1) return 0;

                    _auditService.FA_Add(form.Id, form.FormType, form.ValueDate, FormActionType.Delete, currentUser, $"Deleted a {form.FormType} form. (Form status at the moment of deletion is {form.FormStatus}).");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
        }

        public int ApproveForm(TreasuryFormApprovingReq input, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == input.FormId);
                    if (form == null) return 0;

                    form.ApprovedDate = DateTime.Now;
                    form.FormStatus = (input.ApprovalStatus) ? FormStatus.Approved : FormStatus.Rejected;

                    var saveFormApproval = db.SaveChanges();
                    if (saveFormApproval < 1) return 0;

                    ApprovalResponse(form.Id, form.FormType, form.ValueDate, form.PreparedBy, form.ApprovedBy, input.ApprovalNote, form.FormStatus);
                    return form.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
        }

        public bool WithdrawForm(int formId, string performedBy, string formType)
        {
            var withdraFormStatus = RetractFormSubmission(formId, performedBy, formType);
            return withdraFormStatus;
        }

        public bool ReassignApproverForm(int formId, string newApprover, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);
                    if (form == null) return false;

                    var permittedApprover = db.Config_Approver
                        .FirstOrDefault(x =>
                            x.FormType == form.FormType && x.Username != currentUser && x.Username == newApprover);
                    if (permittedApprover == null) return false;

                    var currentApprover = form.ApprovedBy;
                    form.ApprovedBy = newApprover;
                    form.ApprovedDate = null;

                    var reassignStatus = db.SaveChanges();
                    if (reassignStatus < 1) return false;

                    ReassignApprover(formId, form.FormType, form.ValueDate, currentUser, currentApprover, form.ApprovedBy);
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


        #region Generate/Export Document

        public string GenExportFormId(int formId, string currentUser, bool isExportToExcel)
        {
            return _genFile.GenId_TreasuryForm(formId, currentUser, isExportToExcel);
        }

        public ExportedFile GetGeneratedForm(string generatedFileId)
        {
            return _genFile.GenFile(generatedFileId);
        }

        #endregion

        #region EDW Data Fetch

        public List<EDW_FID_List> List_Issuer()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var issuerList = db.EDW_FID_List.Where(x => x.Type == "ISSUER").ToList();
                    return issuerList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<string> List_FcaBankAccount()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var fcaBankAccounts = db.Config_FcaBankAccount.Select(x => x.AccountName3).ToList();
                    return fcaBankAccounts;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<EDW_FID_List> List_CounterParty()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var counterPartyList = db.EDW_FID_List.Where(x => x.Type == "COUNTERPARTY").ToList();
                    return counterPartyList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<TDeposit> DepositFromEdw(DateTime tradeDate, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.EDW_Maturity_Deposit
                        .Where(x => DbFunctions.TruncateTime(x.MaturityDate) == DbFunctions.TruncateTime(tradeDate)
                                    && x.Currency == currency)
                        .Select(
                        x => new TDeposit
                        {
                            Dealer = x.Operator,
                            Bank = x.Bank,
                            TradeDate = DateTime.Now,
                            ValueDate = x.ValueDate,
                            MaturityDate = x.MaturityDate,
                            Principal = x.Principle,
                            Tenor = x.Tenor,
                            RatePercent = x.Rate,
                            IntProfitReceivable = 0,
                            PrincipalIntProfitReceivable = 0,
                            ManualCalc_P_Plus_I = false,
                            AssetType = x.AssetType,
                            RepoTag = null,
                            ContactPerson = null,
                            Notes = null
                        }).ToList();

                    foreach (var item in result)
                    {
                        var rate = (double)item.RatePercent / 100;
                        var tenorDays = (item.MaturityDate.Date - item.ValueDate.Date).TotalDays;
                        var tenor = (double)tenorDays / 365;

                        item.IntProfitReceivable = item.Principal * tenor * rate;
                        item.PrincipalIntProfitReceivable = item.Principal + item.IntProfitReceivable;
                        item.Id = GetRandomId(30, 99);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public List<TMmi> MmiFromEdw(DateTime tradeDate, string currency)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.EDW_Maturity_MM
                       .Where(x => DbFunctions.TruncateTime(x.ValueDate) == DbFunctions.TruncateTime(tradeDate)
                                   && x.Currency == currency)
                       .Select(x => new TMmi
                       {
                           CashflowType = Cashflow.INFLOW.ToString(),
                           Dealer = x.Operator,
                           Issuer = x.Issuer,
                           TradeDate = DateTime.Now,
                           ValueDate = x.ValueDate,
                           MaturityDate = x.MaturityDate,
                           HoldingDayTenor = x.Tenor,
                           CounterParty = x.Bank,
                           SellPurchaseRateYield = x.Rate,
                           Price = x.Price,
                           IntDividendReceivable = 0,
                           PurchaseProceeds = 0,
                           Proceeds = 0,
                           CertNoStockCode = x.StockCode,
                           ModifiedBy = null,
                           Nominal = x.Principle,
                           ProductType = x.AssetType
                       })
                       .ToList();

                    foreach (var item in result)
                    {
                        item.Id = GetRandomId(30, 99);
                    }

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

        #region Private Methods

        private FID_Treasury_Deposit FID_Treasury_Deposit_ObjMap(int formId, string cashFlow, TDeposit item, string currentUser)
        {
            return new FID_Treasury_Deposit
            {
                FormId = formId,
                CashflowType = cashFlow,
                Dealer = item.Dealer != null ? item.Dealer : currentUser,
                Bank = item.Bank,
                TradeDate = item.TradeDate,
                ValueDate = item.ValueDate,
                MaturityDate = item.MaturityDate,
                Tenor = item.Tenor,
                Principal = item.Principal,
                RatePercent = item.RatePercent,
                IntProfitReceivable = item.IntProfitReceivable,
                PrincipalIntProfitReceivable = item.PrincipalIntProfitReceivable,
                AssetType = item.AssetType,
                RepoTag = item.RepoTag,
                ContactPerson = item.ContactPerson,
                Notes = item.Notes,
                FcaAccount = item.FcaAccount,
                ModifiedBy = currentUser,
                ModifiedDate = DateTime.Now
            };
        }

        private FID_Treasury_MMI FID_Treasury_MMI_ObjMap(int formId, string cashFlow, TMmi item, string currentUser)
        {
            return new FID_Treasury_MMI()
            {
                FormId = formId,
                CashflowType = cashFlow,
                Dealer = item.Dealer != null ? item.Dealer : currentUser,
                Issuer = item.Issuer,
                ProductType = item.ProductType,
                CounterParty = item.CounterParty,
                TradeDate = item.TradeDate,
                ValueDate = item.ValueDate,
                MaturityDate = item.MaturityDate,
                HoldingDayTenor = item.HoldingDayTenor,
                Nominal = item.Nominal,
                SellPurchaseRateYield = item.SellPurchaseRateYield,
                Price = item.Price,
                IntDividendReceivable = item.IntDividendReceivable,
                Proceeds = item.Proceeds,
                PurchaseProceeds = item.PurchaseProceeds,
                CertNoStockCode = item.CertNoStockCode,
                FcaAccount = item.FcaAccount,
                ModifiedBy = currentUser,
                ModifiedDate = DateTime.Now
            };
        }

        #endregion
    }
}
