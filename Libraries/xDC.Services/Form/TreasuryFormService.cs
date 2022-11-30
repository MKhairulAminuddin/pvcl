using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using xDC.Domain.Web.FID.TreasuryForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Audit;
using xDC.Services.FileGenerator;
using xDC.Services.Notification;
using xDC.Services.Workflow;
using static xDC.Utils.Common;

namespace xDC.Services.Form
{
    public class TreasuryFormService : FormService, ITreasuryFormService
    {
        #region Fields

        private readonly IXDcLogger _logger;
        private readonly IGenFile_TreasuryForm _genFile;
        private readonly IWorkflowService _wfService;

        #endregion

        #region Ctor

        public TreasuryFormService(Workflow.IWorkflowService wfService, INotificationService notifyService, IXDcLogger logger, IAuditService auditService, IGenFile_TreasuryForm genFile)
            : base(wfService, notifyService, logger, auditService)
        {
            _logger = logger;
            _genFile = genFile;
            _wfService = wfService;
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

        #endregion


        public string GenExportFormId(int formId, string currentUser, bool isExportToExcel)
        {
            return _genFile.GenId_TreasuryForm(formId, currentUser, isExportToExcel);
        }

        public FileStream GetGeneratedForm(string generatedFileId)
        {
            return _genFile.GenFile(generatedFileId);
        }


        public List<EDW_FID_List> List_Issuer(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var issuerList = db.EDW_FID_List.Where(x => x.Type == "ISSUER").ToList();
            return issuerList;
        }

        public List<string> List_FcaBankAccount(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var fcaBankAccounts = db.Config_FcaBankAccount.Select(x => x.AccountName3).ToList();
            return fcaBankAccounts;
        }

        public List<EDW_FID_List> List_CounterParty(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var counterPartyList = db.EDW_FID_List.Where(x => x.Type == "COUNTERPARTY").ToList();
            return counterPartyList;
        }

    }
}
