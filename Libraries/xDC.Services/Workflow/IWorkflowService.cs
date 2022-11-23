using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;

namespace xDC.Services.Workflow
{
    public interface IWorkflowService
    {
        bool Initiate(int formId, string formType, string preparer, string approver, string notes);
        bool Approval(int formId, string formType, string preparer, string approver, string notes, string formStatus);
        void Reassign(int formId, string formType, string preparer, string newApprover);
        void Withdraw(int formId, string formType);
        IQueryable<Form_Workflow> Info(int formId, string formType);
    }
}
