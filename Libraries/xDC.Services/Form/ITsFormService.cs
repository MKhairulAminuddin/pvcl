using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;

namespace xDC.Services.Form
{
    public interface ITsFormService
    {
        TsLandingPage GetLandingPageData(string currentUser);
        List<TsConsolidatedPage> GetConsolidatedPage(long settlementDateEpoch, string currency);
        List<TsHomeGrid1> GetTsHomeGrid1(string currentUser);
        List<TsHomeGrid2> GetTsHomeGrid2(string currentUser);
        List<ISSD_TradeSettlement> GetTsItemsGrid(int formId, string instrumentType);

        List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, DateTime settlementDate, string currency);
        List<TsOpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency);
        TS_TotalFlow GetTotalFlow(kashflowDBEntities db, List<int> formId, DateTime settlementDate, string currency);

        // TODO: Refactor this
        bool EditFormRules(string formStatus, string approvedBy, string currentUser, out string errorMessage);
        double GetTotalInflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category);
        double GetTotalOutflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category);
        List<TsOpeningBalance> OpeningBalanceSummary(long submissionDateEpoch = 0);
        List<TsFormSummary> TsFormSummaryList(long submissionDateEpoch = 0);




        bool CreateForm(TsCreateNewFormRequest form, string currentUser, out int createdFormId);
        bool EditForm(TsCreateNewFormRequest req, string currentUser);
        bool DeleteForm(int formId, string currentUser);
        bool FormApproval(TsFormApprovalRequest req, string currentUser);
    }
}
