using System;
using System.Collections.Generic;
using System.IO;
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
        List<TsOpeningBalance> GetOpeningBalance(DateTime settlementDate, string currency);
        TS_TotalFlow GetTotalFlow(kashflowDBEntities db, List<int> formId, DateTime settlementDate, string currency);

        // TODO: Refactor this
        double GetTotalInflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category);
        double GetTotalOutflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category);
        List<TsOpeningBalance> OpeningBalanceSummary(long submissionDateEpoch = 0);
        List<TsFormSummary> TsFormSummaryList(long submissionDateEpoch = 0);




        bool CreateForm(TsCreateNewFormRequest form, string currentUser, out int createdFormId);
        bool EditForm(TsCreateNewFormRequest req, string currentUser);
        bool DeleteForm(int formId, string currentUser);
        bool FormApproval(TsFormApprovalRequest req, string currentUser);
        TsForm ViewConsolidatedForm(string settlementDateEpoch, string currency);
        TsForm ViewForm(int formId, string currentUser);
        TsForm ViewEditForm(int formId, string currentUser);
        TsForm ViewNewForm(string formType, string currentUser);

        bool WithdrawForm(int formId, string performedBy, string formType);
        bool ReassignApproverForm(int formId, string newApprover, string currentUser);

        string GenExportFormId(int formId, string currentUser, bool isExportToExcel);
        string GenExportConsolidatedFormId(DateTime settlementDate, string currency, string currentUser, bool isExportToExcel);
        FileStream GetGeneratedForm(string generatedFileId);
    }
}
