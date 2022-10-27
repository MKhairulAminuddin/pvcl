using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.WebApi.Forms;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using static xDC.Utils.Common;

namespace xDC.Services.App
{
    public static class TreasuryFormService
    {
        #region Grid

        public static List<TreasuryHomeGrid1> GetTsHomeGrid1(string currentUser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var todayDate = DateTime.Now.Date;

                    var tsForms = db.FID_Treasury;
                    var resultVM = new List<TreasuryHomeGrid1>();

                    foreach (var item in tsForms)
                    {
                        resultVM.Add(new TreasuryHomeGrid1
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            ValueDate = item.ValueDate,
                            Currency = item.Currency,
                            FormStatus = item.FormStatus,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,

                            EnableEdit = FormService.EnableEdit(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnableDelete = FormService.EnableDelete(item.FormStatus, item.PreparedBy, item.ApprovedBy, currentUser),
                            EnablePrint = FormService.EnablePrint(currentUser, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Download),
                            EnableRetractSubmission = FormService.EnableRetractSubmission(currentUser, item.PreparedBy, item.FormStatus, PermissionKey.ISSD_TradeSettlementForm_Edit),

                            IsRejected = (currentUser == item.PreparedBy && item.FormStatus == Common.FormStatus.Rejected),
                            IsPendingMyApproval = (currentUser == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),
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


        public static List<EDW_FID_List> List_Issuer(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var issuerList = db.EDW_FID_List.Where(x => x.Type == "ISSUER").ToList();
            return issuerList;
        }

        public static List<string> List_FcaBankAccount(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var fcaBankAccounts = db.Config_FcaBankAccount.Select(x => x.AccountName3).ToList();
            return fcaBankAccounts;
        }

        public static List<EDW_FID_List> List_CounterParty(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var counterPartyList = db.EDW_FID_List.Where(x => x.Type == "COUNTERPARTY").ToList();
            return counterPartyList;
        }

    }
}
