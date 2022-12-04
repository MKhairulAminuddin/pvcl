using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.FID.FcaTaggingForm;
using xDC.Domain.WebApi.Form_FcaTagging;
using xDC.Infrastructure.Application;

namespace xDC.Services.Form
{
    public interface IFcaTaggingFormService
    {
        List<SummaryRes> DxGrid_GetSummaryForIssdView(out bool reqStatus);
        List<SummaryRes> DxGrid_GetSummaryForFidView(out bool reqStatus);
        List<DetailRes> DxGrid_GetDetailForIssdView(long settlementDateEpoch, string currency, out bool reqStatus);
        List<DetailRes> DxGrid_GetDetailForFidView(long settlementDateEpoch, string currency, out bool reqStatus);
        List<ISSD_TradeSettlement> DxGrid_GetIndividualGridData(string tradeType, long settlementDateEpoch, string currency, out bool reqStatus);
        List<FcaBankAccountRes> List_GetFcaBankAccount(out bool reqStatus);
        FcaAccountAssignmentFormVM Page_FcaTaggingForm(long settlementDateEpoch, string currency, out bool reqStatus);
    }
}
