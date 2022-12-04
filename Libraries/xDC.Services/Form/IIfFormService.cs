using System.Collections.Generic;
using System.IO;
using System.Linq;
using xDC.Domain.Web.AMSD.InflowFundForm;
using xDC.Domain.Web.Application;
using xDC.Domain.WebApi.Forms.InflowFund;
using xDC.Domain.WebApi.Forms.TradeSettlement;
using xDC.Infrastructure.Application;

namespace xDC.Services.Form
{
    public interface IIfFormService
    {
        // read data
        LandingPage GetLandingPage(string currentUser);
        InflowFundForm ViewFormPage(int formId, string currentUser);
        InflowFundForm EditFormPage(int formId, string currentUser);

        List<IfHomeGrid1> GetHomeGrid(string currentUser);
        List<AMSD_IF_Item> GetFormItems(int formId);
        
        List<IfFormSummaryList> IfFormSummaryList(long submissionDateEpoch = 0);
        List<IfAmountSummary> IfAmountSummary(long submissionDateepoch = 0);


        // form
        /// <summary>
        /// Create Inflow Fund form.
        /// </summary>
        /// <param name="input">IF Form data</param>
        /// <param name="currentUser">User who perform the action</param>
        /// <returns>Created form ID</returns>
        int CreateForm(IfFormPage input, string currentUser);
        int EditForm(int formId, IfFormPage input, string currentUser);
        int DeleteForm(int formId, string currentUser);

        /// <summary>
        /// Form Approval for Inflow Fund Form. Either Approve or Reject. Otherwise consider not valid.
        /// </summary>
        /// <param name="input">Form updated data</param>
        /// <param name="currentUser">Current user performing the action</param>
        /// <returns>Return Form ID if successful approval process.</returns>
        int ApproveForm(IfFormApprovingReq input, string currentUser);
        /// <summary>
        /// Retract/Withdraw submitted form.
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="performedBy"></param>
        /// <param name="formType"></param>
        /// <returns>Status of the form withdrawal. Either true or false. </returns>
        bool WithdrawForm(int formId, string performedBy, string formType);
        bool ReassignApproverForm(int formId, string newApprover, string currentUser);

        string GenExportFormId(int formId, string currentUser, bool isExportToExcel);
        ExportedFile GetGeneratedForm(string generatedFileId);
    }
}
