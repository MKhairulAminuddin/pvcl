using System;
using System.Collections.Generic;
using System.IO;
using xDC.Domain.Web.Application;
using xDC.Domain.Web.FID.TreasuryForm;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Domain.WebApi.Forms;
using xDC.Domain.WebApi.Forms.Treasury;
using xDC.Infrastructure.Application;
using xDC.Utils;

namespace xDC.Services.Form
{
    public interface ITreasuryFormService
    {
        List<TreasuryHomeGrid1> GetTsHomeGrid1(string currentUser);
        TreasuryFormPage GetViewPageData(int formId, string currentUser);
        TreasuryFormPage GetEditPageData(int formId, string currentUser);
        List<TreasuryFormSummary> TreasuryFormSummaryList(long submissionDateEpoch = 0);
        List<EDW_FID_List> List_Issuer();
        List<string> List_FcaBankAccount();
        List<EDW_FID_List> List_CounterParty();

        List<TDeposit> DepositFromEdw(DateTime tradeDate, string currency);
        List<TMmi> MmiFromEdw(DateTime tradeDate, string currency);

        string GenExportFormId(int formId, string currentUser, bool isExportToExcel);
        ExportedFile GetGeneratedForm(string generatedFileId);

        #region Form Action
        int CreateForm(TForm input, string currentUser);
        int EditForm(int formId, TForm input, string currentUser);
        int DeleteForm(int formId, string currentUser);
        int ApproveForm(TreasuryFormApprovingReq input, string currentUser);
        bool WithdrawForm(int formId, string performedBy, string formType);
        bool ReassignApproverForm(int formId, string newApprover, string currentUser);

        #endregion

        #region Grid

        List<FID_Treasury_Deposit> GetDepositGrid(int formId, Cashflow cashflow);
        List<FID_Treasury_MMI> GetMmiGrid(int formId, Cashflow cashflow);

        #endregion
    }
}
