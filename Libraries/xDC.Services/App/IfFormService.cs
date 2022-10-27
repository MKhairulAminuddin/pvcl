using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.App
{
    public static class IfFormService
    {
        #region Landing Page

        public static InflowFundFormsLandingPage GetLandingPageData(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var enableCreateForm = new AuthService().IsUserHaveAccess(currentUser, Common.PermissionKey.AMSD_InflowFundForm_Edit);
                    var today = DateTime.Now;

                    var model = new InflowFundFormsLandingPage()
                    {
                        CountTodaySubmission = db.AMSD_IF.Count(x => x.FormType == Common.FormType.AMSD_IF
                                            && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                            && x.FormStatus != Common.FormStatus.Draft),
                        CountTodayPendingApproval = db.AMSD_IF
                                                        .Count(x => x.FormType == Common.FormType.AMSD_IF
                                                                && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == Common.FormStatus.PendingApproval),
                        CountTodayApproved = db.AMSD_IF
                                                        .Count(x => x.FormType == Common.FormType.AMSD_IF
                                                                && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
                                                                && x.FormStatus == Common.FormStatus.Approved),
                        CountTodayRejected = db.AMSD_IF
                                                        .Count(x => x.FormType == Common.FormType.AMSD_IF
                                                                && DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(today)
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

        public static InflowFundForm GetPageViewData(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var isApprovedOrRejected = (form.FormStatus == Common.FormStatus.Approved || form.FormStatus == Common.FormStatus.Rejected);
                        var latestWorkflow = db.Form_Workflow
                            .Where(x => x.Id == form.Id && x.FormType == form.FormType && (x.WorkflowStatus.Contains(Common.FormStatus.Approved) || x.WorkflowStatus.Contains(Common.FormStatus.Rejected)))
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

                        var formView = new InflowFundForm()
                        {
                            Id = form.Id,
                            FormStatus = form.FormStatus,

                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,

                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,
                            ApprovalNote = latestWorkflow?.WorkflowNotes,

                            IsAdminEdited = form.AdminEditted,
                            AdminEditedBy = form.AdminEdittedBy,
                            AdminEditedDate = form.AdminEdittedDate,

                            IsApproved = (form.FormStatus == Common.FormStatus.Approved),

                            EnableReassign = (form.FormStatus == Common.FormStatus.PendingApproval && form.ApprovedBy != currentUser),
                            EnableResubmit = (form.FormStatus == Common.FormStatus.Approved || form.FormStatus == Common.FormStatus.Rejected)
                                             && (form.ApprovedBy != currentUser),
                            EnableSubmitForApproval = (form.FormStatus == Common.FormStatus.Draft || form.FormStatus == Common.FormStatus.Draft),
                            EnableDraftButton = (form.FormStatus == Common.FormStatus.Draft),
                            EnableEditDraftBtn = (form.FormStatus == Common.FormStatus.Draft && form.PreparedBy == currentUser),
                            EnableSaveAdminChanges = new AuthService().IsUserHaveAccess(currentUser, Common.PermissionKey.AMSD_InflowFundForm_PowerUser) && (form.FormStatus == Common.FormStatus.Approved),
                            EnableApproveRejectBtn = (form.ApprovedBy == currentUser && form.FormStatus == Common.FormStatus.PendingApproval)
                        };

                        return formView;
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

        #region Grid

        public static List<IfHomeGrid1> GetTsHomeGrid1(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todayDate = DateTime.Now.Date;

                    var tsForms = db.AMSD_IF;
                    var resultVM = new List<IfHomeGrid1>();

                    foreach (var item in tsForms)
                    {
                        resultVM.Add(new IfHomeGrid1
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormDate = item.FormDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = FormService.EnableEdit(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnableDelete = FormService.EnableDelete(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnablePrint = FormService.EnablePrint(currentUser, item.FormStatus, PermissionKey.AMSD_InflowFundForm_Download),
                            EnableRetractSubmission = FormService.EnableRetractSubmission(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.AMSD_InflowFundForm_Edit),

                            IsRejected = (currentUser == item.PreparedBy && item.FormStatus == FormStatus.Rejected),
                            IsPendingMyApproval = (currentUser == item.ApprovedBy && item.FormStatus == FormStatus.PendingApproval),
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

    }
}
