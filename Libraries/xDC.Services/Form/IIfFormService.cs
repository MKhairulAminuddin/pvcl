using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.AMSD.InflowFundForm;
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
        IQueryable<AMSD_IF_Item> GetFormItems(int formId);
        
        List<IfFormSummaryList> IfFormSummaryList(long submissionDateEpoch = 0);
        List<IfAmountSummary> IfAmountSummary(long submissionDateepoch = 0);


        // form
        int CreateForm(IfFormPage input, string currentUser);
        int EditForm(int formId, IfFormPage input, string currentUser);
        int DeleteForm(int formId, string currentUser);
        bool WithdrawForm(int formId, string performedBy, string formType);
    }
}
