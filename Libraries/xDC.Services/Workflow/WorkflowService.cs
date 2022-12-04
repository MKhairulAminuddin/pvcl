using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.Workflow
{
    public class WorkflowService: IWorkflowService
    {
        // instantiate workflow
        // submit for approval
        // approval feedback
        // reassign approver
        // retract submission
        // get info

        #region Fields

        private readonly IXDcLogger _logger;


        private readonly string Wf_PendingApproval = "Pending Approval";
        private readonly string Wf_ReassignApprover = "Reassign Approver";
        private readonly string Wf_WithdrawSubmission = "Withdraw Submission";
        private readonly string Wf_Approved = "Approved";
        private readonly string Wf_Rejected = "Rejected";

        #endregion

        #region Ctor

        public WorkflowService(IXDcLogger logger)
        {
            _logger = logger;
        }

        #endregion

        public bool Initiate(int formId, string formType, string preparer, string approver, string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var wf = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        RequestBy = preparer,
                        RequestTo = approver,
                        RecordedDate = DateTime.Now,
                        WorkflowNotes = notes,
                        WorkflowStatus = Wf_PendingApproval
                    };

                    db.Form_Workflow.Add(wf);
                    var created = db.SaveChanges();
                    return created > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }

        public bool Approval(int formId, string formType, string preparer, string approver, string notes, string formStatus)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    bool approvalStatus = formStatus == FormStatus.Approved;

                    var wf = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        RequestBy = preparer,
                        RequestTo = approver,
                        RecordedDate = DateTime.Now,
                        WorkflowNotes = notes,
                        WorkflowStatus = approvalStatus ? Wf_Approved : Wf_Rejected
                    };

                    db.Form_Workflow.Add(wf);
                    var created = db.SaveChanges();
                    return created > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }

        public void Reassign(int formId, string formType, string preparer, string newApprover)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var wf = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        RequestBy = preparer,
                        RequestTo = newApprover,
                        RecordedDate = DateTime.Now,
                        WorkflowStatus = Wf_ReassignApprover
                    };

                    db.Form_Workflow.Add(wf);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        public void Withdraw(int formId, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var wf = new Form_Workflow()
                    {
                        FormId = formId,
                        FormType = formType,
                        RecordedDate = DateTime.Now,
                        WorkflowStatus = Wf_WithdrawSubmission
                    };

                    db.Form_Workflow.Add(wf);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        public List<Form_Workflow> Info(int formId, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var wfList = db.Form_Workflow.Where(x => x.FormId == formId && x.FormType == formType).OrderByDescending(x => x.RecordedDate);
                    return wfList.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return null;
            }
        }

        public string LatestApprovalNotes(int formId, string formType)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formWf = db.Form_Workflow
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault(x =>
                                x.FormId == formId && x.FormType == formType &&
                                (x.WorkflowStatus == Common.FormStatus.Approved ||
                                 x.WorkflowStatus == Common.FormStatus.Rejected));

                    if (formWf != null)
                    {
                        return formWf.WorkflowNotes;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return string.Empty;
            }
        }
    }
}
