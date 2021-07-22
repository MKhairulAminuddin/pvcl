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
    public class WorkflowService
    {
        public void UpdateWorkflow(Form_Workflow input)
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

        public void SubmitForApprovalWorkflow(int formId)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.Form_Header.FirstOrDefault(x => x.Id == formId);

                    var formWorkflow = new Form_Workflow()
                    {
                        FormId = formId,
                        RequestBy = form.PreparedBy,
                        StartDate = form.PreparedDate,
                        RequestTo = form.ApprovedBy,
                        WorkflowStatus = Common.FormStatusMapping(2)
                    };

                    db.Form_Workflow.Add(formWorkflow);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void ApprovalFeedbackWorkflow(int formId, bool isApproved,string notes)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var currentFormWorkflow = db.Form_Workflow
                        .Where(x => x.FormId == formId)
                        .OrderByDescending(x => new { x.StartDate, x.Id}).FirstOrDefault();
                    currentFormWorkflow.EndDate = DateTime.Now;

                    var newFormWorkflow = new Form_Workflow()
                    {
                        FormId = formId,
                        RequestBy = currentFormWorkflow.RequestTo,
                        StartDate = DateTime.Now,
                        RequestTo = currentFormWorkflow.RequestBy,
                        EndDate = DateTime.Now,
                        WorkflowStatus = isApproved ? Common.FormStatusMapping(3) : Common.FormStatusMapping(4),
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

        public List<Form_Workflow> GetWorkflow(int formId)
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
    }
}
