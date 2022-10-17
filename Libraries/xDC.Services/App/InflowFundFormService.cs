using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        public static bool EnablePrint(string formStatus)
        {
            var isDraftForm = formStatus == Common.FormStatus.Draft;

            return !isDraftForm;
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

        public static InflowFundForm View(int formId, string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        var isApprovedOrRejected = (form.FormStatus == Common.FormStatus.Approved ||
                                                    form.FormStatus == Common.FormStatus.Rejected);
                        var getFormWorkflow = db.Form_Workflow
                            .Where(x => (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected) &&
                                        x.FormId == form.Id).OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

                        var formView = new InflowFundForm()
                        {
                            Id = form.Id,
                            FormStatus = form.FormStatus,

                            PreparedBy = form.PreparedBy,
                            PreparedDate = form.PreparedDate,

                            ApprovedBy = form.ApprovedBy,
                            ApprovedDate = form.ApprovedDate,
                            ApprovalNote = getFormWorkflow?.WorkflowNotes,

                            IsAdminEdited = form.AdminEditted,
                            AdminEditedBy = form.AdminEdittedBy,
                            AdminEditedDate = form.AdminEdittedDate,

                            EnableReassign = (form.FormStatus == Common.FormStatus.PendingApproval && form.ApprovedBy != currentUser.Identity.Name),
                            IsApproved = (form.FormStatus == Common.FormStatus.Approved),
                            EnableResubmit = (form.FormStatus == Common.FormStatus.Approved || form.FormStatus == Common.FormStatus.Rejected)
                                             && (!User.IsInRole(Config.Acl.PowerUser)
                                                 && (form.ApprovedBy != User.Identity.Name)),
                            EnableSubmitForApproval = (form.FormStatus == Common.FormStatus.Draft || form.FormStatus == Common.FormStatus.Draft)
                                                      && (!User.IsInRole(Config.Acl.PowerUser)),
                            EnableDraftButton = (form.FormStatus == Common.FormStatus.Draft) && (!currentUser.IsInRole(Config.Acl.PowerUser)),
                            EnableSaveAdminChanges = User.IsInRole(Config.Acl.PowerUser) && (form.FormStatus == Common.FormStatus.Approved),
                            EnableApproveRejectBtn = (User.IsInRole(Config.Acl.Amsd) && form.ApprovedBy == currentUser.Identity.Name && form.FormStatus == Common.FormStatus.PendingApproval)
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
            }
        }
    }
}
