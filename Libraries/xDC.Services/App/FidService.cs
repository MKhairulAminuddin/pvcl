using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class TreasuryFormService
    {

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

        public static List<string> List_FcaBankAccount(kashflowDBEntities db, string currency)
        {
            var fcaBankAccounts = db.Config_FcaBankAccount
                .Where(x => x.Currency == currency)
                .Select(x => x.AccountName3)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return fcaBankAccounts;
        }

        public static List<EDW_FID_List> List_CounterParty(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var counterPartyList = db.EDW_FID_List.Where(x => x.Type == "COUNTERPARTY").ToList();
            return counterPartyList;
        }

        public static List<EDW_FID_List> List_CounterPartyIssuerCombined(kashflowDBEntities db)
        {
            // check for same date and same currency exist
            var counterPartyList = db.EDW_FID_List.Select(x => new EDW_FID_List
            {
                Name = x.Name,
            }).Distinct().ToList();
            return counterPartyList;
        }

        public static bool RetractFormSubmission(int formId, string performedBy)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var form = db.FID_Treasury.FirstOrDefault(x => x.Id == formId);

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
    }
}
