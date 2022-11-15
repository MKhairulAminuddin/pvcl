using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Domain.Web.FID.TreasuryForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.App
{
    public static class TreasuryFormService
    {
        #region Grid

        public static List<TreasuryHomeGrid1> GetTsHomeGrid1(string currentUser)
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

                            EnableEdit = FormService.EnableEdit(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnableDelete = FormService.EnableDelete(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnablePrint = FormService.EnablePrint(currentUser, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Download),
                            EnableRetractSubmission = FormService.EnableRetractSubmission(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.FID_TreasuryForm_Edit),

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

        #endregion

        #region Form Page

        public static TreasuryFormPage GetViewPageData(int formId, string currentUser)
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
                            ApprovalNotes = WorkflowService.GetApprovalNotes(form.Id, form.FormType),

                            EnableApproveRejectBtn = FormService.EnableApprovalAction(currentUser, form.ApprovedBy, form.FormStatus, db),
                            EnableReassign = FormService.EnableReassignApprover(currentUser, form.ApprovedBy, form.PreparedBy, form.FormStatus),
                            EnableEditDraftBtn = FormService.EnableEdit(form.FormStatus, form.PreparedBy, form.ApprovedBy, currentUser)
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
                Logger.LogError(ex);
                return null;
            }
        }

        public static TreasuryFormPage GetEditPageData(int formId, string currentUser)
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

                            EnableSubmitForApproval = FormService.EnableSubmitForApproval(form.ApprovedBy, form.FormStatus),
                            EnableResubmitBtn = FormService.EnableFormResubmission(currentUser, form.FormStatus, form.PreparedBy),
                            EnableSaveAsDraftBtn = FormService.EnableSaveAsDraftBtn(currentUser, form.FormStatus, form.PreparedBy, form.ApprovedBy),
                            EnableReassign = FormService.EnableReassignApprover(currentUser, form.ApprovedBy, form.PreparedBy, form.FormStatus)
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
                Logger.LogError(ex);
                return null;
            }
        }

        #endregion

        public static List<TreasuryFormSummary> TreasuryFormSummaryList(long submissionDateEpoch = 0)
        {
            try
            {
                DateTime selectedDate;
                if (submissionDateEpoch != 0)
                {
                    selectedDate = Utils.Common.ConvertEpochToDateTime(submissionDateEpoch).Value;
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
                Logger.LogError(ex);
                return null;
            }
        }
        public static List<EDW_FID_List> List_Issuer(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var issuerList = db.EDW_FID_List.Where(x => x.Type == "ISSUER").ToList();
            return issuerList;
        }

        public static List<string> List_FcaBankAccount(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var fcaBankAccounts = db.Config_FcaBankAccount.Select(x => x.AccountName3).ToList();
            return fcaBankAccounts;
        }

        public static List<EDW_FID_List> List_CounterParty(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var counterPartyList = db.EDW_FID_List.Where(x => x.Type == "COUNTERPARTY").ToList();
            return counterPartyList;
        }

    }
}
