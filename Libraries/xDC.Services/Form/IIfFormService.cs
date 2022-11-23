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
        LandingPage GetLandingPageData(string currentUser);
        List<IfHomeGrid1> GetHomeGrid(string currentUser);
        IQueryable<AMSD_IF_Item> GetFormItems(int formId);
        InflowFundForm GetPageViewData(int formId, string currentUser);
        List<IfFormSummaryList> IfFormSummaryList(long submissionDateEpoch = 0);
        List<IfAmountSummary> IfAmountSummary(long submissionDateepoch = 0);


        // form
        bool CreateForm(AMSD_IF form, List<AMSD_IF_Item> formItems, string notes, out int createdFormId);
        bool WithdrawForm(int formId, string performedBy, string formType);
    }
}
