using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.WebApi.Audit;
using xDC.Infrastructure.Application;

namespace xDC.Services.Audit
{
    public interface IAuditService
    {
        // Form Audit
        void FA_Add(int formId, string formType, DateTime? formDate, string actionType, string modifiedBy, string remarks = null);
        void FA_EditRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter, string columnName);
        void FA_AddRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueAfter);
        void FA_RemoveRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore);
        void FA_Approval(int formId, string formType, string approvalStatus, DateTime? formDate, string modifiedBy);
        void FA_AssignApprover(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter);
        void FA_ReassignApprover(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter);
        void FA_Resubmission(int formId, string formType, DateTime? formDate, string modifiedBy);
        void FA_AdminEdit(int formId, string formType, DateTime? formDate, string modifiedBy);


        // User Management Audit

        List<Audit_UserManagement> Get_UMA(out bool status, AuditReq req = null);
        void Capture_UMA(string activityType, string remarks, string userAccount, string performedBy);


        // Role Management Audit

        List<Audit_RoleManagement> Get_RMA(out bool status, AuditReq req = null);
        void Capture_RMA(string activityType, string remarks, string role, string performedBy);



    }
}
