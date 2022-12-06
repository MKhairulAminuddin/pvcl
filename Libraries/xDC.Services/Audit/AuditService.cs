using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.WebApi.Administration;
using xDC.Domain.WebApi.Audit;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.Audit
{
    public class AuditService: IAuditService
    {
        #region Fields

        #endregion

        #region Ctor


        #endregion

        #region User Access Audit

        public List<Audit_UserAccess> Get_UAA()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Audit_UserAccess.ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }


        #endregion

        #region Audit form

        private void FA_Add(Audit_Form obj)
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

        public List<Audit_Form> FA(DateTime fromDate, DateTime toDate)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Audit_Form
                         .Where(x =>
                            (DbFunctions.TruncateTime(x.ModifiedOn) >= DbFunctions.TruncateTime(fromDate) &&
                             DbFunctions.TruncateTime(x.ModifiedOn) <= DbFunctions.TruncateTime(toDate))).ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public void FA_Add(int formId, string formType, DateTime? formDate, string actionType, string modifiedBy, string remarks = null)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var obj = new Audit_Form()
                    {
                        FormId = formId,
                        FormType = formType,
                        FormDate = formDate,
                        ActionType = actionType,

                        ModifiedBy = modifiedBy,
                        ModifiedOn = DateTime.Now,

                        Remarks = remarks
                    };

                    FA_Add(obj);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public void FA_EditRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter, string columnName)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.Modify,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Modify existing record. Column {columnName}",
                ValueBefore = valueBefore,
                ValueAfter = valueAfter

            };

            FA_Add(obj);
        }

        public void FA_AddRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueAfter)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.Modify,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Add a record",
                ValueAfter = valueAfter
            };

            FA_Add(obj);
        }

        public void FA_RemoveRow(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.Modify,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Remove a record",
                ValueBefore = valueBefore
            };

            FA_Add(obj);
        }

        public void FA_Approval(int formId, string formType, string approvalStatus, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = approvalStatus == Common.FormStatus.Approved ? Common.FormActionType.Approve : Common.FormActionType.Reject,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"{formType} form {approvalStatus}"
            };

            FA_Add(obj);
        }

        public void FA_AssignApprover(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.RequestApproval,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Assign form's approver",
                ValueBefore = valueBefore,
                ValueAfter = valueAfter
            };

            FA_Add(obj);
        }

        public void FA_ReassignApprover(int formId, string formType, DateTime? formDate, string modifiedBy, string valueBefore, string valueAfter)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.Reassign,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Reassign form's approver",
                ValueBefore = valueBefore,
                ValueAfter = valueAfter
            };

            FA_Add(obj);
        }

        public void FA_Resubmission(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.Resubmit,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Resubmit Form"
            };

            FA_Add(obj);
        }

        public void FA_AdminEdit(int formId, string formType, DateTime? formDate, string modifiedBy)
        {
            var obj = new Audit_Form()
            {
                FormId = formId,
                FormType = formType,
                FormDate = formDate,
                ActionType = Common.FormActionType.AdminEdit,

                ModifiedBy = modifiedBy,
                ModifiedOn = DateTime.Now,

                Remarks = $"Form editted by Power User/ Admin"
            };

            FA_Add(obj);
        }

        #endregion

        #region User Management Audit

        public List<Audit_UserManagement> Get_UMA()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Audit_UserManagement.ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public void Capture_UMA(string activityType, string remarks, string userAccount, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var recordedItem = new Audit_UserManagement()
                    {
                        Activity = activityType,
                        Remarks = remarks,
                        UserAccount = userAccount,
                        PerformedBy = performedBy,
                        RecordedDate = DateTime.Now
                    };

                    db.Audit_UserManagement.Add(recordedItem);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        #endregion

        #region Role Management Audit

        public List<Audit_RoleManagement> Get_RMA()
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.Audit_RoleManagement.ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public void Capture_RMA(string activityType, string remarks, string role, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var recordedItem = new Audit_RoleManagement()
                    {
                        Activity = activityType,
                        Remarks = remarks,
                        Role = role,
                        PerformedBy = performedBy,
                        RecordedDate = DateTime.Now
                    };

                    db.Audit_RoleManagement.Add(recordedItem);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        #endregion
    }
}
