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
    public class AuditService
    {
        #region Audit form

        public void AuditForm_Insert(Audit_Form obj)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var item = db.Audit_Form.Add(obj);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void AuditForm_Create(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Create,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Created an {formType} form"
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_RequestApproval(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.RequestApproval,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now.AddMilliseconds(100),

                Remarks = $"Request Approval for {formType} form"
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_Delete(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Delete,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Delete {formType} form"
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_EditRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter, string columnName)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Modify,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Modify existing record. Column {columnName}",
                ValueBefore = valueBefore,
                ValueAfter = valueAfter

            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_AddRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueAfter)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Modify,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Add a record",
                ValueAfter = valueAfter
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_RemoveRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Modify,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Remove a record",
                ValueBefore = valueBefore
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_Approval(int formId, string formType, string approvalStatus, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = (approvalStatus == Common.FormStatus.Approved) ? Common.ActionType.Approve : Common.ActionType.Reject,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"{formType} form {approvalStatus}"
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_ReassignApprover(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Reassign,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Reassign form's approver",
                ValueBefore = valueBefore,
                ValueAfter = valueAfter
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_Resubmission(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.Resubmit,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Resubmit Form"
            };

            AuditForm_Insert(obj);
        }

        public void AuditForm_AdminEdit(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.ActionType.AdminEdit,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Form editted by Power User/ Admin"
            };

            AuditForm_Insert(obj);
        }

        #endregion

        #region Audit 10AM Deal Cut Off Closing Balance


        #endregion

    }
}
