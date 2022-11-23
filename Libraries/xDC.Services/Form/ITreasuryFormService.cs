using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.FID.TreasuryForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms;
using xDC.Infrastructure.Application;

namespace xDC.Services.Form
{
    public interface ITreasuryFormService
    {
        List<TreasuryHomeGrid1> GetTsHomeGrid1(string currentUser);
        TreasuryFormPage GetViewPageData(int formId, string currentUser);
        TreasuryFormPage GetEditPageData(int formId, string currentUser);
        List<TreasuryFormSummary> TreasuryFormSummaryList(long submissionDateEpoch = 0);
        List<EDW_FID_List> List_Issuer(kashflowDBEntities db);
        List<string> List_FcaBankAccount(kashflowDBEntities db);
        List<EDW_FID_List> List_CounterParty(kashflowDBEntities db);
    }
}
