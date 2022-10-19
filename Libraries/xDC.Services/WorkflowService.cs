using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services
{
    public static class WorkflowService
    {
        public static void UpdateWorkflow(Form_Workflow input)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    db.Form_Workflow.Add(input);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static bool FormResubmissionFromApprovedRejected(int formId, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formApprovedWf = db.Form_Workflow.Where(x => x.FormId == formId && x.FormType == formType 
                                            && (x.WorkflowStatus == Common.FormStatus.Approved || x.WorkflowStatus == Common.FormStatus.Rejected));

                    if (formApprovedWf.Count() > 0)
                    {
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

        public static void SubmitForApprovalWorkflow(int formId, string formType, string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formWorkflow = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        WorkflowStatus = Common.FormStatus.PendingApproval,
                        WorkflowNotes = notes,
                        RecordedDate = DateTime.Now
                    };

                    if (formType == Common.FormType.AMSD_IF)
                    {
                        var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                        formWorkflow.RequestBy = form.PreparedBy;
                        formWorkflow.RequestTo = form.ApprovedBy;
                    }
                    else if (formType == Common.FormType.FID_TREASURY)
                    {
                        var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);
                        formWorkflow.RequestBy = form.PreparedBy;
                        formWorkflow.RequestTo = form.ApprovedBy;
                    }
                    else
                    {
                        var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId && x.FormType == formType);
                        formWorkflow.RequestBy = form.PreparedBy;
                        formWorkflow.RequestTo = form.ApprovedBy;
                    }
                    
                    db.Form_Workflow.Add(formWorkflow);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void ApprovalResponse(int formId, string formStatus, string notes, string formType, string preparedBy, string approvedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newFormWorkflow = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        RequestBy = approvedBy,
                        RecordedDate = DateTime.Now,
                        RequestTo = preparedBy,
                        WorkflowStatus = formStatus,
                        WorkflowNotes = notes
                    };

                    db.Form_Workflow.Add(newFormWorkflow);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void ReassignWorkflow(int formId, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formWorkflow = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        WorkflowStatus = Common.FormStatus.PendingApproval,
                        WorkflowNotes = "Reassign Approval",
                        RecordedDate = DateTime.Now
                    };

                    if (formType == Common.FormType.AMSD_IF)
                    {
                        var form = db.AMSD_IF.FirstOrDefault(x => x.Id == formId);
                        formWorkflow.RequestBy = form.PreparedBy;
                        formWorkflow.RequestTo = form.ApprovedBy;
                    }
                    else if (formType == Common.FormType.FID_TREASURY)
                    {
                        var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);
                        formWorkflow.RequestBy = form.PreparedBy;
                        formWorkflow.RequestTo = form.ApprovedBy;
                    }
                    else
                    {
                        var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId && x.FormType == formType);
                        formWorkflow.RequestBy = form.PreparedBy;
                        formWorkflow.RequestTo = form.ApprovedBy;
                    }

                    db.Form_Workflow.Add(formWorkflow);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static List<Form_Workflow> GetWorkflow(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Form_Workflow.Where(x => x.FormId == formId).ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public static Form_Workflow GetLatestWorkflow(kashflowDBEntities db, int formId, string formType)
        {
            var result = db.Form_Workflow
                            .Where(x => x.FormId == formId && x.FormType == formType)
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

            if (result != null && result.WorkflowStatus == Common.FormStatus.PendingApproval)
            {
                return null;
            }
            else
            {
                return result;
            }
        }
    }
}
