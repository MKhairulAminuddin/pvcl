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
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class InflowFundFormService
    {
        public static bool EnableEdit(string formStatus, string formApprover, string currentUser)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isFormApprover = formApprover == currentUser;

            return !isPendingApproval || !isFormApprover;
        }

        public static bool EnableDelete(string formStatus)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;

            return !isPendingApproval;
        }

        public static bool EnablePrint(string currentUser, string formStatus)
        {
            var isAllowedToDownload = new AuthService().IsUserHaveAccess(currentUser, Common.PermissionKey.AMSD_InflowFundForm_Download);
            var isDraft = formStatus == Common.FormStatus.Draft;

            return isAllowedToDownload && !isDraft;
        }

        public static bool RetractFormSubmission(int formId, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        form.FormStatus = Common.FormStatus.Draft;
                        db.SaveChanges();

                        new AuditService().Capture_FA(form.Id, form.FormType, Common.FormActionType.RetractSubmission, performedBy, $"Retract form submission for {form.FormType} form.");

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
    }
}
